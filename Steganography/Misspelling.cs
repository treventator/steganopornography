// ============================================================
// Misspelling.cs — ฟอร์มย่อย (Sub-form) สำหรับเทคนิค Misspelling
// ============================================================
// ฟอร์มนี้ทำหน้าที่ให้ผู้ใช้เลือกคู่คำสะกดถูก-สะกดผิด (65 คู่)
// แล้วใช้คู่เหล่านั้นในการฝัง (Embed) หรือถอด (Extract) ข้อมูลลับ
//
// หลักการ Misspelling:
//   - มีคู่คำ 65 คู่ เช่น "สวัสดี" (ถูก) ↔ "สวัดดี" (ผิด)
//   - ในข้อความ covertext ถ้าพบคำที่อยู่ในคู่:
//     → ใส่คำสะกดถูก = bit 0
//     → ใส่คำสะกดผิด = bit 1
//   - สแกนข้อความจากซ้ายไปขวาแบบ greedy (ไม่ซ้อนทับ)
//   - ผู้ใช้ต้องเลือกคู่เดียวกันทั้งตอนฝังและตอนถอด มิฉะนั้นจะถอดผิด
//
// Pattern การสื่อสารกับ Main Form (SteganographyForm):
//   1. Main form สร้าง instance: var form = new Misspelling();
//   2. Main form ตั้งค่า Properties: IsEmbedMode, CipherText, CoverText หรือ SteganotextInput
//   3. Main form เรียก form.ShowDialog() → ฟอร์มเปิดแบบ modal (ผู้ใช้ต้องตอบก่อน)
//   4. ผู้ใช้เลือกคู่คำจาก CheckedListBox แล้วกด OK
//   5. ฟอร์มประมวลผล → เก็บผลลัพธ์ใน ResultText → ปิดตัวเอง → DialogResult.OK
//   6. Main form อ่าน form.ResultText ไปใช้ต่อ
//
// ข้อแตกต่างจาก Homoglyph:
//   - ใช้ CheckedListBox แทน CheckBox เพราะมี 65 คู่ (เยอะเกินไปสำหรับ CheckBox แยกตัว)
//   - มีปุ่ม "เลือกทั้งหมด" และ "ล้างทั้งหมด" เพื่อความสะดวก
// ============================================================

using System;                       // นำเข้า namespace หลักของ .NET สำหรับ Exception, EventArgs ฯลฯ
using System.Collections.Generic;   // นำเข้า namespace สำหรับ List<T> ที่ใช้เก็บรายการ index ของคู่คำที่เลือก
using System.Windows.Forms;         // นำเข้า namespace สำหรับ Windows Forms UI เช่น Form, CheckedListBox, MessageBox

namespace Steganography             // ประกาศ namespace ของโปรเจค — ทุกคลาสในโปรเจคอยู่ใน namespace เดียวกัน
{
    /// <summary>
    /// หน้าต่างเลือกคู่คำสะกดผิด-ถูก สำหรับเทคนิค Misspelling
    /// ใช้ทั้งฝั่ง Embed และ Extract
    /// </summary>
    public partial class Misspelling : Form   // ประกาศคลาส Misspelling สืบทอดจาก Form (Windows Forms)
    {                                          // "partial" = คลาสนี้แบ่งเป็น 2 ไฟล์: Misspelling.cs (logic) + Misspelling.Designer.cs (UI)
        // ============================================================
        //  Properties: ช่องทางสื่อสารข้อมูลกับ Main Form
        // ============================================================
        //  Main form จะ set ค่า Properties เหล่านี้ก่อนเรียก ShowDialog()
        //  แล้ว sub-form จะอ่านค่าเหล่านี้ตอนทำงาน
        //  ผลลัพธ์จะถูกเขียนกลับผ่าน ResultText (private set)

        /// <summary>โหมดการทำงาน: true = Embed, false = Extract</summary>
        public bool IsEmbedMode { get; set; } = true;   // true = ฝังข้อมูลลับลงใน covertext, false = ถอดข้อมูลจาก steganotext
                                                          // main form ตั้งค่า false เมื่อเรียกจากแท็บ Decryption

        /// <summary>Ciphertext (Base64) ที่จะฝัง</summary>
        public string CipherText { get; set; } = "";     // ข้อความที่ถูกเข้ารหัส AES แล้ว (Base64) ที่ต้องการซ่อน
                                                          // ใช้เฉพาะตอน Embed — ส่งมาจาก main form (textBox1)

        /// <summary>Covertext สำหรับฝัง</summary>
        public string CoverText { get; set; } = "";      // ข้อความปกติภาษาไทยที่จะใช้เป็นหน้ากาก
                                                          // ใช้เฉพาะตอน Embed — ส่งมาจาก InputCovertext ของ main form

        /// <summary>Steganotext สำหรับถอด</summary>
        public string SteganotextInput { get; set; } = ""; // ข้อความที่มีข้อมูลลับซ่อนอยู่ (ด้วยคำสะกดผิด)
                                                            // ใช้เฉพาะตอน Extract — ส่งมาจาก InputChipertext ของ main form

        /// <summary>ผลลัพธ์หลัง OK</summary>
        public string ResultText { get; private set; } = ""; // ผลลัพธ์หลังประมวลผลเสร็จ
                                                              // private set = เฉพาะฟอร์มนี้เท่านั้นที่เขียนค่าได้ (ป้องกันการแก้ไขจากภายนอก)
                                                              // Embed → ผลคือ Steganotext (ข้อความที่แทนคำสะกดผิด/ถูกแล้ว)
                                                              // Extract → ผลคือ Ciphertext (Base64) ที่ถอดออกมาจากคำสะกดผิด/ถูก

        // ============================================================
        //  Constructor — จุดเริ่มต้นเมื่อสร้าง object ของฟอร์ม
        // ============================================================

        public Misspelling()       // Constructor — ถูกเรียกเมื่อ main form ทำ new Misspelling()
        {
            InitializeComponent(); // เรียกเมธอดใน .Designer.cs เพื่อสร้าง UI controls ทั้งหมด
                                   // รวมถึง checkedListBox1 ที่มี 65 คู่คำ, ปุ่ม OK/Cancel/SelectAll/ClearAll

            Load += (s, ev) =>     // ผูก event handler สำหรับ Form.Load ด้วย lambda expression
                                   // Load event จะ fire ก่อนฟอร์มแสดงผลครั้งแรก
                                   // ใช้ lambda แทน method แยกเพราะ logic สั้นแค่บรรทัดเดียว

                Text = IsEmbedMode // ตั้ง title bar ของฟอร์มตามโหมดการทำงาน
                    ? "Misspelling — เลือกคู่คำสำหรับฝัง"    // โหมด Embed → ชื่อหน้าต่างบอกว่ากำลัง "ฝัง"
                    : "Misspelling — เลือกคู่คำสำหรับถอด";   // โหมด Extract → ชื่อหน้าต่างบอกว่ากำลัง "ถอด"
        }

        // ============================================================
        //  Event Handler: checkedListBox1 Selection Changed
        // ============================================================

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e) // event handler เมื่อผู้ใช้คลิกเปลี่ยน item ที่เลือกใน list
        {
            // เมธอดนี้ว่างเปล่าในปัจจุบัน — เตรียมไว้สำหรับ preview หรือ validation ในอนาคต
            // เช่น อาจแสดงจำนวน bit ที่สามารถฝังได้ตามคู่คำที่เลือก
        }

        // ============================================================
        //  Event Handler: ปุ่ม OK — ผู้ใช้กดยืนยันการเลือก
        // ============================================================

        private void BtnOK_Click(object sender, EventArgs e)   // event handler สำหรับปุ่ม OK (ผูกใน Designer.cs)
        {
            ExecuteMisspelling();   // เรียกเมธอดหลักที่ทำการฝัง/ถอดจริง
        }

        // ============================================================
        //  Event Handler: ปุ่ม Cancel — ผู้ใช้กดยกเลิก
        // ============================================================

        private void BtnCancel_Click(object sender, EventArgs e)   // event handler สำหรับปุ่ม Cancel
        {
            DialogResult = DialogResult.Cancel;   // ตั้ง DialogResult เป็น Cancel → main form จะรู้ว่าผู้ใช้ยกเลิก
            Close();                              // ปิดฟอร์ม → กลับไปที่ main form โดยไม่ทำอะไร
        }

        // ============================================================
        //  Event Handler: ปุ่ม "เลือกทั้งหมด" — ติ๊กทุกคู่คำใน list
        // ============================================================

        private void BtnSelectAll_Click(object sender, EventArgs e)   // event handler สำหรับปุ่ม Select All
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)     // วนลูปทุก item ใน CheckedListBox (0 ถึง 64 = 65 คู่)
                checkedListBox1.SetItemChecked(i, true);               // ติ๊กเลือก item ที่ index i → true = checked
        }                                                              // ประโยชน์: ผู้ใช้ไม่ต้องติ๊กทีละคู่ทั้ง 65 คู่

        // ============================================================
        //  Event Handler: ปุ่ม "ล้างทั้งหมด" — เอาติ๊กออกทุกคู่คำ
        // ============================================================

        private void BtnClearAll_Click(object sender, EventArgs e)    // event handler สำหรับปุ่ม Clear All
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)     // วนลูปทุก item ใน CheckedListBox
                checkedListBox1.SetItemChecked(i, false);              // เอาติ๊กออก item ที่ index i → false = unchecked
        }

        // ============================================================
        //  ExecuteMisspelling() — เมธอดหลักที่ทำการฝังหรือถอดข้อมูล
        // ============================================================
        //  ขั้นตอนการทำงาน:
        //    1. เรียก GetActivePairIndices() เพื่อดูว่าผู้ใช้เลือกคู่คำไหนบ้าง
        //    2. ตรวจสอบว่าเลือกอย่างน้อย 1 คู่ (ถ้าไม่เลือก → แจ้งเตือน)
        //    3. เรียก SteganographyEngine.MisspellingEmbed() หรือ MisspellingExtract()
        //    4. เก็บผลลัพธ์ใน ResultText → ตั้ง DialogResult.OK → ปิดฟอร์ม

        public void ExecuteMisspelling()   // ประกาศ public เพื่อให้ main form เรียกได้โดยตรงถ้าต้องการ
        {
            var activePairs = GetActivePairIndices();   // ดึงรายการ index ของคู่คำที่ถูกติ๊กเลือก
                                                        // เช่น ถ้าเลือกคู่ที่ 0, 3, 5 จะได้ List {0, 3, 5}
            if (activePairs.Count == 0)   // ตรวจสอบว่าเลือกอย่างน้อย 1 คู่ — ถ้าไม่มีเลยจะ Embed/Extract ไม่ได้
            {
                MessageBox.Show("กรุณาเลือกคู่คำอย่างน้อย 1 คู่",   // แสดง popup เตือนผู้ใช้
                    "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;   // ออกจากเมธอดทันที — ฟอร์มยังเปิดอยู่ ผู้ใช้เลือกใหม่ได้
            }

            try   // ใช้ try-catch เพราะ Engine อาจ throw exception เช่น ความจุไม่พอ หรือ covertext ไม่มีคู่คำ
            {
                if (IsEmbedMode)   // ถ้าเป็นโหมดฝัง (Embed)
                    ResultText = SteganographyEngine.MisspellingEmbed(CoverText, CipherText, activePairs);
                    // เรียก MisspellingEmbed ใน SteganographyEngine:
                    //   - CoverText = ข้อความปกติที่จะซ่อนข้อมูล (ต้องมีคู่คำที่เลือกอยู่ในเนื้อหา)
                    //   - CipherText = ข้อความเข้ารหัส (Base64) ที่ต้องการซ่อน
                    //   - activePairs = รายการ index ของคู่คำที่เลือก
                    //   - คืนค่า Steganotext → เก็บใน ResultText
                else   // ถ้าเป็นโหมดถอด (Extract)
                    ResultText = SteganographyEngine.MisspellingExtract(SteganotextInput, activePairs);
                    // เรียก MisspellingExtract ใน SteganographyEngine:
                    //   - SteganotextInput = ข้อความที่มีคำสะกดผิดซ่อนอยู่
                    //   - activePairs = ต้องเลือกคู่เดียวกับตอนฝัง
                    //   - คืนค่า Ciphertext (Base64) → main form นำไปถอดรหัส AES ต่อ

                DialogResult = DialogResult.OK;   // ตั้ง DialogResult เป็น OK → บอก main form ว่าทำงานสำเร็จ
                Close();   // ปิดฟอร์ม → control กลับไป main form → main form อ่าน ResultText
            }
            catch (Exception ex)   // จับ exception ที่อาจเกิดขึ้นระหว่างการ Embed/Extract
            {
                MessageBox.Show(ex.Message, "เกิดข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // แสดง popup ข้อผิดพลาด — ฟอร์มไม่ปิด ผู้ใช้สามารถลองใหม่
            }
        }

        // ============================================================
        //  GetActivePairIndices() — อ่านค่าจาก CheckedListBox สร้างรายการ index
        // ============================================================
        //  CheckedListBox คือ control ที่แสดง list ของ item พร้อม checkbox
        //  ต่างจาก CheckBox ตัวแยกๆ ใน Homoglyph — ที่นี่ใช้ CheckedListBox เพราะมี 65 คู่คำ
        //  ลำดับ index ของ item ใน CheckedListBox ต้องตรงกับ SteganographyEngine.MisspellingPairs
        //  (กำหนดใน Misspelling.Designer.cs ตอน AddRange items)

        /// <summary>คืนรายการ index ของคู่คำที่ผู้ใช้ติ๊กเลือก</summary>
        public List<int> GetActivePairIndices()   // คืน List<int> ให้ SteganographyEngine ใช้ระบุว่าจะใช้คู่คำไหน
        {
            var list = new List<int>();   // สร้าง list ว่างสำหรับเก็บ index
            for (int i = 0; i < checkedListBox1.Items.Count; i++)   // วนลูปทุก item ใน CheckedListBox (0 ถึง 64)
            {
                if (checkedListBox1.GetItemChecked(i))   // ตรวจสอบว่า item ที่ index i ถูกติ๊กหรือไม่
                    list.Add(i);   // ถ้าถูกติ๊ก → เพิ่ม index เข้า list
            }
            return list;   // คืน list กลับไปให้ ExecuteMisspelling() ส่งต่อให้ Engine
        }
    }
}
