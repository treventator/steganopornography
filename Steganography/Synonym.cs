// ============================================================
// Synonym.cs — ฟอร์มย่อย (Sub-form) สำหรับเทคนิค Synonym (คำพ้องความหมาย)
// ============================================================
// ฟอร์มนี้ทำหน้าที่ให้ผู้ใช้เลือกกลุ่มคำพ้องความหมาย (64 กลุ่ม)
// แล้วใช้กลุ่มเหล่านั้นในการฝัง (Embed) หรือถอด (Extract) ข้อมูลลับ
//
// หลักการ Synonym:
//   - มี 64 กลุ่มคำพ้อง แต่ละกลุ่มมี 2 คำ เช่น "กล่าว"↔"พูด", "ทำ"↔"กระทำ"
//   - ในข้อความ covertext ถ้าพบคำที่อยู่ในกลุ่ม:
//     → คำ index 0 ในกลุ่ม = bit 0
//     → คำ index 1 ในกลุ่ม = bit 1
//   - สแกนข้อความจากซ้ายไปขวาแบบ greedy (ไม่ซ้อนทับ)
//   - ข้อดี: ข้อความที่ได้ยังคงอ่านรู้เรื่อง เพราะเป็นคำพ้องที่สื่อความหมายเดียวกัน
//
// ข้อเปรียบเทียบกับ Misspelling:
//   - Misspelling ใช้คำสะกดผิด (อาจทำให้ข้อความดูแปลก)
//   - Synonym ใช้คำพ้องที่ถูกต้องทั้งคู่ (ข้อความดูเป็นธรรมชาติกว่า)
//   - ทั้งสองใช้ CheckedListBox เหมือนกันเพราะมีจำนวนมาก (65 คู่ vs 64 กลุ่ม)
//
// Pattern การสื่อสารกับ Main Form (SteganographyForm):
//   1. Main form สร้าง instance: var form = new Synonym();
//   2. Main form ตั้ง Properties: IsEmbedMode, CipherText, CoverText หรือ SteganotextInput
//   3. Main form เรียก form.ShowDialog() → ฟอร์มเปิดแบบ modal
//   4. ผู้ใช้เลือกกลุ่มคำจาก CheckedListBox แล้วกด OK
//   5. ฟอร์มประมวลผล → เก็บผลลัพธ์ใน ResultText → ปิดตัวเอง → DialogResult.OK
//   6. Main form อ่าน form.ResultText ไปใช้ต่อ
// ============================================================

using System;                       // นำเข้า namespace หลักของ .NET สำหรับ Exception, EventArgs ฯลฯ
using System.Collections.Generic;   // นำเข้า namespace สำหรับ List<T> ที่ใช้เก็บรายการ index ของกลุ่มคำที่เลือก
using System.Windows.Forms;         // นำเข้า namespace สำหรับ Windows Forms UI เช่น Form, CheckedListBox, MessageBox

namespace Steganography             // ประกาศ namespace ของโปรเจค — ทุกคลาสอยู่ใน namespace เดียวกัน
{
    /// <summary>
    /// หน้าต่างเลือกกลุ่มคำพ้องความหมาย สำหรับเทคนิค Synonym
    /// ใช้ทั้งฝั่ง Embed และ Extract
    /// </summary>
    public partial class Synonym : Form   // ประกาศคลาส Synonym สืบทอดจาก Form (Windows Forms)
    {                                      // "partial" = แบ่งเป็น 2 ไฟล์: Synonym.cs (logic) + Synonym.Designer.cs (UI layout + items)
        // ============================================================
        //  Properties: ช่องทางสื่อสารข้อมูลกับ Main Form
        // ============================================================
        //  Properties pattern เหมือนกับทุก sub-form ในโปรเจค:
        //  Main form set ค่า → ShowDialog() → sub-form อ่าน → ประมวลผล → เขียน ResultText → ปิด
        //  Main form ตรวจสอบ DialogResult.OK แล้วอ่าน ResultText

        /// <summary>โหมดการทำงาน: true = Embed, false = Extract</summary>
        public bool IsEmbedMode { get; set; } = true;   // true = ฝังข้อมูล (แทนคำพ้องใน covertext ตาม bits)
                                                          // false = ถอดข้อมูล (อ่านว่าแต่ละตำแหน่งใช้คำไหนในกลุ่ม)

        /// <summary>Ciphertext (Base64) ที่จะฝัง</summary>
        public string CipherText { get; set; } = "";     // ข้อความเข้ารหัส AES (Base64) ที่ต้องการซ่อน — ใช้ตอน Embed เท่านั้น

        /// <summary>Covertext สำหรับฝัง</summary>
        public string CoverText { get; set; } = "";      // ข้อความปกติภาษาไทยที่จะแทนคำพ้อง — ใช้ตอน Embed เท่านั้น

        /// <summary>Steganotext สำหรับถอด</summary>
        public string SteganotextInput { get; set; } = ""; // ข้อความที่มีคำพ้องซ่อนอยู่ — ใช้ตอน Extract เท่านั้น

        /// <summary>ผลลัพธ์หลัง OK</summary>
        public string ResultText { get; private set; } = ""; // ผลลัพธ์หลังประมวลผล — private set ป้องกันการเขียนจากภายนอก
                                                              // Embed → Steganotext (ข้อความที่แทนคำพ้องแล้ว)
                                                              // Extract → Ciphertext (Base64) ที่ถอดออกมาจากรูปแบบคำพ้อง

        // ============================================================
        //  Constructor — จุดเริ่มต้นเมื่อสร้าง object ของฟอร์ม
        // ============================================================

        public Synonym()           // Constructor — ถูกเรียกเมื่อ main form ทำ new Synonym()
        {
            InitializeComponent(); // เรียกเมธอดใน .Designer.cs เพื่อสร้าง UI controls ทั้งหมด
                                   // รวมถึง checkedListBox1 ที่มี 64 กลุ่มคำพ้อง, ปุ่ม OK/Cancel/SelectAll/ClearAll

            Load += (s, ev) =>     // ผูก event handler สำหรับ Form.Load ด้วย lambda expression
                                   // Load event จะ fire ก่อนฟอร์มแสดงผลครั้งแรก
                                   // ใช้ lambda แทน method แยกเพราะ logic สั้นแค่บรรทัดเดียว

                Text = IsEmbedMode // ตั้ง title bar ของฟอร์มตามโหมดการทำงาน
                    ? "Synonym — เลือกกลุ่มคำสำหรับฝัง"    // โหมด Embed → บอกว่ากำลัง "ฝัง"
                    : "Synonym — เลือกกลุ่มคำสำหรับถอด";   // โหมด Extract → บอกว่ากำลัง "ถอด"
        }

        // ============================================================
        //  Event Handler: ปุ่ม OK — ผู้ใช้กดยืนยันการเลือก
        // ============================================================

        private void BtnOK_Click(object sender, EventArgs e)   // event handler สำหรับปุ่ม OK (ผูกใน Designer.cs)
        {
            ExecuteSynonym();   // เรียกเมธอดหลักที่ทำการฝัง/ถอดจริง (แยกออกมาเพื่อให้โค้ดเป็นระเบียบ)
        }

        // ============================================================
        //  Event Handler: ปุ่ม Cancel — ผู้ใช้กดยกเลิก
        // ============================================================

        private void BtnCancel_Click(object sender, EventArgs e)   // event handler สำหรับปุ่ม Cancel
        {
            DialogResult = DialogResult.Cancel;   // ตั้ง DialogResult เป็น Cancel → main form จะรู้ว่าผู้ใช้ยกเลิก
            Close();                              // ปิดฟอร์ม → กลับไปที่ main form โดยไม่เปลี่ยนแปลงอะไร
        }

        // ============================================================
        //  Event Handler: ปุ่ม "เลือกทั้งหมด" — ติ๊กทุกกลุ่มคำพ้องใน list
        // ============================================================

        private void BtnSelectAll_Click(object sender, EventArgs e)   // event handler สำหรับปุ่ม Select All
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)     // วนลูปทุก item ใน CheckedListBox (0 ถึง 63 = 64 กลุ่ม)
                checkedListBox1.SetItemChecked(i, true);               // ติ๊กเลือก item ที่ index i → true = checked
        }                                                              // ประโยชน์: ผู้ใช้ไม่ต้องติ๊กทีละกลุ่มทั้ง 64 กลุ่ม

        // ============================================================
        //  Event Handler: ปุ่ม "ล้างทั้งหมด" — เอาติ๊กออกทุกกลุ่มคำพ้อง
        // ============================================================

        private void BtnClearAll_Click(object sender, EventArgs e)    // event handler สำหรับปุ่ม Clear All
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)     // วนลูปทุก item ใน CheckedListBox
                checkedListBox1.SetItemChecked(i, false);              // เอาติ๊กออก item ที่ index i → false = unchecked
        }

        // ============================================================
        //  ExecuteSynonym() — เมธอดหลักที่ทำการฝังหรือถอดข้อมูล
        // ============================================================
        //  ขั้นตอนการทำงาน:
        //    1. เรียก GetActiveGroupIndices() เพื่อดูว่าผู้ใช้เลือกกลุ่มคำพ้องไหนบ้าง
        //    2. ตรวจสอบว่าเลือกอย่างน้อย 1 กลุ่ม (ถ้าไม่เลือก → แจ้งเตือน)
        //    3. เรียก SteganographyEngine.SynonymEmbed() หรือ SynonymExtract()
        //    4. เก็บผลลัพธ์ใน ResultText → ตั้ง DialogResult.OK → ปิดฟอร์ม

        public void ExecuteSynonym()   // ประกาศ public เพื่อให้ main form เรียกได้โดยตรงถ้าต้องการ
        {
            var activeGroups = GetActiveGroupIndices();   // ดึงรายการ index ของกลุ่มคำพ้องที่ถูกติ๊กเลือก
                                                          // เช่น ถ้าเลือกกลุ่ม "กล่าว/พูด" (0) กับ "ทำ/กระทำ" (1) จะได้ List {0, 1}
            if (activeGroups.Count == 0)   // ตรวจสอบว่าเลือกอย่างน้อย 1 กลุ่ม — ถ้าไม่มีเลยจะ Embed/Extract ไม่ได้
            {
                MessageBox.Show("กรุณาเลือกกลุ่มคำพ้องอย่างน้อย 1 กลุ่ม",   // แสดง popup เตือนผู้ใช้
                    "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;   // ออกจากเมธอดทันที — ฟอร์มยังเปิดอยู่ ผู้ใช้เลือกใหม่ได้
            }

            try   // ใช้ try-catch เพราะ Engine อาจ throw exception เช่น ความจุไม่พอ หรือ covertext ไม่มีคำพ้องที่เลือก
            {
                if (IsEmbedMode)   // ถ้าเป็นโหมดฝัง (Embed)
                    ResultText = SteganographyEngine.SynonymEmbed(CoverText, CipherText, activeGroups);
                    // เรียก SynonymEmbed ใน SteganographyEngine:
                    //   - CoverText = ข้อความปกติที่จะซ่อนข้อมูล (ต้องมีคำพ้องจากกลุ่มที่เลือกอยู่ในเนื้อหา)
                    //   - CipherText = ข้อความเข้ารหัส (Base64) ที่จะแปลงเป็น bits แล้วฝัง
                    //   - activeGroups = รายการ index ของกลุ่มคำพ้องที่เลือก
                    //   - Engine จะสแกน covertext หาคำที่ตรงกับกลุ่มที่เลือก
                    //     แล้วแทนที่ด้วยคำ index 0 (bit 0) หรือ index 1 (bit 1) ตาม payload
                    //   - คืนค่า Steganotext → เก็บใน ResultText
                else   // ถ้าเป็นโหมดถอด (Extract)
                    ResultText = SteganographyEngine.SynonymExtract(SteganotextInput, activeGroups);
                    // เรียก SynonymExtract ใน SteganographyEngine:
                    //   - SteganotextInput = ข้อความที่มีคำพ้องซ่อนอยู่
                    //   - activeGroups = ต้องเลือกกลุ่มเดียวกับตอนฝัง มิฉะนั้นถอดผิด
                    //   - Engine จะสแกนอ่านว่าแต่ละตำแหน่งใช้คำ index 0 หรือ 1 ของกลุ่ม
                    //   - คืนค่า Ciphertext (Base64) → main form นำไปถอดรหัส AES ต่อ

                DialogResult = DialogResult.OK;   // ตั้ง DialogResult เป็น OK → บอก main form ว่าทำงานสำเร็จ
                Close();   // ปิดฟอร์ม → control กลับไป main form → main form อ่าน ResultText
            }
            catch (Exception ex)   // จับ exception ที่อาจเกิดขึ้นระหว่างการ Embed/Extract
            {
                MessageBox.Show(ex.Message, "เกิดข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // แสดง popup ข้อผิดพลาด — ฟอร์มไม่ปิด ผู้ใช้สามารถเปลี่ยนการเลือกแล้วลองใหม่
            }
        }

        // ============================================================
        //  GetActiveGroupIndices() — อ่านค่าจาก CheckedListBox สร้างรายการ index
        // ============================================================
        //  CheckedListBox คือ control ที่แสดง list ของ item พร้อม checkbox
        //  ลำดับ index ของ item ใน CheckedListBox ต้องตรงกับ SteganographyEngine.SynonymGroups
        //  (กำหนดใน Synonym.Designer.cs ตอน AddRange items)
        //  ถ้าเพิ่ม/ลบกลุ่มคำพ้อง ต้องอัปเดตทั้ง Engine และ Designer.cs ให้ตรงกัน

        /// <summary>คืนรายการ index ของกลุ่มคำที่ผู้ใช้ติ๊กเลือก</summary>
        public List<int> GetActiveGroupIndices()   // คืน List<int> ให้ SteganographyEngine ใช้ระบุว่าจะใช้กลุ่มคำไหน
        {
            var list = new List<int>();   // สร้าง list ว่างสำหรับเก็บ index ของกลุ่มที่ถูกเลือก
            for (int i = 0; i < checkedListBox1.Items.Count; i++)   // วนลูปทุก item ใน CheckedListBox (0 ถึง 63)
            {
                if (checkedListBox1.GetItemChecked(i))   // ตรวจสอบว่า item ที่ index i ถูกติ๊กหรือไม่
                    list.Add(i);   // ถ้าถูกติ๊ก → เพิ่ม index เข้า list
            }
            return list;   // คืน list กลับไปให้ ExecuteSynonym() ส่งต่อให้ Engine
        }
    }
}
