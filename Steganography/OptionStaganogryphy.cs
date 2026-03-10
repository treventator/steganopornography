// ============================================================
// OptionStaganogryphy.cs — ฟอร์มย่อย (Sub-form) สำหรับเทคนิค Homoglyph
// ============================================================
// ฟอร์มนี้ทำหน้าที่ให้ผู้ใช้เลือกคู่ตัวอักษรที่มีรูปร่างคล้ายกัน (Homoglyph)
// แล้วใช้คู่เหล่านั้นในการฝัง (Embed) หรือถอด (Extract) ข้อมูลลับ
//
// หลักการ Homoglyph:
//   - ใช้ตัวอักษรไทยที่รูปร่างเหมือนกันแต่เป็นคนละตัว เช่น ฎ↔ฏ, ข↔ฃ, ช↔ซ
//   - ตัวอักษรตัวแรก (original) แทน bit 0, ตัวที่สอง (glyph) แทน bit 1
//   - ผู้ใช้ต้องเลือกคู่เดียวกันทั้งตอนฝังและตอนถอด มิฉะนั้นจะถอดผิด
//
// Pattern การสื่อสารกับ Main Form (SteganographyForm):
//   1. Main form สร้าง instance ของ OptionStaganography
//   2. Main form ตั้งค่า Properties: IsEmbedMode, CipherText, CoverText หรือ SteganotextInput
//   3. Main form เรียก ShowDialog() เพื่อเปิดฟอร์มแบบ modal (บังคับให้ผู้ใช้ตอบก่อนกลับ)
//   4. ผู้ใช้เลือก checkbox แล้วกด OK → ฟอร์มนี้ประมวลผลและเก็บผลลัพธ์ใน ResultText
//   5. ฟอร์มปิดตัวเอง พร้อม DialogResult.OK → Main form อ่านค่า ResultText ไปใช้
// ============================================================

using System;                       // นำเข้า namespace หลักของ .NET สำหรับใช้ประเภทพื้นฐาน เช่น Exception, EventArgs
using System.Collections.Generic;   // นำเข้า namespace สำหรับ List<T> ซึ่งใช้เก็บรายการ index ของคู่ที่เลือก
using System.Windows.Forms;         // นำเข้า namespace สำหรับ Windows Forms UI เช่น Form, CheckBox, MessageBox, DialogResult

namespace Steganography             // ประกาศ namespace ของโปรเจค — ทุกคลาสในโปรเจคอยู่ใน namespace เดียวกัน
{
    /// <summary>
    /// หน้าต่างสำหรับเลือกคู่ตัวอักษร Homoglyph
    /// ใช้ทั้งฝั่ง Embed (Steganography tab) และ Extract (Decryption tab)
    /// </summary>
    public partial class OptionStaganography : Form   // ประกาศคลาส OptionStaganography สืบทอดจาก Form (Windows Forms)
    {                                                  // "partial" หมายความว่าคลาสนี้ถูกแบ่งเป็น 2 ไฟล์: ไฟล์นี้ (logic) กับ .Designer.cs (UI layout)
        // ============================================================
        //  Properties: รับ/ส่งข้อมูลกับ SteganographyForm
        // ============================================================
        //  Properties เหล่านี้เป็น "ช่องทางสื่อสาร" ระหว่าง main form กับ sub-form นี้
        //  Main form จะ set ค่าเหล่านี้ก่อนเรียก ShowDialog()
        //  แล้ว sub-form จะอ่านค่าเหล่านี้ตอนทำงาน และเขียนผลลัพธ์กลับผ่าน ResultText

        /// <summary>โหมดการทำงาน: true = Embed, false = Extract</summary>
        public bool IsEmbedMode { get; set; } = true;  // Property กำหนดโหมด — true = ฝังข้อมูล (Embed), false = ถอดข้อมูล (Extract)
                                                        // ค่า default = true (ฝัง) — main form จะเปลี่ยนเป็น false ถ้าเรียกจากแท็บ Decryption

        /// <summary>Ciphertext (Base64) ที่จะฝัง (ใช้ตอน Embed)</summary>
        public string CipherText { get; set; } = "";    // ข้อความที่ถูกเข้ารหัส AES แล้ว (รูปแบบ Base64) ที่ต้องการซ่อน
                                                         // ใช้เฉพาะตอน Embed — main form ส่งค่ามาจาก textBox1

        /// <summary>Covertext ที่จะใช้ฝัง (ใช้ตอน Embed)</summary>
        public string CoverText { get; set; } = "";     // ข้อความปกติ (ภาษาไทย) ที่จะใช้เป็น "หน้ากาก" สำหรับซ่อนข้อมูล
                                                         // ใช้เฉพาะตอน Embed — main form ส่งค่ามาจาก InputCovertext

        /// <summary>Steganotext ที่จะอ่าน (ใช้ตอน Extract)</summary>
        public string SteganotextInput { get; set; } = ""; // ข้อความที่มีข้อมูลลับซ่อนอยู่แล้ว ที่ต้องการถอดออก
                                                            // ใช้เฉพาะตอน Extract — main form ส่งค่ามาจาก InputChipertext

        /// <summary>ผลลัพธ์ที่ได้หลัง OK (Steganotext หรือ Ciphertext)</summary>
        public string ResultText { get; private set; } = ""; // ผลลัพธ์หลังประมวลผลเสร็จ — main form จะอ่านค่านี้หลัง ShowDialog() คืน OK
                                                              // set เป็น private เพราะเฉพาะฟอร์มนี้เท่านั้นที่ควรเขียนค่าผลลัพธ์
                                                              // ถ้า Embed → ผลลัพธ์คือ Steganotext (ข้อความที่ซ่อนข้อมูลแล้ว)
                                                              // ถ้า Extract → ผลลัพธ์คือ Ciphertext (Base64) ที่ถอดออกมาได้

        // ============================================================
        //  Constructor — จุดเริ่มต้นเมื่อสร้าง object ของฟอร์ม
        // ============================================================

        public OptionStaganography()         // Constructor ของฟอร์ม — ถูกเรียกเมื่อ main form ทำ new OptionStaganography()
        {
            InitializeComponent();           // เรียกเมธอดที่ถูก generate อัตโนมัติใน .Designer.cs
                                             // ทำหน้าที่สร้าง control ทั้งหมด (CheckBox, Button, Label ฯลฯ)
                                             // ตั้งค่าตำแหน่ง ขนาด ข้อความ และ event handler ต่างๆ ของ UI
        }

        // ============================================================
        //  Event Handler: Form Load — เรียกอัตโนมัติเมื่อฟอร์มกำลังจะแสดง
        // ============================================================

        private void OptionStaganography_Load(object sender, EventArgs e)  // event handler สำหรับ Form.Load — ทำงานก่อนฟอร์มแสดงผล
        {                                                                   // sender = object ที่ส่ง event (ตัว form เอง), e = ข้อมูล event (ว่างเปล่า)
            // ปรับ UI ตามโหมด — เปลี่ยน Title Bar ให้ตรงกับสิ่งที่ผู้ใช้กำลังจะทำ
            Text = IsEmbedMode                                              // Text คือ property ของ Form ที่กำหนดข้อความบน title bar
                ? "Homoglyph — เลือกคู่ตัวอักษรสำหรับฝัง"                  // ถ้าเป็นโหมด Embed → แสดงว่ากำลัง "ฝัง"
                : "Homoglyph — เลือกคู่ตัวอักษรสำหรับถอด";                 // ถ้าเป็นโหมด Extract → แสดงว่ากำลัง "ถอด"
        }

        // ============================================================
        //  Event Handler: ปุ่ม OK — ผู้ใช้กดยืนยันการเลือก
        // ============================================================

        private void BtnOK_Click(object sender, EventArgs e)   // event handler สำหรับปุ่ม OK — ผูกไว้ใน Designer.cs
        {
            ExecuteHomoglyph();   // เรียกเมธอดหลักที่ทำการฝัง/ถอดจริง (แยกออกมาเพื่อให้โค้ดเป็นระเบียบ)
        }

        // ============================================================
        //  Event Handler: ปุ่ม Cancel — ผู้ใช้กดยกเลิก
        // ============================================================

        private void BtnCancel_Click(object sender, EventArgs e)   // event handler สำหรับปุ่ม Cancel
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel; // ตั้ง DialogResult เป็น Cancel เพื่อบอก main form ว่าผู้ใช้ยกเลิก
                                                                     // main form จะตรวจสอบค่านี้ → ถ้าไม่ใช่ OK จะไม่นำผลลัพธ์ไปใช้
            Close();   // ปิดฟอร์มนี้ → กลับไปที่ main form โดยไม่เปลี่ยนแปลงอะไร
        }

        // ============================================================
        //  ExecuteHomoglyph() — เมธอดหลักที่ทำการฝังหรือถอดข้อมูล
        // ============================================================
        //  เมธอดนี้:
        //    1. อ่านว่าผู้ใช้ติ๊กเลือก CheckBox คู่ไหนบ้าง
        //    2. ตรวจสอบว่าเลือกอย่างน้อย 1 คู่
        //    3. เรียก SteganographyEngine ให้ทำงานจริง (Embed หรือ Extract)
        //    4. เก็บผลลัพธ์ใน ResultText แล้วปิดฟอร์มพร้อม DialogResult.OK

        /// <summary>
        /// เรียกจาก SteganographyForm หลัง ShowDialog() หรือผ่านปุ่ม OK ใน form นี้
        /// </summary>
        public void ExecuteHomoglyph()   // ประกาศเป็น public เพื่อให้ main form สามารถเรียกได้โดยตรงถ้าต้องการ
        {
            var activePairs = GetActivePairIndices();  // ดึงรายการ index ของคู่ Homoglyph ที่ผู้ใช้ติ๊กเลือกไว้
                                                       // เช่น ถ้าเลือก ฎ↔ฏ กับ ช↔ซ จะได้ List {0, 2}
            if (activePairs.Count == 0)                // ตรวจสอบว่าเลือกอย่างน้อย 1 คู่หรือไม่
            {
                MessageBox.Show("กรุณาเลือกคู่ตัวอักษร Homoglyph อย่างน้อย 1 คู่",   // แสดง popup เตือนถ้าไม่ได้เลือกสักคู่
                    "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);        // ใช้ไอคอนเตือน (สามเหลี่ยมเหลือง)
                return;   // ออกจากเมธอดทันที — ฟอร์มยังไม่ปิด ผู้ใช้สามารถเลือกใหม่ได้
            }

            try   // ใช้ try-catch เพราะ Engine อาจ throw exception ได้ เช่น covertext สั้นเกินไป
            {
                if (IsEmbedMode)   // ถ้าเป็นโหมดฝัง (Embed)
                    ResultText = SteganographyEngine.HomoglyphEmbed(CoverText, CipherText, activePairs);
                    // เรียก HomoglyphEmbed ใน SteganographyEngine:
                    //   - CoverText = ข้อความปกติที่ใช้เป็นหน้ากาก
                    //   - CipherText = ข้อความเข้ารหัสที่ต้องการซ่อน
                    //   - activePairs = รายการคู่ที่เลือก
                    //   - คืนค่า Steganotext (ข้อความที่มีข้อมูลซ่อนอยู่แล้ว) → เก็บใน ResultText
                else   // ถ้าเป็นโหมดถอด (Extract)
                    ResultText = SteganographyEngine.HomoglyphExtract(SteganotextInput, activePairs);
                    // เรียก HomoglyphExtract ใน SteganographyEngine:
                    //   - SteganotextInput = ข้อความที่มีข้อมูลซ่อนอยู่
                    //   - activePairs = ต้องเลือกคู่เดียวกับตอนฝัง มิฉะนั้นถอดผิด
                    //   - คืนค่า Ciphertext (Base64) → เก็บใน ResultText → main form จะนำไปถอดรหัส AES ต่อ

                DialogResult = DialogResult.OK;   // ตั้ง DialogResult เป็น OK เพื่อบอก main form ว่าสำเร็จ
                                                  // main form ตรวจสอบค่านี้: if (form.ShowDialog() == DialogResult.OK)
                Close();   // ปิดฟอร์มนี้ → control กลับไปที่ main form
            }
            catch (Exception ex)   // จับ exception ทุกประเภทที่อาจเกิดขึ้น เช่น ความจุไม่พอ
            {
                MessageBox.Show(ex.Message, "เกิดข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // แสดง popup ข้อผิดพลาด — ฟอร์มไม่ปิด ผู้ใช้สามารถแก้ไขแล้วลองใหม่ได้
            }
        }

        // ============================================================
        //  GetActivePairIndices() — อ่านค่า CheckBox แล้วสร้างรายการ index
        // ============================================================
        //  Homoglyph มี 3 คู่ ใช้ CheckBox แยกแต่ละคู่ (ไม่ใช่ CheckedListBox)
        //  เพราะมีแค่ 3 คู่ จึงใช้ CheckBox ตัวแยกจะดูชัดเจนกว่า
        //
        //  index ของคู่ต้องตรงกับ SteganographyEngine.HomoglyphPairs:
        //    index 0 → ฎ ↔ ฏ (ChkbxDochada)
        //    index 1 → ข ↔ ฃ (ChkbxKhoKhai)
        //    index 2 → ช ↔ ซ (ChkbxChoChang)

        /// <summary>
        /// คืนรายการ index ของคู่ที่ผู้ใช้ติ๊กเลือก
        /// 3 คู่: ฎ↔ฏ (0), ข↔ฃ (1), ช↔ซ (2)
        /// </summary>
        public List<int> GetActivePairIndices()   // คืนค่าเป็น List<int> เพราะ SteganographyEngine รับ parameter เป็น List<int>
        {
            var list = new List<int>();            // สร้าง list ว่างสำหรับเก็บ index ของคู่ที่ถูกเลือก
            if (ChkbxDochada.Checked)  list.Add(0); // ถ้า CheckBox "ฎ ↔ ฏ" ถูกติ๊ก → เพิ่ม index 0 เข้า list
            if (ChkbxKhoKhai.Checked)  list.Add(1); // ถ้า CheckBox "ข ↔ ฃ" ถูกติ๊ก → เพิ่ม index 1 เข้า list
            if (ChkbxChoChang.Checked) list.Add(2); // ถ้า CheckBox "ช ↔ ซ" ถูกติ๊ก → เพิ่ม index 2 เข้า list
            return list;   // คืน list กลับไปให้ ExecuteHomoglyph() ใช้ส่งต่อให้ Engine
        }
    }
}
