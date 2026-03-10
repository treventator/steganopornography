// ============================================================
// Space.cs — ฟอร์มย่อย (Sub-form) สำหรับเทคนิค Zero-Width Space (ZWSP)
// ============================================================
// ฟอร์มนี้จัดการการฝัง/ถอดข้อมูลด้วยอักขระ Zero-Width Space (U+200B)
// ซึ่งเป็นอักขระที่มองไม่เห็นด้วยตาเปล่า แต่มีอยู่จริงในข้อความ
//
// หลักการ ZWSP:
//   - แทรกอักขระ U+200B (Zero-Width Space) ระหว่างตัวอักษรแต่ละคู่ใน covertext
//   - slot ที่ i = ช่องว่างระหว่าง char[i] กับ char[i+1]:
//     → ถ้ามี ZWSP = bit 1
//     → ถ้าไม่มี ZWSP = bit 0
//   - ความจุ (capacity) = covertext.Length - 1 bits
//   - ไม่ต้องเลือก options ใดๆ เพราะไม่มีคู่/กลุ่มให้เลือก ใช้ได้ทันที
//
// Pattern การสื่อสารกับ Main Form:
//   1. Main form สร้าง instance: var form = new ChkbxZWSP();
//   2. Main form ตั้ง Properties: IsEmbedMode, CipherText, CoverText หรือ SteganotextInput
//   3. Main form เรียก form.ShowDialog() → ฟอร์มเปิดแบบ modal
//   4. ฟอร์มแสดงข้อมูลสรุป (ขนาด covertext, ความจุ) → ผู้ใช้กด OK
//   5. ฟอร์มประมวลผล → เก็บผลลัพธ์ใน ResultText → ปิดตัวเอง → DialogResult.OK
//   6. Main form อ่าน form.ResultText ไปใช้ต่อ
//
// ข้อแตกต่างจาก Homoglyph/Misspelling:
//   - ไม่มี CheckBox หรือ CheckedListBox เพราะไม่มี options ให้เลือก
//   - แสดงเฉพาะ Label คำอธิบาย + ปุ่ม OK/Cancel
//   - UI เรียบง่ายกว่าเพราะเทคนิคนี้ไม่ต้องตั้งค่าอะไร
// ============================================================

using System;               // นำเข้า namespace หลักของ .NET สำหรับ Exception, EventArgs, Math.Max()
using System.Windows.Forms; // นำเข้า namespace สำหรับ Windows Forms UI เช่น Form, Label, Button, MessageBox

namespace Steganography     // ประกาศ namespace ของโปรเจค — ทุกคลาสในโปรเจคอยู่ใน namespace เดียวกัน
{
    /// <summary>
    /// หน้าต่างสำหรับเทคนิค Zero-Width Space (ZWSP)
    /// แทรก U+200B ระหว่างตัวอักษรใน covertext เพื่อแทน bit 0/1
    /// </summary>
    public partial class ChkbxZWSP : Form   // ประกาศคลาส ChkbxZWSP สืบทอดจาก Form
    {                                        // "partial" = แบ่งเป็น 2 ไฟล์: Space.cs (logic) + Space.Designer.cs (UI layout)
                                             // ชื่อคลาส ChkbxZWSP มาจากชื่อเดิมที่เคยมี checkbox → ปัจจุบันไม่มี checkbox แล้ว

        // ============================================================
        //  Properties: ช่องทางสื่อสารข้อมูลกับ Main Form
        // ============================================================
        //  Properties pattern เหมือนกับทุก sub-form ในโปรเจค:
        //  Main form set ค่าก่อนเรียก ShowDialog() → sub-form อ่านค่าตอนทำงาน
        //  → sub-form เขียนผลลัพธ์ลง ResultText → Main form อ่านหลัง DialogResult.OK

        /// <summary>โหมดการทำงาน: true = Embed, false = Extract</summary>
        public bool IsEmbedMode { get; set; } = true;   // true = ฝังข้อมูล (แทรก ZWSP ลงใน covertext)
                                                          // false = ถอดข้อมูล (อ่าน ZWSP จาก steganotext)

        /// <summary>Ciphertext (Base64) ที่จะฝัง</summary>
        public string CipherText { get; set; } = "";     // ข้อความเข้ารหัส AES (Base64) ที่ต้องการซ่อน — ใช้ตอน Embed เท่านั้น

        /// <summary>Covertext สำหรับฝัง</summary>
        public string CoverText { get; set; } = "";      // ข้อความปกติที่จะแทรก ZWSP เข้าไป — ใช้ตอน Embed เท่านั้น

        /// <summary>Steganotext สำหรับถอด</summary>
        public string SteganotextInput { get; set; } = ""; // ข้อความที่มี ZWSP ซ่อนอยู่ — ใช้ตอน Extract เท่านั้น

        /// <summary>ผลลัพธ์หลัง OK</summary>
        public string ResultText { get; private set; } = ""; // ผลลัพธ์หลังประมวลผล — private set เพราะเฉพาะ sub-form เท่านั้นที่เขียนได้
                                                              // Embed → Steganotext (ข้อความที่แทรก ZWSP แล้ว)
                                                              // Extract → Ciphertext (Base64) ที่ถอดออกมาจาก ZWSP

        // ============================================================
        //  Constructor — จุดเริ่มต้นเมื่อสร้าง object ของฟอร์ม
        // ============================================================

        public ChkbxZWSP()          // Constructor — ถูกเรียกเมื่อ main form ทำ new ChkbxZWSP()
        {
            InitializeComponent();  // เรียกเมธอดใน .Designer.cs เพื่อสร้าง UI controls ทั้งหมด
                                    // ในฟอร์มนี้มี: LblDesc (Label คำอธิบาย), BtnOK, BtnCancel
        }

        // ============================================================
        //  Event Handler: Form Load — เรียกอัตโนมัติก่อนฟอร์มแสดงผลครั้งแรก
        // ============================================================

        private void ChkbxZWSP_Load(object sender, EventArgs e)   // event handler สำหรับ Form.Load
        {
            Text = IsEmbedMode                           // ตั้ง title bar ตามโหมด
                ? "Zero-Width Space — ฝังข้อมูล"         // โหมด Embed → บอกว่ากำลังจะ "ฝัง"
                : "Zero-Width Space — ถอดข้อมูล";        // โหมด Extract → บอกว่ากำลังจะ "ถอด"

            LblDesc.Text = IsEmbedMode                   // ตั้งข้อความคำอธิบายบน Label ตามโหมด
                ? "เทคนิคนี้แทรกอักขระ Zero-Width Space (U+200B) ระหว่างตัวอักษรใน covertext\n" +
                  "bit 1 = แทรก ZWSP, bit 0 = ไม่แทรก\n\n" +                                      // อธิบายหลักการทำงานให้ผู้ใช้เข้าใจ
                  $"Covertext ที่ป้อน: {CoverText.Length} ตัวอักษร " +                              // แสดงจำนวนตัวอักษรของ covertext
                  $"(รองรับได้สูงสุด {Math.Max(0, CoverText.Length - 1)} bits)"                    // คำนวณความจุ: ช่องว่างระหว่างตัวอักษร = Length - 1
                                                                                                    // ใช้ Math.Max(0, ...) ป้องกันค่าติดลบเมื่อ covertext ว่าง
                : "อ่าน Zero-Width Space จาก steganotext เพื่อถอดรหัส\n" +                          // โหมด Extract → บอกว่าจะอ่าน ZWSP ออกมา
                  "ไม่ต้องตั้งค่าใดๆ กดตกลงได้เลย";                                                // ไม่มี options ให้เลือก → กด OK ได้เลย
        }

        // ============================================================
        //  Event Handler: ปุ่ม OK — ผู้ใช้กดยืนยัน
        // ============================================================
        //  เมื่อกด OK จะเรียก SteganographyEngine โดยตรง (ไม่มี Execute helper method แยก)
        //  เพราะเทคนิค ZWSP ไม่มี options (CheckBox/CheckedListBox) ให้ต้องอ่าน

        private void BtnOK_Click(object sender, EventArgs e)   // event handler สำหรับปุ่ม OK (ผูกใน Designer.cs)
        {
            try   // ใช้ try-catch เพราะ Engine อาจ throw exception เช่น ความจุ covertext น้อยกว่า payload
            {
                if (IsEmbedMode)   // ถ้าเป็นโหมดฝัง (Embed)
                    ResultText = SteganographyEngine.ZWSPEmbed(CoverText, CipherText);
                    // เรียก ZWSPEmbed ใน SteganographyEngine:
                    //   - CoverText = ข้อความปกติ → Engine จะแทรก U+200B ระหว่างตัวอักษร
                    //   - CipherText = ข้อความเข้ารหัส (Base64) ที่จะแปลงเป็น bits แล้วฝัง
                    //   - คืนค่า Steganotext (ข้อความที่แทรก ZWSP แล้ว — ดูเหมือนข้อความปกติ)
                else   // ถ้าเป็นโหมดถอด (Extract)
                    ResultText = SteganographyEngine.ZWSPExtract(SteganotextInput);
                    // เรียก ZWSPExtract ใน SteganographyEngine:
                    //   - SteganotextInput = ข้อความที่มี ZWSP ซ่อนอยู่
                    //   - ไม่ต้องส่ง options เพราะ ZWSP ไม่มีคู่/กลุ่มให้เลือก
                    //   - Engine จะอ่าน ZWSP ระหว่างตัวอักษรเพื่อสร้าง bit stream → ถอดเป็น Ciphertext

                DialogResult = DialogResult.OK;   // ตั้ง DialogResult เป็น OK → บอก main form ว่าทำงานสำเร็จ
                                                  // main form ตรวจสอบ: if (form.ShowDialog() == DialogResult.OK)
                Close();   // ปิดฟอร์มนี้ → control กลับไป main form → main form อ่าน ResultText
            }
            catch (Exception ex)   // จับ exception ที่อาจเกิดขึ้น เช่น ความจุไม่พอ (covertext สั้นเกินไป)
            {
                MessageBox.Show(ex.Message, "เกิดข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // แสดง popup ข้อผิดพลาด — ฟอร์มไม่ปิด ผู้ใช้สามารถใส่ข้อมูลใหม่แล้วลองอีกครั้ง
            }
        }

        // ============================================================
        //  Event Handler: ปุ่ม Cancel — ผู้ใช้กดยกเลิก
        // ============================================================

        private void BtnCancel_Click(object sender, EventArgs e)   // event handler สำหรับปุ่ม Cancel
        {
            DialogResult = DialogResult.Cancel;   // ตั้ง DialogResult เป็น Cancel → main form จะรู้ว่าผู้ใช้ยกเลิก
                                                  // main form จะไม่อ่าน ResultText ถ้า DialogResult ไม่ใช่ OK
            Close();   // ปิดฟอร์ม → กลับไปที่ main form โดยไม่เปลี่ยนแปลงอะไร
        }
    }
}
