// ============================================================
// Nbsp.cs — ฟอร์มย่อย (Sub-form) สำหรับเทคนิค Non-Breaking Space (NBSP)
// ============================================================
// ฟอร์มนี้จัดการการฝัง/ถอดข้อมูลด้วย Non-Breaking Space (U+00A0)
// โดยแทนที่ช่องว่างปกติ (U+0020) ด้วย NBSP ที่มองไม่เห็นความแตกต่างด้วยตาเปล่า
//
// หลักการ NBSP:
//   - สแกน covertext หาตำแหน่ง space ปกติ (U+0020) ทั้งหมด
//   - space ปกติ (U+0020) = bit 0
//   - Non-Breaking Space (U+00A0) = bit 1
//   - ความจุ (capacity) = จำนวน space ใน covertext
//   - ไม่ต้องเลือก options ใดๆ ใช้ได้ทันที
//
// ข้อเปรียบเทียบกับ ZWSP:
//   - ZWSP แทรก "เพิ่ม" อักขระระหว่างตัวอักษร → ความจุ = length - 1
//   - NBSP "แทนที่" space ที่มีอยู่แล้ว → ความจุ = จำนวน space เท่านั้น (น้อยกว่า)
//   - ข้อดีของ NBSP: ไม่ทำให้ข้อความยาวขึ้น (จำนวนอักขระเท่าเดิม)
//
// Pattern การสื่อสารกับ Main Form:
//   1. Main form สร้าง instance: var form = new NbspForm();
//   2. Main form ตั้ง Properties: IsEmbedMode, CipherText, CoverText หรือ SteganotextInput
//   3. Main form เรียก form.ShowDialog() → ฟอร์มเปิดแบบ modal
//   4. ฟอร์มแสดงข้อมูลสรุป (จำนวน space ใน covertext, ความจุ) → ผู้ใช้กด OK
//   5. ฟอร์มประมวลผล → เก็บผลลัพธ์ใน ResultText → ปิดตัวเอง → DialogResult.OK
//   6. Main form อ่าน form.ResultText ไปใช้ต่อ
// ============================================================

using System;               // นำเข้า namespace หลักของ .NET สำหรับ Exception, EventArgs ฯลฯ
using System.Windows.Forms; // นำเข้า namespace สำหรับ Windows Forms UI เช่น Form, Label, Button, MessageBox

namespace Steganography     // ประกาศ namespace ของโปรเจค — ทุกคลาสอยู่ใน namespace เดียวกัน
{
    /// <summary>
    /// หน้าต่างสำหรับเทคนิค Non-Breaking Space (NBSP)
    /// แทนที่ space ปกติ (U+0020) ด้วย NBSP (U+00A0) เพื่อแทน bit 0/1
    /// </summary>
    public partial class NbspForm : Form   // ประกาศคลาส NbspForm สืบทอดจาก Form
    {                                       // "partial" = แบ่งเป็น 2 ไฟล์: Nbsp.cs (logic) + Nbsp.Designer.cs (UI layout)

        // ============================================================
        //  Properties: ช่องทางสื่อสารข้อมูลกับ Main Form
        // ============================================================
        //  ใช้ Properties pattern เหมือนกันทุก sub-form:
        //  Main form set ค่า → sub-form อ่าน → ประมวลผล → เขียน ResultText → Main form อ่านกลับ

        /// <summary>โหมดการทำงาน: true = Embed, false = Extract</summary>
        public bool IsEmbedMode { get; set; } = true;   // true = ฝังข้อมูล (แทนที่ space ด้วย NBSP)
                                                          // false = ถอดข้อมูล (อ่านว่าตำแหน่งไหนเป็น space ตำแหน่งไหนเป็น NBSP)

        /// <summary>Ciphertext (Base64) ที่จะฝัง</summary>
        public string CipherText { get; set; } = "";     // ข้อความเข้ารหัส AES (Base64) ที่ต้องการซ่อน — ใช้ตอน Embed เท่านั้น

        /// <summary>Covertext สำหรับฝัง</summary>
        public string CoverText { get; set; } = "";      // ข้อความปกติที่จะแทนที่ space — ใช้ตอน Embed เท่านั้น

        /// <summary>Steganotext สำหรับถอด</summary>
        public string SteganotextInput { get; set; } = ""; // ข้อความที่มี NBSP ซ่อนอยู่ — ใช้ตอน Extract เท่านั้น

        /// <summary>ผลลัพธ์หลัง OK</summary>
        public string ResultText { get; private set; } = ""; // ผลลัพธ์หลังประมวลผล — private set ป้องกันการเขียนจากภายนอก
                                                              // Embed → Steganotext (ข้อความที่แทนที่ space ด้วย NBSP แล้ว)
                                                              // Extract → Ciphertext (Base64) ที่ถอดออกมาจากรูปแบบ space/NBSP

        // ============================================================
        //  Constructor — จุดเริ่มต้นเมื่อสร้าง object ของฟอร์ม
        // ============================================================

        public NbspForm()           // Constructor — ถูกเรียกเมื่อ main form ทำ new NbspForm()
        {
            InitializeComponent();  // เรียกเมธอดใน .Designer.cs เพื่อสร้าง UI controls
                                    // ในฟอร์มนี้มี: LblDesc (Label คำอธิบาย), BtnOK, BtnCancel
        }

        // ============================================================
        //  Event Handler: Form Load — เรียกอัตโนมัติก่อนฟอร์มแสดงผลครั้งแรก
        // ============================================================

        private void NbspForm_Load(object sender, EventArgs e)   // event handler สำหรับ Form.Load
        {
            int spaceCount = 0;                 // ตัวนับจำนวน space ปกติใน covertext — ใช้แสดงความจุให้ผู้ใช้เห็น
            foreach (char c in CoverText)       // วนลูปอ่านทีละตัวอักษรใน covertext
                if (c == ' ') spaceCount++;     // ถ้าเป็น space ปกติ (U+0020) ให้นับเพิ่ม
                                                // ค่า spaceCount นี้ = ความจุสูงสุด (จำนวน bits ที่ฝังได้)

            Text = IsEmbedMode                           // ตั้ง title bar ของฟอร์มตามโหมด
                ? "Non-Breaking Space — ฝังข้อมูล"       // โหมด Embed → ชื่อหน้าต่างบอกว่ากำลัง "ฝัง"
                : "Non-Breaking Space — ถอดข้อมูล";      // โหมด Extract → ชื่อหน้าต่างบอกว่ากำลัง "ถอด"

            LblDesc.Text = IsEmbedMode                   // ตั้งข้อความคำอธิบายบน Label ตามโหมด
                ? "เทคนิคนี้แทนที่ช่องว่างปกติ (U+0020) ด้วย Non-Breaking Space (U+00A0)\n" +         // อธิบายหลักการทำงาน
                  "bit 1 = NBSP, bit 0 = space ปกติ\n\n" +                                            // อธิบายว่า bit 0 และ bit 1 แทนด้วยอะไร
                  $"Covertext ที่ป้อน: {CoverText.Length} ตัวอักษร ({spaceCount} spaces)" +            // แสดงจำนวนตัวอักษรทั้งหมดและจำนวน space
                  $" — รองรับได้สูงสุด {spaceCount} bits"                                             // แสดงความจุสูงสุด = จำนวน space ใน covertext
                : "อ่าน Non-Breaking Space จาก steganotext เพื่อถอดรหัส\n" +                           // โหมด Extract → อธิบายว่าจะอ่าน NBSP ออกมา
                  "ไม่ต้องตั้งค่าใดๆ กดตกลงได้เลย";                                                   // ไม่มี options → กด OK ได้เลย
        }

        // ============================================================
        //  Event Handler: ปุ่ม OK — ผู้ใช้กดยืนยัน
        // ============================================================
        //  เหมือน ZWSP — ไม่มี Execute helper method แยก
        //  เพราะเทคนิค NBSP ไม่มี options ให้เลือก สามารถเรียก Engine ตรงๆ ได้

        private void BtnOK_Click(object sender, EventArgs e)   // event handler สำหรับปุ่ม OK (ผูกใน Designer.cs)
        {
            try   // ใช้ try-catch เพราะ Engine อาจ throw exception เช่น จำนวน space ใน covertext น้อยกว่า payload
            {
                if (IsEmbedMode)   // ถ้าเป็นโหมดฝัง (Embed)
                    ResultText = SteganographyEngine.NBSPEmbed(CoverText, CipherText);
                    // เรียก NBSPEmbed ใน SteganographyEngine:
                    //   - CoverText = ข้อความปกติ → Engine จะสแกนหา space แล้วแทนที่ด้วย NBSP ตาม payload bits
                    //   - CipherText = ข้อความเข้ารหัส (Base64) ที่จะแปลงเป็น bits แล้วฝัง
                    //   - ตำแหน่ง space ที่ต้องเป็น bit 1 จะถูกแทนที่ด้วย U+00A0
                    //   - ตำแหน่ง space ที่ต้องเป็น bit 0 จะคงเป็น U+0020 เดิม
                    //   - คืนค่า Steganotext → ดูเหมือนข้อความเดิมทุกประการ
                else   // ถ้าเป็นโหมดถอด (Extract)
                    ResultText = SteganographyEngine.NBSPExtract(SteganotextInput);
                    // เรียก NBSPExtract ใน SteganographyEngine:
                    //   - SteganotextInput = ข้อความที่มี NBSP ซ่อนอยู่
                    //   - Engine จะสแกนทุกตำแหน่ง: ถ้าเป็น U+0020 = bit 0, ถ้าเป็น U+00A0 = bit 1
                    //   - สร้าง bit stream → ถอดเป็น Ciphertext (Base64)

                DialogResult = DialogResult.OK;   // ตั้ง DialogResult เป็น OK → บอก main form ว่าทำงานสำเร็จ
                Close();   // ปิดฟอร์มนี้ → control กลับไป main form → main form อ่าน ResultText
            }
            catch (Exception ex)   // จับ exception ที่อาจเกิดขึ้น เช่น ความจุ space ไม่พอฝัง payload
            {
                MessageBox.Show(ex.Message, "เกิดข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // แสดง popup ข้อผิดพลาด — ฟอร์มไม่ปิด ผู้ใช้สามารถลองใหม่ (เช่น ใช้ covertext ที่ยาวกว่า)
            }
        }

        // ============================================================
        //  Event Handler: ปุ่ม Cancel — ผู้ใช้กดยกเลิก
        // ============================================================

        private void BtnCancel_Click(object sender, EventArgs e)   // event handler สำหรับปุ่ม Cancel
        {
            DialogResult = DialogResult.Cancel;   // ตั้ง DialogResult เป็น Cancel → main form จะรู้ว่าผู้ใช้ยกเลิก
                                                  // main form จะไม่อ่าน ResultText เพราะ DialogResult ไม่ใช่ OK
            Close();   // ปิดฟอร์ม → กลับไปที่ main form โดยไม่เปลี่ยนแปลงอะไร
        }
    }
}
