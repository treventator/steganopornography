// =============================================================================================================
//  SteganographyForm.cs — ฟอร์มหลักของแอปพลิเคชัน Steganography
// =============================================================================================================
//  ไฟล์นี้เป็น "สมอง" ของโปรแกรม ทำหน้าที่ควบคุม UI ทั้งหมด 3 แท็บ:
//
//  【ขั้นตอนการทำงาน (Flow)】
//  ┌─────────────────────┐     ┌──────────────────────┐     ┌──────────────────────┐
//  │  1. Encryption Tab  │ ──► │  2. Steganography Tab│ ──► │  3. Decryption Tab   │
//  │  - รับ Plaintext    │     │  - รับ Ciphertext    │     │  - รับ Steganotext   │
//  │  - รับ Key          │     │    (sync อัตโนมัติ)  │     │  - รับ Key เดิม      │
//  │  - รับ Covertext    │     │  - เลือกเทคนิค       │     │  - เลือกเทคนิคเดิม   │
//  │  - เข้ารหัส AES-256│     │  - ฝัง → Steganotext │     │  - ดึง → Ciphertext  │
//  └─────────────────────┘     └──────────────────────┘     │  - ถอดรหัส AES      │
//                                                            │  - ได้ Plaintext     │
//                                                            └──────────────────────┘
//
//  【เทคนิค Steganography ที่รองรับ】
//  - Homoglyph  : สลับตัวอักษรที่หน้าตาคล้ายกัน (ฎ↔ฏ, ข↔ฃ, ช↔ซ)
//  - Misspelling: สลับคำถูก/คำผิดที่กำหนดไว้ 65 คู่
//  - ZWSP       : แทรก Zero-Width Space (U+200B) ระหว่างตัวอักษร — มองไม่เห็น
//  - NBSP       : แทนที่ Space ปกติ (U+0020) ด้วย Non-Breaking Space (U+00A0)
//  - Synonym    : สลับคำพ้องความหมาย 64 กลุ่ม
// =============================================================================================================

using System;       // ใช้ namespace พื้นฐานของ .NET เช่น Exception, EventArgs, Math
using System.IO;    // ใช้สำหรับอ่าน/เขียนไฟล์ (File, FileInfo, Path)
using System.Text;  // ใช้ Encoding.UTF8 สำหรับแปลงข้อความเป็น bytes
using System.Windows.Forms; // ใช้คลาส UI ทั้งหมดของ Windows Forms (Form, TextBox, Button, MessageBox, ฯลฯ)

namespace Steganography // namespace เดียวกันกับทุกไฟล์ในโปรเจค เพื่อให้เรียกคลาสข้ามไฟล์ได้โดยไม่ต้อง using
{
    // คลาส SteganographyForm สืบทอดจาก Form ซึ่งเป็นหน้าต่างหลักของ Windows Forms
    // "partial" หมายความว่าคลาสนี้ถูกแบ่งเป็น 2 ไฟล์: ไฟล์นี้ (logic) + .Designer.cs (UI layout)
    public partial class SteganographyForm : Form
    {
        // ค่าคงที่กำหนดขนาดไฟล์สูงสุดที่อนุญาตให้โหลด = 5 MB
        // เหตุผล: ป้องกัน TextBox ค้างเมื่อโหลดไฟล์ใหญ่เกินไป (TextBox ไม่ได้ออกแบบมาสำหรับข้อความหลายล้านตัวอักษร)
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        // สร้าง ToolTip object สำหรับแสดงคำอธิบายเมื่อผู้ใช้ hover เมาส์บน control ต่างๆ
        // เหตุผล: ช่วยให้ผู้ใช้เข้าใจว่าแต่ละช่องใช้ทำอะไร โดยไม่ต้องอ่านคู่มือ
        System.Windows.Forms.ToolTip tt = new System.Windows.Forms.ToolTip();

        // Constructor ของฟอร์มหลัก — ถูกเรียกตอน new SteganographyForm() ใน Program.cs
        public SteganographyForm()
        {
            // InitializeComponent() ถูก generate อัตโนมัติใน .Designer.cs
            // มันสร้างและจัดวาง control ทั้งหมด (TextBox, Button, TabControl, ฯลฯ) ตาม layout ที่ออกแบบไว้
            InitializeComponent();
        }

        // ============================================================
        //  Form Load — ตั้งค่าเริ่มต้นทุกอย่างเมื่อฟอร์มถูกแสดงครั้งแรก
        // ============================================================
        //  เมธอดนี้ถูกเรียกอัตโนมัติเมื่อฟอร์มโหลดเสร็จ (event: Form.Load)
        //  ทำหน้าที่: ตั้งค่า ToolTip, ซ่อน Key, ตั้งค่า Drag & Drop

        private void SteganographyForm_Load(object sender, EventArgs e)
        {
            // ตั้งข้อความเริ่มต้นใน status bar ด้านล่างของหน้าต่าง
            // เหตุผล: แจ้งผู้ใช้ว่าโปรแกรมพร้อมใช้งานแล้ว
            tsslLabel.Text = "พร้อมใช้งาน";

            // ตั้ง ToolTip ให้แต่ละ control ที่ต้องการคำอธิบาย
            // เหตุผล: ผู้ใช้ hover เมาส์จะเห็นคำอธิบายว่าช่องนี้ใช้ทำอะไร
            tt.SetToolTip(InputKey, "Key ที่ใช้เข้ารหัส AES-256 (PBKDF2, 10,000 iterations)"); // อธิบายว่า Key ใช้กับ AES-256 + PBKDF2
            tt.SetToolTip(InputkeyDecry, "Key เดียวกับที่ใช้ตอนเข้ารหัส"); // เตือนว่าต้องใช้ Key เดิม
            tt.SetToolTip(TbStegano, "Steganotext ที่ซ่อนข้อมูลลับไว้ (ReadOnly)"); // บอกว่าช่องนี้ read-only

            // ซ่อน Key ด้วยตัวอักษร ● เพื่อความปลอดภัย (เหมือนช่อง password ในเว็บ)
            // เหตุผล: ป้องกันคนที่มองหน้าจอจากด้านข้าง (shoulder surfing) ไม่ให้เห็น Key
            InputKey.PasswordChar      = '●'; // ช่อง Key ในแท็บ Encryption — แสดงเป็น ●●●●
            InputkeyDecry.PasswordChar = '●'; // ช่อง Key ในแท็บ Decryption — แสดงเป็น ●●●●

            // ──────────────────────────────────────────────
            //  Drag & Drop สำหรับ InputPlaintext (แท็บ Encryption)
            // ──────────────────────────────────────────────
            //  ผู้ใช้สามารถลากไฟล์ .txt มาวางบน TextBox ได้โดยตรง แทนที่จะกดปุ่ม Browser
            //  ต้องตั้งค่า 2 event: DragEnter (ตรวจว่าเป็นไฟล์ไหม) + DragDrop (โหลดไฟล์)

            // เปิดใช้งาน Drag & Drop บน InputPlaintext
            InputPlaintext.AllowDrop = true;

            // DragEnter: เมื่อผู้ใช้ลากไฟล์มาวางบน TextBox → ตรวจสอบว่าข้อมูลที่ลากมาเป็น "ไฟล์" หรือไม่
            // ถ้าเป็นไฟล์ → แสดงไอคอน Copy (เครื่องหมาย +), ถ้าไม่ใช่ → แสดงว่าไม่รับ (None)
            InputPlaintext.DragEnter += (s, ev) =>
                ev.Effect = ev.Data.GetDataPresent(DataFormats.FileDrop) // ตรวจว่ามีข้อมูลชนิด FileDrop ไหม
                    ? DragDropEffects.Copy   // มี → แสดงว่ารับได้ (cursor เปลี่ยนเป็น +)
                    : DragDropEffects.None;  // ไม่มี → ไม่รับ (cursor เปลี่ยนเป็นห้าม)

            // DragDrop: เมื่อผู้ใช้ปล่อยไฟล์ลง TextBox → โหลดเนื้อหาไฟล์มาใส่
            InputPlaintext.DragDrop += (s, ev) =>
            {
                // ดึง path ของไฟล์ที่ลากมา (อาจลากมาหลายไฟล์ แต่เราใช้แค่ไฟล์แรก)
                var files = (string[])ev.Data.GetData(DataFormats.FileDrop);
                // ตรวจสอบว่ามีไฟล์จริงๆ (ไม่ใช่ null และมีอย่างน้อย 1 ไฟล์)
                if (files != null && files.Length > 0)
                    // โหลดไฟล์แรกเข้า InputPlaintext พร้อมแสดงชื่อไฟล์ที่ namefile
                    LoadFileIntoTextBox(files[0], InputPlaintext, namefile);
            };

            // ──────────────────────────────────────────────
            //  Drag & Drop สำหรับ InputChipertext (แท็บ Decryption)
            // ──────────────────────────────────────────────
            //  ผู้ใช้ลากไฟล์ steganotext มาวางที่ช่อง Decryption ได้เลย

            // เปิดใช้งาน Drag & Drop บน InputChipertext
            InputChipertext.AllowDrop = true;

            // DragEnter: ตรวจว่าเป็นไฟล์ไหม (เหมือนกับด้านบน)
            InputChipertext.DragEnter += (s, ev) =>
                ev.Effect = ev.Data.GetDataPresent(DataFormats.FileDrop) // ตรวจชนิดข้อมูล
                    ? DragDropEffects.Copy   // เป็นไฟล์ → รับได้
                    : DragDropEffects.None;  // ไม่ใช่ → ไม่รับ

            // DragDrop: ปล่อยไฟล์ → โหลดเนื้อหา
            InputChipertext.DragDrop += (s, ev) =>
            {
                var files = (string[])ev.Data.GetData(DataFormats.FileDrop); // ดึง path ไฟล์
                if (files != null && files.Length > 0)
                    // โหลดไฟล์เข้า InputChipertext; ส่ง null เป็น fileNameLabel เพราะ Decryption tab ไม่มีช่องแสดงชื่อไฟล์
                    LoadFileIntoTextBox(files[0], InputChipertext, null);
            };
        }

        // ============================================================
        //  LoadFileIntoTextBox — ฟังก์ชันกลางสำหรับโหลดไฟล์เข้า TextBox
        // ============================================================
        //  ใช้ร่วมกันทั้ง Drag & Drop และปุ่ม Browser
        //  พารามิเตอร์:
        //    filePath      = path ของไฟล์ที่จะโหลด
        //    target        = TextBox ที่จะใส่เนื้อหาไฟล์ลงไป
        //    fileNameLabel = TextBox ที่จะแสดงชื่อไฟล์ (ถ้า null จะข้ามไป)

        /// <summary>
        /// โหลดเนื้อหาจากไฟล์ .txt เข้า TextBox ที่กำหนด
        /// </summary>
        private void LoadFileIntoTextBox(string filePath, System.Windows.Forms.TextBox target, System.Windows.Forms.TextBox fileNameLabel)
        {
            try // try-catch เพื่อดักจับ error เช่น ไฟล์ถูกล็อก, permission denied, ไฟล์เสียหาย
            {
                // สร้าง FileInfo เพื่อตรวจสอบขนาดไฟล์ก่อนอ่านจริง
                // เหตุผล: ตรวจขนาดก่อนเพื่อไม่ให้โปรแกรมค้างจากการอ่านไฟล์ใหญ่มาก
                var fi = new FileInfo(filePath);

                // ตรวจว่าไฟล์ใหญ่เกินขีดจำกัด 5 MB หรือไม่
                if (fi.Length > MaxFileSizeBytes)
                {
                    // แจ้งเตือนผู้ใช้ว่าไฟล์ใหญ่เกินไป พร้อมบอกขนาดจริง vs ขีดจำกัด
                    MessageBox.Show($"ไฟล์ใหญ่เกินไป ({fi.Length / 1024} KB)\nรองรับสูงสุด {MaxFileSizeBytes / 1024} KB",
                        "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return; // หยุดทำงาน ไม่โหลดไฟล์
                }

                // อ่านเนื้อหาทั้งหมดจากไฟล์เป็น string ด้วย encoding UTF-8
                // เหตุผล: ภาษาไทยใช้ UTF-8 ถ้าใช้ encoding อื่นจะได้อักษรเพี้ยน
                string content = File.ReadAllText(filePath, Encoding.UTF8);

                // ใส่เนื้อหาไฟล์ลงใน TextBox เป้าหมาย
                target.Text = content;

                // ถ้ามี TextBox สำหรับแสดงชื่อไฟล์ → ใส่ชื่อไฟล์ (ไม่รวม path)
                // เหตุผล: Encryption tab มี namefile แสดงชื่อไฟล์, Decryption tab ไม่มี (ส่ง null)
                if (fileNameLabel != null)
                    fileNameLabel.Text = Path.GetFileName(filePath); // ดึงเฉพาะชื่อไฟล์ เช่น "test.txt" ไม่เอา path เต็ม

                // อัปเดต status bar แสดงชื่อไฟล์ที่โหลด + จำนวนตัวอักษร
                tsslLabel.Text = "โหลดไฟล์: " + Path.GetFileName(filePath) + "  |  " + content.Length + " chars";
            }
            catch (Exception ex) // ดักจับ error ทุกชนิดที่อาจเกิดจากการอ่านไฟล์
            {
                // แจ้งผู้ใช้ว่าอ่านไฟล์ไม่ได้ พร้อมแสดง error message
                MessageBox.Show("ไม่สามารถอ่านไฟล์ได้:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============================================================
        //  Tab Changed — Sync Ciphertext อัตโนมัติเมื่อเปลี่ยนแท็บ
        // ============================================================
        //  Event นี้เกิดขึ้นทุกครั้งที่ผู้ใช้คลิกเปลี่ยนแท็บ
        //  ใช้สำหรับ sync ข้อมูล Ciphertext จากแท็บ Encryption → Steganography โดยอัตโนมัติ
        //  เหตุผล: ผู้ใช้ไม่ต้อง copy-paste ciphertext ด้วยตัวเอง ลดโอกาสผิดพลาด

        private void TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // ตรวจว่าผู้ใช้เปลี่ยนไปแท็บ Steganography หรือไม่
            if (TabControl1.SelectedTab == TabSteganography)
            {
                // ถ้า textBox1 (ช่อง Ciphertext ในแท็บ Steganography) ยังว่าง
                // แต่ InputPayload (ช่อง Ciphertext ในแท็บ Encryption) มีค่าอยู่
                // → sync ข้อมูลมาให้อัตโนมัติ
                if (string.IsNullOrEmpty(textBox1.Text) && !string.IsNullOrEmpty(InputPayload.Text))
                {
                    textBox1.Text = InputPayload.Text; // คัดลอก ciphertext จาก Encryption → Steganography
                    tsslLabel.Text = "Sync Ciphertext จาก Encryption tab แล้ว"; // แจ้งผู้ใช้ว่า sync แล้ว
                }

                // อัปเดตจำนวน bit ที่ต้องการ vs ความจุ covertext แสดงใน status bar
                // เหตุผล: ให้ผู้ใช้เห็นว่า covertext พอซ่อนข้อมูลได้ไหม ก่อนจะกดปุ่มเทคนิค
                UpdateSteganoBitCapacity();
            }
        }

        // ============================================================
        //  แท็บ Encryption — เข้ารหัส AES-256 + จัดการไฟล์
        // ============================================================
        //  แท็บนี้เป็นขั้นตอนแรก: ผู้ใช้ใส่ Plaintext + Key + Covertext
        //  แล้วกด Encryption เพื่อเข้ารหัสเป็น Ciphertext (Base64)
        //  Ciphertext จะถูก sync ไปยังแท็บ Steganography อัตโนมัติ

        // ──── ปุ่ม Encryption (เข้ารหัส AES-256) ────
        //  ขั้นตอน: ตรวจ input → AesEncrypt(plaintext, key) → แสดง ciphertext + sync ไป Stegano tab
        private void EncrytionButton1_Click(object sender, EventArgs e)
        {
            // Validation: ตรวจว่าผู้ใช้ใส่ Key หรือยัง
            // เหตุผล: ไม่มี Key จะเข้ารหัส AES ไม่ได้
            if (string.IsNullOrEmpty(InputKey.Text))
            {
                MessageBox.Show("กรุณาใส่ Key ก่อนเข้ารหัส", "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // หยุดทำงาน ไม่เข้ารหัส
            }

            // Validation: ตรวจว่าผู้ใช้ใส่ Plaintext หรือยัง
            // เหตุผล: ไม่มีข้อความจะเข้ารหัสอะไร
            if (string.IsNullOrEmpty(InputPlaintext.Text))
            {
                MessageBox.Show("กรุณาใส่ข้อความ Plaintext ก่อนเข้ารหัส", "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // หยุดทำงาน
            }

            try // try-catch เพราะการเข้ารหัสอาจล้มเหลวได้ (เช่น key ไม่ valid)
            {
                // เรียก CryptoHelper.AesEncrypt() เพื่อเข้ารหัส plaintext ด้วย AES-256-CBC
                // Input: ข้อความ plaintext + key (password)
                // Output: ciphertext เป็น Base64 string (สามารถ copy/paste ได้ง่าย)
                // ภายใน: ใช้ PBKDF2 แปลง password → AES key 256-bit + IV 128-bit
                string cipherBase64 = CryptoHelper.AesEncrypt(InputPlaintext.Text, InputKey.Text);

                // แสดง ciphertext ในช่อง InputPayload (แท็บ Encryption)
                InputPayload.Text = cipherBase64;

                // sync ciphertext ไปที่ textBox1 ในแท็บ Steganography ทันที
                // เหตุผล: เพื่อให้ผู้ใช้ไม่ต้อง copy-paste เอง เมื่อสลับไปแท็บ Steganography ข้อมูลจะพร้อมใช้
                textBox1.Text = cipherBase64;

                // อัปเดต status bar แสดงว่าเข้ารหัสสำเร็จ + จำนวนตัวอักษรของ ciphertext
                tsslLabel.Text = "เข้ารหัสสำเร็จ  |  Ciphertext: " + cipherBase64.Length + " chars";
            }
            catch (Exception ex) // ดักจับ error จากการเข้ารหัส
            {
                // แจ้งผู้ใช้ว่าเข้ารหัสไม่สำเร็จ พร้อมแสดงรายละเอียด error
                MessageBox.Show("เกิดข้อผิดพลาดในการเข้ารหัส:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ──── ปุ่ม Browser (เลือกไฟล์ .txt เพื่อโหลด Plaintext) ────
        //  เปิด OpenFileDialog ให้ผู้ใช้เลือกไฟล์ → ตรวจขนาด → อ่านเนื้อหา → ใส่ลง InputPlaintext
        private void Browser_Click(object sender, EventArgs e)
        {
            // สร้าง OpenFileDialog ภายใน using block เพื่อให้ถูก dispose อัตโนมัติหลังใช้งาน
            // เหตุผล: dialog ใช้ทรัพยากร OS ต้อง dispose เพื่อคืนหน่วยความจำ
            using (var dlg = new OpenFileDialog())
            {
                // ตั้งค่า filter ให้แสดงเฉพาะไฟล์ .txt เป็นค่าเริ่มต้น หรือแสดงทุกไฟล์
                dlg.Filter = "Text file (*.txt)|*.txt|All files (*.*)|*.*";
                // ไม่อนุญาตเลือกหลายไฟล์ — ใช้ไฟล์เดียวต่อครั้ง
                dlg.Multiselect = false;

                // แสดง dialog ให้ผู้ใช้เลือกไฟล์ ถ้ากด OK จะเข้า if
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    // ตรวจขนาดไฟล์ก่อนอ่าน (เหมือน LoadFileIntoTextBox)
                    var fi = new FileInfo(dlg.FileName);
                    if (fi.Length > MaxFileSizeBytes)
                    {
                        // ไฟล์ใหญ่เกิน 5 MB → แจ้งเตือนและไม่โหลด
                        MessageBox.Show($"ไฟล์ใหญ่เกินไป ({fi.Length / 1024} KB)\nรองรับสูงสุด {MaxFileSizeBytes / 1024} KB",
                            "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return; // หยุดทำงาน
                    }

                    // แสดงชื่อไฟล์ที่เลือกในช่อง namefile (เฉพาะชื่อ ไม่เอา path เต็ม)
                    namefile.Text = Path.GetFileName(dlg.FileName);

                    try // ดักจับ error จากการอ่านไฟล์
                    {
                        // อ่านเนื้อหาไฟล์ทั้งหมดเป็น string ด้วย UTF-8
                        string content = File.ReadAllText(dlg.FileName, Encoding.UTF8);
                        // ใส่เนื้อหาลง TextBox InputPlaintext
                        InputPlaintext.Text = content;
                        // อัปเดต status bar แสดงชื่อไฟล์ + จำนวนตัวอักษร
                        tsslLabel.Text = "โหลดไฟล์: " + namefile.Text + "  |  " + content.Length + " chars";
                    }
                    catch (Exception ex) // ดักจับ error เช่น ไฟล์ถูกล็อก
                    {
                        MessageBox.Show("ไม่สามารถอ่านไฟล์ได้:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // ──── ปุ่ม Copy/Reset สำหรับแท็บ Encryption ────

        // CopyButton1 = คัดลอก Key ไปยัง Clipboard
        // เหตุผล: ผู้ใช้ต้องจำ Key เพื่อใช้ถอดรหัสทีหลัง การ copy ช่วยให้ไม่ต้องจำ/พิมพ์ซ้ำ
        private void CopyButton1_Click(object sender, EventArgs e)
        {
            // ตรวจว่ามี Key อยู่ในช่องหรือไม่
            if (!string.IsNullOrEmpty(InputKey.Text))
            {
                Clipboard.SetText(InputKey.Text); // คัดลอก Key ไปยัง Clipboard ของ OS
                tsslLabel.Text = "คัดลอก Key แล้ว"; // แจ้งผู้ใช้ผ่าน status bar
            }
            else
                // ไม่มี Key → แจ้งเตือน
                MessageBox.Show("ไม่มี Key ให้ copy", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        // CopyButton2 = คัดลอก Ciphertext (Base64) ไปยัง Clipboard
        // เหตุผล: ผู้ใช้อาจต้องการ copy ciphertext ไปใช้ที่อื่น หรือ paste เข้าแท็บ Decryption ด้วยตัวเอง
        private void CopyButton2_Click(object sender, EventArgs e)
        {
            // ตรวจว่ามี Ciphertext อยู่หรือไม่
            if (!string.IsNullOrEmpty(InputPayload.Text))
            {
                Clipboard.SetText(InputPayload.Text); // คัดลอก Ciphertext ไปยัง Clipboard
                tsslLabel.Text = "คัดลอก Ciphertext แล้ว"; // แจ้งผ่าน status bar
            }
            else
                // ยังไม่มี Ciphertext → แจ้งให้เข้ารหัสก่อน
                MessageBox.Show("ไม่มี Ciphertext ให้ copy — กด Encryption ก่อน", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        // BtRs1 = ล้างช่อง Plaintext + ชื่อไฟล์
        // เหตุผล: ให้ผู้ใช้เริ่มใหม่ได้โดยไม่ต้องลบทีละตัวอักษร
        private void ResetBotton1_Click(object sender, EventArgs e)
        {
            InputPlaintext.Clear(); // ล้างเนื้อหาในช่อง Plaintext
            namefile.Clear();       // ล้างชื่อไฟล์ที่แสดงอยู่
            tsslLabel.Text = "ล้าง Plaintext แล้ว"; // แจ้งผ่าน status bar
        }

        // BtRs2 = ล้างช่อง Key (แท็บ Encryption)
        // เหตุผล: ให้ผู้ใช้เปลี่ยน Key ได้ง่าย
        private void ResetBotton2_Click(object sender, EventArgs e)
        {
            InputKey.Clear(); // ล้าง Key
        }

        // ResetBotton3 = ล้างช่อง Covertext (แท็บ Encryption)
        // เหตุผล: ให้ผู้ใช้เปลี่ยน covertext สำหรับซ่อนข้อความได้
        private void ResetBotton3_Click(object sender, EventArgs e)
        {
            InputCovertext.Clear(); // ล้าง Covertext
        }

        // ============================================================
        //  แท็บ Steganography — ฝังข้อมูล (Embed)
        // ============================================================
        //  แท็บนี้เป็นขั้นตอนที่ 2: นำ Ciphertext (จากแท็บ Encryption) + Covertext
        //  มาฝังลงใน covertext ด้วยเทคนิคที่เลือก → ได้ Steganotext
        //
        //  【รูปแบบ Sub-form Pattern】
        //  ทุกปุ่มเทคนิค (Homoglyph/Misspelling/ZWSP/NBSP/Synonym) ทำงานเหมือนกัน:
        //  1. ตรวจ input ด้วย ValidateSteganographyInput()
        //  2. สร้าง sub-form → ตั้งค่า IsEmbedMode=true, CipherText, CoverText
        //  3. เปิด sub-form ด้วย ShowDialog() (modal dialog — ต้องปิดก่อนกลับมาฟอร์มหลัก)
        //  4. ถ้าผู้ใช้กด ตกลง (DialogResult.OK) → ดึง ResultText มาแสดงใน TbStegano
        //
        //  เหตุผลที่ใช้ sub-form: แต่ละเทคนิคมี options ต่างกัน (เช่น Homoglyph ต้องเลือกคู่อักษร,
        //  Misspelling ต้องเลือกคู่คำ) จึงแยก UI ออกเป็นหน้าต่างย่อย

        // ──── ปุ่ม Homoglyph Embed (BtHm) ────
        //  ซ่อนข้อมูลโดยสลับตัวอักษรที่หน้าตาคล้ายกัน เช่น ฎ↔ฏ, ข↔ฃ, ช↔ซ
        //  bit 0 = ตัวอักษรต้นฉบับ (ซ้าย), bit 1 = ตัวอักษร glyph (ขวา)
        private void btOptions_Click(object sender, EventArgs e)
        {
            // ตรวจสอบว่ามี Ciphertext และ Covertext ครบหรือยัง ถ้าไม่ครบจะ return false
            if (!ValidateSteganographyInput()) return;

            // สร้าง sub-form OptionStaganography (Homoglyph) พร้อมตั้งค่า:
            var form = new OptionStaganography
            {
                IsEmbedMode = true,                   // โหมดฝัง (Embed) ไม่ใช่โหมดดึง (Extract)
                CipherText  = textBox1.Text.Trim(),   // Ciphertext ที่จะฝัง (ตัด whitespace หัว-ท้าย)
                CoverText   = GetCovertext()           // Covertext ที่จะใช้เป็นข้อความปกปิด (ดึงจากแท็บ Encryption)
            };

            // แสดง sub-form เป็น modal dialog (ผู้ใช้ต้องปิดก่อนกลับมาฟอร์มหลัก)
            // ถ้ากด "ตกลง" จะได้ DialogResult.OK
            if (form.ShowDialog() == DialogResult.OK)
            {
                // ดึง Steganotext ที่ฝังเสร็จแล้วมาแสดงในช่อง TbStegano
                TbStegano.Text = form.ResultText;
                // อัปเดต status bar แสดงว่าฝังสำเร็จ + จำนวนตัวอักษรของ steganotext
                tsslLabel.Text = "Homoglyph Embed สำเร็จ  |  Steganotext: " + form.ResultText.Length + " chars";
            }
        }

        // ──── ปุ่ม Misspelling Embed (BtMs) ────
        //  ซ่อนข้อมูลโดยสลับคำถูก/คำผิดที่กำหนดไว้ 65 คู่
        //  คำถูก = bit 0, คำผิด = bit 1
        //  ค้นหาแบบ greedy left-to-right: สแกนจากซ้ายไปขวา หาคำที่ตรงก่อนใช้ก่อน
        private void BtMs_Click(object sender, EventArgs e)
        {
            // ตรวจ input ว่าครบไหม
            if (!ValidateSteganographyInput()) return;

            // สร้าง sub-form Misspelling พร้อมตั้งค่าโหมดฝัง
            var form = new Misspelling
            {
                IsEmbedMode = true,                   // โหมดฝัง
                CipherText  = textBox1.Text.Trim(),   // Ciphertext ที่จะฝัง
                CoverText   = GetCovertext()           // Covertext ที่จะใช้ปกปิด
            };

            // แสดง sub-form เป็น modal dialog → ผู้ใช้เลือกคู่คำที่จะใช้ → กดตกลง
            if (form.ShowDialog() == DialogResult.OK)
            {
                TbStegano.Text = form.ResultText; // ดึง steganotext ที่ฝังเสร็จ
                tsslLabel.Text = "Misspelling Embed สำเร็จ  |  Steganotext: " + form.ResultText.Length + " chars";
            }
        }

        // ──── ปุ่ม ZWSP Embed (BtSp) ────
        //  ซ่อนข้อมูลโดยแทรก Zero-Width Space (U+200B) ระหว่างตัวอักษร
        //  มี ZWSP = bit 1, ไม่มี ZWSP = bit 0
        //  ความจุ = (จำนวนตัวอักษรใน covertext - 1) bits
        //  ข้อดี: มองไม่เห็นด้วยตาเปล่าเลย ไม่เปลี่ยนแปลงข้อความที่มนุษย์อ่าน
        private void Btsp_Click(object sender, EventArgs e)
        {
            // ตรวจ input
            if (!ValidateSteganographyInput()) return;

            // สร้าง sub-form ChkbxZWSP (ZWSP ไม่ต้องเลือก options แต่ใช้ sub-form เพื่อความสม่ำเสมอ)
            var form = new ChkbxZWSP
            {
                IsEmbedMode = true,                   // โหมดฝัง
                CipherText  = textBox1.Text.Trim(),   // Ciphertext ที่จะฝัง
                CoverText   = GetCovertext()           // Covertext
            };

            // แสดง sub-form → กดตกลง → ได้ steganotext
            if (form.ShowDialog() == DialogResult.OK)
            {
                TbStegano.Text = form.ResultText; // ดึง steganotext
                tsslLabel.Text = "ZWSP Embed สำเร็จ  |  Steganotext: " + form.ResultText.Length + " chars";
            }
        }

        // ──── ปุ่ม NBSP Embed (BtNb) ────
        //  ซ่อนข้อมูลโดยแทนที่ Space ปกติ (U+0020) ด้วย Non-Breaking Space (U+00A0)
        //  Space ปกติ = bit 0, NBSP = bit 1
        //  ความจุ = จำนวน space ใน covertext
        //  ข้อดี: มองไม่เห็นด้วยตาเปล่า (NBSP แสดงเหมือน space ปกติ)
        //  ข้อเสีย: ความจุต่ำกว่า ZWSP เพราะขึ้นกับจำนวน space ที่มีอยู่
        private void BtNb_Click(object sender, EventArgs e)
        {
            // ตรวจ input
            if (!ValidateSteganographyInput()) return;

            // สร้าง sub-form NbspForm พร้อมตั้งค่าโหมดฝัง
            var form = new NbspForm
            {
                IsEmbedMode = true,                   // โหมดฝัง
                CipherText  = textBox1.Text.Trim(),   // Ciphertext
                CoverText   = GetCovertext()           // Covertext
            };

            // แสดง sub-form → ผู้ใช้กดตกลง → ได้ steganotext
            if (form.ShowDialog() == DialogResult.OK)
            {
                TbStegano.Text = form.ResultText; // ดึง steganotext
                tsslLabel.Text = "NBSP Embed สำเร็จ  |  Steganotext: " + form.ResultText.Length + " chars";
            }
        }

        // ──── ปุ่ม Synonym Embed (BtSnn) ────
        //  ซ่อนข้อมูลโดยสลับคำพ้องความหมาย 64 กลุ่ม
        //  คำแรกในกลุ่ม = bit 0, คำที่สองในกลุ่ม = bit 1
        //  ตัวอย่าง: "กล่าว"↔"พูด", "ทำ"↔"กระทำ"
        //  ค้นหาแบบ greedy left-to-right เหมือน Misspelling
        private void BtSnn_Click(object sender, EventArgs e)
        {
            // ตรวจ input
            if (!ValidateSteganographyInput()) return;

            // สร้าง sub-form Synonym พร้อมตั้งค่าโหมดฝัง
            var form = new Synonym
            {
                IsEmbedMode = true,                   // โหมดฝัง
                CipherText  = textBox1.Text.Trim(),   // Ciphertext
                CoverText   = GetCovertext()           // Covertext
            };

            // แสดง sub-form → ผู้ใช้เลือกกลุ่มคำพ้อง → กดตกลง → ได้ steganotext
            if (form.ShowDialog() == DialogResult.OK)
            {
                TbStegano.Text = form.ResultText; // ดึง steganotext
                tsslLabel.Text = "Synonym Embed สำเร็จ  |  Steganotext: " + form.ResultText.Length + " chars";
            }
        }

        // ──── ปุ่ม Reset (BtRs3) — ล้าง Ciphertext + Steganotext ในแท็บ Steganography ────
        // เหตุผล: ให้ผู้ใช้เริ่มกระบวนการฝังใหม่ได้
        private void BtRs3_Click(object sender, EventArgs e)
        {
            textBox1.Clear();   // ล้าง Ciphertext ในช่องบน
            TbStegano.Clear();  // ล้าง Steganotext ในช่องล่าง
            tsslLabel.Text = "ล้างข้อมูล Steganography tab แล้ว"; // แจ้ง status bar
        }

        // ──── ปุ่ม Copy Steganotext — คัดลอก steganotext ไปยัง Clipboard ────
        // เหตุผล: ผู้ใช้ต้องการส่ง steganotext ให้ผู้รับ (paste ในแชท, email, ฯลฯ)
        private void CopyStegano_Click(object sender, EventArgs e)
        {
            // ตรวจว่ามี Steganotext อยู่หรือไม่
            if (!string.IsNullOrEmpty(TbStegano.Text))
            {
                Clipboard.SetText(TbStegano.Text); // คัดลอกไปยัง Clipboard
                tsslLabel.Text = "คัดลอก Steganotext แล้ว"; // แจ้งผู้ใช้
            }
            else
                // ยังไม่มี Steganotext → แจ้งเตือน
                MessageBox.Show("ไม่มี Steganotext ให้ copy", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        // ──── ปุ่ม Save Steganotext (BtS) — บันทึก steganotext เป็นไฟล์ .txt ────
        // เหตุผล: ผู้ใช้ต้องการบันทึก steganotext เป็นไฟล์เพื่อส่งให้ผู้รับ
        //         การบันทึกเป็นไฟล์ UTF-8 สำคัญมาก เพราะ ZWSP/NBSP จะถูกเก็บรักษาไว้อย่างถูกต้อง
        private void BtS_Click(object sender, EventArgs e)
        {
            // ตรวจว่ามี Steganotext ให้บันทึกไหม
            if (string.IsNullOrEmpty(TbStegano.Text))
            {
                MessageBox.Show("ไม่มี Steganotext ให้บันทึก", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // หยุดทำงาน
            }

            // สร้าง SaveFileDialog ภายใน using block เพื่อ dispose อัตโนมัติ
            using (var dlg = new SaveFileDialog
            {
                Title       = "บันทึก Steganotext",                        // ชื่อหน้าต่าง dialog
                Filter      = "Text file (*.txt)|*.txt|All files (*.*)|*.*", // filter ชนิดไฟล์
                DefaultExt  = "txt",                                        // นามสกุลเริ่มต้น
                FileName    = "steganotext.txt"                              // ชื่อไฟล์เริ่มต้น
            })
            {
                // แสดง dialog ให้ผู้ใช้เลือกตำแหน่งบันทึก
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    // เขียนเนื้อหา steganotext ลงไฟล์ด้วย UTF-8
                    // สำคัญ: ต้องใช้ UTF-8 เพื่อเก็บ invisible characters (ZWSP/NBSP) ได้ถูกต้อง
                    File.WriteAllText(dlg.FileName, TbStegano.Text, Encoding.UTF8);
                    // แจ้งผู้ใช้ว่าบันทึกสำเร็จ
                    MessageBox.Show("บันทึก Steganotext เรียบร้อย", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // อัปเดต status bar แสดงชื่อไฟล์ที่บันทึก
                    tsslLabel.Text = "บันทึกไฟล์: " + Path.GetFileName(dlg.FileName);
                }
            }
        }

        // ──── Event: textBox1 (Ciphertext ในแท็บ Steganography) เปลี่ยนค่า ────
        // เหตุผล: เมื่อ ciphertext เปลี่ยน ต้องคำนวณ bit capacity ใหม่
        //         เพื่อแสดงให้ผู้ใช้เห็นว่า covertext พอซ่อนข้อมูลได้ไหม
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            UpdateSteganoBitCapacity(); // คำนวณ + แสดง bit capacity ใหม่
        }

        // ──── Event: InputCovertext เปลี่ยนค่า ────
        // เหตุผล: เมื่อ covertext เปลี่ยน ความจุ (capacity) ก็เปลี่ยนตาม ต้องอัปเดต
        private void InputCovertext_TextChanged(object sender, EventArgs e)
        {
            UpdateSteganoBitCapacity(); // คำนวณ + แสดง bit capacity ใหม่
        }

        // ============================================================
        //  UpdateSteganoBitCapacity — คำนวณ bit ที่ต้องการ vs ความจุ covertext
        // ============================================================
        //  คำนวณว่า ciphertext ต้องการกี่ bit ในการฝัง (payload format: 32-bit length + data bits)
        //  และ covertext มีความจุกี่ bit สำหรับแต่ละเทคนิค
        //  แสดงผลใน status bar พร้อมเครื่องหมาย ✓ หรือ ✗

        /// <summary>
        /// คำนวณ bits ที่ต้องการ vs ความจุ covertext แล้วแสดงใน status bar
        /// </summary>
        private void UpdateSteganoBitCapacity()
        {
            // ดึง ciphertext จากช่อง textBox1 (ตัดช่องว่างหัว-ท้าย)
            string cipherText = textBox1.Text.Trim();
            // ดึง covertext จากช่อง InputCovertext (อยู่ในแท็บ Encryption แต่ใช้ร่วมกัน)
            string coverText  = GetCovertext();

            // ถ้า ciphertext หรือ covertext ยังว่าง → แสดง status ปกติ ไม่คำนวณ
            if (string.IsNullOrEmpty(cipherText) || string.IsNullOrEmpty(coverText))
            {
                tsslLabel.Text = "พร้อมใช้งาน"; // แสดง status เริ่มต้น
                return; // หยุดทำงาน
            }

            try // ดักจับ error จากการแปลง ciphertext เป็น bits
            {
                // แปลง ciphertext เป็น payload bits ตาม format: [32-bit length header][data bits]
                // เหตุผล: ทุกเทคนิคใช้ format เดียวกัน ต้องรู้จำนวน bit ที่ต้องการจริงๆ
                bool[] bits    = SteganographyEngine.StringToPayloadBits(cipherText);
                int needed     = bits.Length; // จำนวน bit ที่ต้องการทั้งหมด (header + data)

                // คำนวณความจุ ZWSP: ช่องว่างระหว่างตัวอักษรทุกตัว = (ความยาว - 1) slots
                // ตัวอย่าง: "ABCD" มี 3 ช่องว่าง (A|B, B|C, C|D) = 3 bits
                int zwspSlots  = Math.Max(0, coverText.Length - 1);

                // คำนวณความจุ NBSP: นับจำนวน space (ทั้ง U+0020 และ U+00A0) ใน covertext
                // NBSP ใช้ space เป็นตัวพา bit จึงความจุขึ้นกับจำนวน space
                int nbspSlots  = 0;
                foreach (char c in coverText) // วนทุกตัวอักษรใน covertext
                    if (c == ' ' || c == '\u00A0') nbspSlots++; // นับ space ปกติ + NBSP

                // แสดงผลใน status bar: จำนวน bit ที่ต้องการ + ความจุ ZWSP/NBSP + เครื่องหมายว่าพอไหม
                tsslLabel.Text = $"ต้องการ {needed} bits  |  ZWSP: {zwspSlots}  NBSP: {nbspSlots}" +
                                 (zwspSlots >= needed ? "  ✓" : "  ✗ (Covertext สั้นเกินไป)");
                // ✓ = covertext มีความจุพอ, ✗ = covertext สั้นเกินไป ต้องหา covertext ที่ยาวกว่า
            }
            catch { } // ถ้าเกิด error (เช่น ciphertext ไม่ valid) ก็ไม่ทำอะไร ไม่ crash
        }

        // ──── GetCovertext — ดึง covertext จากแท็บ Encryption ────
        // เหตุผล: InputCovertext อยู่ในแท็บ Encryption แต่ถูกใช้โดยแท็บ Steganography
        //         ฟังก์ชันนี้เป็น helper ที่รวมศูนย์การเข้าถึง covertext ไว้ที่เดียว
        //         ถ้าอนาคตต้องเปลี่ยนแหล่งที่มาของ covertext จะแก้ที่เดียว

        /// <summary>ดึง covertext จาก InputCovertext (อยู่ใน Encryption tab)</summary>
        private string GetCovertext() => InputCovertext.Text;

        // ============================================================
        //  ValidateSteganographyInput — ตรวจ input ก่อนฝังข้อมูล
        // ============================================================
        //  ตรวจว่ามี Ciphertext + Covertext ครบก่อนเริ่มฝัง
        //  ถ้า ciphertext ว่าง แต่ InputPayload มีค่า → auto-sync มาให้
        //  เหตุผล: ป้องกัน error จากการฝังข้อมูลโดยไม่มี input + ลด step ให้ผู้ใช้

        private bool ValidateSteganographyInput()
        {
            // Auto-sync: ถ้า textBox1 (Stegano tab) ว่าง แต่ InputPayload (Encryption tab) มีค่า
            // → copy มาให้อัตโนมัติ เพื่อความสะดวกของผู้ใช้
            if (string.IsNullOrEmpty(textBox1.Text) && !string.IsNullOrEmpty(InputPayload.Text))
                textBox1.Text = InputPayload.Text; // sync ciphertext

            // ตรวจว่ามี Ciphertext อยู่หรือไม่ (หลัง auto-sync แล้ว)
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                // ไม่มี Ciphertext → แจ้งเตือนให้ไปเข้ารหัสที่แท็บ Encryption ก่อน
                MessageBox.Show(
                    "กรุณาใส่ Ciphertext ในช่องด้านบน\n(เข้ารหัสจากแท็บ Encryption ก่อน แล้วมาที่แท็บนี้)",
                    "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false; // ส่งคืน false → handler จะ return ไม่ทำต่อ
            }

            // ตรวจว่ามี Covertext อยู่หรือไม่
            if (string.IsNullOrEmpty(GetCovertext()))
            {
                // ไม่มี Covertext → แจ้งเตือนให้ไปใส่ที่แท็บ Encryption
                MessageBox.Show(
                    "กรุณาใส่ Covertext (ข้อความปกปิด) ในแท็บ Encryption ด้านล่าง",
                    "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false; // ส่งคืน false
            }

            return true; // input ครบถ้วน → อนุญาตให้ดำเนินการฝังได้
        }

        // ============================================================
        //  แท็บ Decryption — ดึงข้อมูล (Extract) + ถอดรหัส AES
        // ============================================================
        //  แท็บนี้เป็นขั้นตอนที่ 3 (สุดท้าย): ผู้รับวาง Steganotext + ใส่ Key
        //  → เลือกเทคนิคเดียวกับตอนฝัง → Extract ได้ Ciphertext
        //  → ถอดรหัส AES → ได้ Plaintext ต้นฉบับ
        //
        //  【รูปแบบ Sub-form Pattern (Extract Mode)】
        //  เหมือนกับ Embed แต่ตั้งค่าต่างกัน:
        //  1. ตรวจ input ด้วย ValidateDecryptionInput()
        //  2. สร้าง sub-form → ตั้ง IsEmbedMode=false, SteganotextInput=steganotext
        //  3. เปิด sub-form → ผู้ใช้เลือก options เดิมกับตอนฝัง → กดตกลง
        //  4. ได้ ResultText = Ciphertext (Base64) → ส่งต่อให้ TriggerDecryption() ถอดรหัส AES

        // ──── ปุ่ม Homoglyph Extract (BtHmD) ────
        //  ดึงข้อมูลที่ซ่อนไว้ด้วยเทคนิค Homoglyph กลับมา
        //  ผู้ใช้ต้องเลือกคู่ตัวอักษรเดียวกับตอนฝัง มิฉะนั้นจะได้ข้อมูลผิด
        private void BtHmD_Click(object sender, EventArgs e)
        {
            // ตรวจว่ามี Steganotext + Key ครบหรือยัง
            if (!ValidateDecryptionInput()) return;

            // สร้าง sub-form Homoglyph ในโหมด Extract
            var form = new OptionStaganography
            {
                IsEmbedMode      = false,                   // โหมดดึง (Extract) ไม่ใช่โหมดฝัง
                SteganotextInput = InputChipertext.Text      // Steganotext ที่จะดึงข้อมูลออก
            };

            // แสดง sub-form → ผู้ใช้เลือกคู่อักษรเดิม → กดตกลง
            if (form.ShowDialog() == DialogResult.OK)
                // ResultText = Ciphertext (Base64) ที่ดึงออกมาได้ → ส่งต่อให้ถอดรหัส AES
                TriggerDecryption(form.ResultText);
        }

        // ──── ปุ่ม Misspelling Extract (BtMsD) ────
        //  ดึงข้อมูลที่ซ่อนไว้ด้วยเทคนิค Misspelling กลับมา
        //  ผู้ใช้ต้องเลือกคู่คำเดียวกับตอนฝัง
        private void BtMsD_Click(object sender, EventArgs e)
        {
            // ตรวจ input
            if (!ValidateDecryptionInput()) return;

            // สร้าง sub-form Misspelling ในโหมด Extract
            var form = new Misspelling
            {
                IsEmbedMode      = false,                   // โหมดดึง
                SteganotextInput = InputChipertext.Text      // Steganotext ที่จะดึง
            };

            // แสดง sub-form → ผู้ใช้เลือกคู่คำเดิม → กดตกลง → ได้ ciphertext
            if (form.ShowDialog() == DialogResult.OK)
                TriggerDecryption(form.ResultText); // ส่ง ciphertext ไปถอดรหัส AES
        }

        // ──── ปุ่ม ZWSP Extract (BtSpD) ────
        //  ดึงข้อมูลที่ซ่อนไว้ด้วย Zero-Width Space กลับมา
        //  ZWSP ไม่ต้องเลือก options เพราะใช้วิธีเดียว (ตรวจ ZWSP ระหว่างตัวอักษร)
        private void BtSpD_Click(object sender, EventArgs e)
        {
            // ตรวจ input
            if (!ValidateDecryptionInput()) return;

            // สร้าง sub-form ZWSP ในโหมด Extract
            var form = new ChkbxZWSP
            {
                IsEmbedMode      = false,                   // โหมดดึง
                SteganotextInput = InputChipertext.Text      // Steganotext ที่จะดึง
            };

            // แสดง sub-form → กดตกลง → ได้ ciphertext
            if (form.ShowDialog() == DialogResult.OK)
                TriggerDecryption(form.ResultText); // ส่ง ciphertext ไปถอดรหัส AES
        }

        // ──── ปุ่ม NBSP Extract (BtNbD) ────
        //  ดึงข้อมูลที่ซ่อนไว้ด้วย Non-Breaking Space กลับมา
        //  NBSP ก็ไม่ต้องเลือก options เพราะใช้วิธีเดียว (ตรวจ NBSP vs Space ปกติ)
        private void BtNbD_Click(object sender, EventArgs e)
        {
            // ตรวจ input
            if (!ValidateDecryptionInput()) return;

            // สร้าง sub-form NBSP ในโหมด Extract
            var form = new NbspForm
            {
                IsEmbedMode      = false,                   // โหมดดึง
                SteganotextInput = InputChipertext.Text      // Steganotext ที่จะดึง
            };

            // แสดง sub-form → กดตกลง → ได้ ciphertext
            if (form.ShowDialog() == DialogResult.OK)
                TriggerDecryption(form.ResultText); // ส่ง ciphertext ไปถอดรหัส AES
        }

        // ──── ปุ่ม Synonym Extract (BtSnnD) ────
        //  ดึงข้อมูลที่ซ่อนไว้ด้วยเทคนิค Synonym กลับมา
        //  ผู้ใช้ต้องเลือกกลุ่มคำพ้องเดียวกับตอนฝัง
        private void BtSnnD_Click(object sender, EventArgs e)
        {
            // ตรวจ input
            if (!ValidateDecryptionInput()) return;

            // สร้าง sub-form Synonym ในโหมด Extract
            var form = new Synonym
            {
                IsEmbedMode      = false,                   // โหมดดึง
                SteganotextInput = InputChipertext.Text      // Steganotext ที่จะดึง
            };

            // แสดง sub-form → ผู้ใช้เลือกกลุ่มคำเดิม → กดตกลง → ได้ ciphertext
            if (form.ShowDialog() == DialogResult.OK)
                TriggerDecryption(form.ResultText); // ส่ง ciphertext ไปถอดรหัส AES
        }

        // ──── ปุ่ม Decryption หลัก (DecrytionBotton10) ────
        //  ถอดรหัส AES โดยตรงจาก Ciphertext ที่กรอกในช่อง InputChipertext
        //  ใช้เมื่อผู้ใช้มี Ciphertext (Base64) อยู่แล้ว ไม่ต้อง extract จาก steganotext
        //  ตัวอย่างการใช้: ได้รับ ciphertext ทาง email แล้ว paste มาถอดรหัสตรงๆ
        private void DecrytionBotton10_Click(object sender, EventArgs e)
        {
            // ตรวจว่ามี Ciphertext ในช่อง InputChipertext หรือไม่
            if (string.IsNullOrEmpty(InputChipertext.Text))
            {
                MessageBox.Show("กรุณาใส่ Steganotext หรือ Ciphertext (Base64) ก่อน",
                    "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // หยุดทำงาน
            }

            // ส่ง ciphertext ไปถอดรหัส AES (Trim เพื่อตัด whitespace ที่อาจติดมาจากการ copy-paste)
            TriggerDecryption(InputChipertext.Text.Trim());
        }

        // ============================================================
        //  TriggerDecryption — ถอดรหัส AES-256 จาก Ciphertext (Base64) → Plaintext
        // ============================================================
        //  ฟังก์ชันกลางที่ถูกเรียกจากทุกปุ่ม extract และปุ่ม Decryption หลัก
        //  รับ cipherBase64 (ที่ได้จาก extract หรือกรอกตรง) → ถอดรหัส AES → แสดง plaintext
        //  เหตุผลที่แยกเป็นฟังก์ชัน: ลด code ซ้ำ เพราะทุกเทคนิคใช้ขั้นตอนถอดรหัสเหมือนกัน

        private void TriggerDecryption(string cipherBase64)
        {
            // ตรวจว่ามี Key สำหรับถอดรหัสหรือยัง
            // เหตุผล: AES ต้องใช้ Key เดียวกับตอนเข้ารหัส ไม่มี Key ถอดไม่ได้
            if (string.IsNullOrEmpty(InputkeyDecry.Text))
            {
                MessageBox.Show("กรุณาใส่ Key สำหรับถอดรหัส",
                    "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // หยุดทำงาน
            }

            try // ดักจับ error จากการถอดรหัส (Key ผิด, Base64 เสียหาย, padding ไม่ถูกต้อง)
            {
                // เรียก CryptoHelper.AesDecrypt() เพื่อถอดรหัส
                // Input: ciphertext (Base64) + key (password เดียวกับตอนเข้ารหัส)
                // Output: plaintext ต้นฉบับ
                // ภายใน: ใช้ PBKDF2 แปลง password → AES key + IV เดียวกับตอนเข้ารหัส
                //         เพราะใช้ salt คงที่ ("Stegano2025Educa") + iterations เท่ากัน (10,000)
                string plaintext = CryptoHelper.AesDecrypt(cipherBase64, InputkeyDecry.Text);

                // แสดง plaintext ที่ถอดรหัสได้ในช่อง OutputPlaintext
                OutputPlaintext.Text = plaintext;

                // อัปเดต status bar แสดงว่าถอดรหัสสำเร็จ + จำนวนตัวอักษร
                tsslLabel.Text = "ถอดรหัสสำเร็จ  |  Plaintext: " + plaintext.Length + " chars";
            }
            catch (Exception ex) // ดักจับ error — มักเกิดจาก Key ผิดหรือ ciphertext เสียหาย
            {
                // แจ้งผู้ใช้พร้อมแนะนำสาเหตุที่เป็นไปได้ทั้งหมด
                // เหตุผลที่ list ไว้หลายข้อ: ให้ผู้ใช้ตรวจสอบได้ด้วยตัวเอง ลดการถามซ้ำ
                MessageBox.Show(
                    "ถอดรหัสไม่สำเร็จ อาจเกิดจาก:\n" +
                    "• Key ผิด\n" +                                        // สาเหตุที่พบบ่อยที่สุด
                    "• เลือกเทคนิค Steganography ผิด (ต้องใช้เทคนิคเดียวกับตอนฝัง)\n" +  // ใช้ Homoglyph ฝัง แต่ Misspelling ดึง
                    "• เลือกคู่ตัวอักษร/คำผิด (ต้องเลือกเหมือนตอนฝัง)\n" + // เลือก checkbox ไม่ตรง
                    "• Steganotext เสียหายหรือถูกแก้ไข\n\n" +               // copy-paste ไม่ครบ หรือ editor ลบ invisible chars
                    "รายละเอียด: " + ex.Message,                           // แสดง error message จริงสำหรับ debug
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============================================================
        //  ValidateDecryptionInput — ตรวจ input ก่อนเริ่ม extract
        // ============================================================
        //  ตรวจว่ามี Steganotext + Key ครบก่อนเปิด sub-form
        //  เหตุผล: ถ้าไม่ตรวจก่อน ผู้ใช้จะเสียเวลาเลือก options ใน sub-form แล้วพบว่าไม่มี input

        private bool ValidateDecryptionInput()
        {
            // ตรวจว่ามี Steganotext อยู่หรือไม่
            if (string.IsNullOrEmpty(InputChipertext.Text))
            {
                MessageBox.Show("กรุณาวาง Steganotext ในช่องด้านบน",
                    "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false; // ไม่มี → ส่งคืน false → handler จะ return
            }

            // ตรวจว่ามี Key อยู่หรือไม่
            if (string.IsNullOrEmpty(InputkeyDecry.Text))
            {
                MessageBox.Show("กรุณาใส่ Key สำหรับถอดรหัสก่อนเลือกเทคนิค",
                    "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false; // ไม่มี → ส่งคืน false
            }

            return true; // input ครบถ้วน → อนุญาตให้ดำเนินการ extract ได้
        }

        // ──── ปุ่ม Paste Key (PasteButton1) — วาง Key จาก Clipboard ลงช่อง Decryption ────
        // เหตุผล: ผู้ใช้ copy Key จากแท็บ Encryption แล้ว paste มาที่แท็บ Decryption ได้สะดวก
        //         ทำงานคู่กับ CopyButton1 (Copy Key ในแท็บ Encryption)
        private void PasteButton1_Click(object sender, EventArgs e)
        {
            // ตรวจว่า Clipboard มีข้อความอยู่หรือไม่
            if (Clipboard.ContainsText())
            {
                InputkeyDecry.Text = Clipboard.GetText(); // วาง Key จาก Clipboard
                tsslLabel.Text = "วาง Key แล้ว"; // แจ้งผ่าน status bar
            }
        }

        // ──── BtRs4 = ล้างช่อง Steganotext input (แท็บ Decryption) ────
        // เหตุผล: ให้ผู้ใช้วาง steganotext ใหม่ได้
        private void ResetBotton4_Click(object sender, EventArgs e)
        {
            InputChipertext.Clear(); // ล้าง Steganotext
            tsslLabel.Text = "ล้าง Steganotext แล้ว"; // แจ้ง status bar
        }

        // ──── BtRs5 = ล้างช่อง Key (แท็บ Decryption) ────
        // เหตุผล: ให้ผู้ใช้เปลี่ยน Key ได้
        private void BtRs5_Click(object sender, EventArgs e)
        {
            InputkeyDecry.Clear(); // ล้าง Key
        }

        // ──── BtRs6 = ล้างช่อง Plaintext output (แท็บ Decryption) ────
        // เหตุผล: ล้างผลลัพธ์เก่าก่อนถอดรหัสใหม่
        private void BtRs6_Click(object sender, EventArgs e)
        {
            OutputPlaintext.Clear(); // ล้าง Plaintext
            tsslLabel.Text = "ล้าง Plaintext แล้ว"; // แจ้ง status bar
        }

        // ──── CopyButton3 = คัดลอก Plaintext output ไปยัง Clipboard ────
        // เหตุผล: ผู้ใช้ต้องการ copy ข้อความที่ถอดรหัสได้ไปใช้งาน
        private void CopyButton3_Click(object sender, EventArgs e)
        {
            // ตรวจว่ามี Plaintext ให้ copy หรือไม่
            if (!string.IsNullOrEmpty(OutputPlaintext.Text))
            {
                Clipboard.SetText(OutputPlaintext.Text); // คัดลอกไปยัง Clipboard
                tsslLabel.Text = "คัดลอก Plaintext แล้ว"; // แจ้งผ่าน status bar
            }
            else
                // ยังไม่มี Plaintext → แจ้งให้ถอดรหัสก่อน
                MessageBox.Show("ไม่มี Plaintext ให้ copy — ถอดรหัสก่อน", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        // ──── ปุ่ม Save Plaintext (BtSavePlain) — บันทึก plaintext ที่ถอดรหัสได้เป็นไฟล์ ────
        // เหตุผล: ข้อความที่ถอดรหัสได้อาจยาวมาก การบันทึกเป็นไฟล์สะดวกกว่า copy-paste
        private void BtSavePlain_Click(object sender, EventArgs e)
        {
            // ตรวจว่ามี Plaintext ให้บันทึกหรือไม่
            if (string.IsNullOrEmpty(OutputPlaintext.Text))
            {
                MessageBox.Show("ไม่มี Plaintext ให้บันทึก — ถอดรหัสก่อน", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // หยุดทำงาน
            }

            // สร้าง SaveFileDialog ภายใน using block เพื่อ dispose อัตโนมัติ
            using (var dlg = new SaveFileDialog
            {
                Title      = "บันทึก Plaintext",                            // ชื่อหน้าต่าง dialog
                Filter     = "Text file (*.txt)|*.txt|All files (*.*)|*.*",  // filter ชนิดไฟล์
                DefaultExt = "txt",                                          // นามสกุลเริ่มต้น
                FileName   = "plaintext.txt"                                 // ชื่อไฟล์เริ่มต้น
            })
            {
                // แสดง dialog ให้ผู้ใช้เลือกตำแหน่งบันทึก
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    // เขียน plaintext ลงไฟล์ด้วย UTF-8 (รองรับภาษาไทยครบถ้วน)
                    File.WriteAllText(dlg.FileName, OutputPlaintext.Text, Encoding.UTF8);
                    // แจ้งผู้ใช้ว่าบันทึกสำเร็จ
                    MessageBox.Show("บันทึก Plaintext เรียบร้อย", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // อัปเดต status bar แสดงชื่อไฟล์ที่บันทึก
                    tsslLabel.Text = "บันทึกไฟล์: " + Path.GetFileName(dlg.FileName);
                }
            }
        }


        // ============================================================
        //  Menu Handlers — เมนูบนสุดของหน้าต่าง (MenuStrip)
        // ============================================================
        //  เมนูเหล่านี้ให้ข้อมูลเกี่ยวกับโปรแกรม ไม่มีผลต่อ logic การฝัง/ดึง

        // เมนู "Exit" — ปิดโปรแกรม
        // เหตุผล: ให้ผู้ใช้ปิดโปรแกรมผ่านเมนูได้ (นอกจากกดปุ่ม X)
        // ใช้ expression-bodied member (=>) เพราะมีแค่บรรทัดเดียว
        private void exitToolStripMenuItem_Click(object sender, EventArgs e) => Close();

        // เมนู "วิธีใช้งาน" — แสดง dialog อธิบายขั้นตอนการใช้งานทั้งหมด
        // เหตุผล: ให้ผู้ใช้ใหม่เข้าใจ flow: Encryption → Steganography → Decryption
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // แสดง MessageBox ที่มีข้อความอธิบายขั้นตอนทั้ง 3 ขั้นตอน
            MessageBox.Show(
                "วิธีใช้งานโปรแกรม\n\n" +
                "─── ขั้นตอนซ่อนข้อความ ───\n" +
                "1. [แท็บ Encryption]\n" +
                "   • ใส่ Plaintext (หรือกด Browser โหลดไฟล์)\n" +
                "   • ใส่ Key (รหัสผ่าน)\n" +
                "   • ใส่ Covertext (ข้อความที่จะใช้ปกปิด)\n" +
                "   • กด Encryption → ได้ Ciphertext\n\n" +
                "2. [แท็บ Steganography]\n" +
                "   • Ciphertext จะ sync มาอัตโนมัติ\n" +
                "   • เลือกเทคนิค: Homoglyph / Misspelling / Space / Synonym\n" +
                "   • เลือก options ใน dialog แล้วกด ตกลง\n" +
                "   • ได้ Steganotext → กด Save บันทึก\n\n" +
                "─── ขั้นตอนถอดข้อความ ───\n" +
                "3. [แท็บ Decryption]\n" +
                "   • วาง Steganotext ในช่องบน\n" +
                "   • ใส่ Key เดิม\n" +
                "   • เลือกเทคนิค + options เดิมกับตอนฝัง\n" +
                "   • กดปุ่มเทคนิค → ได้ Plaintext",
                "วิธีใช้งาน",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        // เมนู "ข้อมูล Steganography" — อธิบายเทคนิคทั้ง 4 แบบ + payload format
        // เหตุผล: ให้ผู้ใช้เข้าใจว่าแต่ละเทคนิคทำงานอย่างไร ข้อดี/ข้อเสีย
        private void steganographyInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "เทคนิค Steganography ที่รองรับ\n\n" +
                "1. Homoglyph\n" +
                "   แทนตัวอักษรไทยที่หน้าตาคล้ายกัน\n" +
                "   ฎ↔ฏ  เ↔แ  ด↔ต  ข↔ฃ  ช↔ซ\n" +
                "   ข้อจำกัด: Covertext ต้องมีตัวอักษรที่เลือกเพียงพอ\n\n" +
                "2. Misspelling\n" +
                "   แทนคำด้วยการสะกดผิดที่กำหนดไว้ล่วงหน้า\n" +
                "   คำถูก = bit 0 / คำผิด = bit 1\n" +
                "   มีคู่คำให้เลือก 64 คู่\n\n" +
                "3. Zero-Width Space (ZWSP)\n" +
                "   แทรก U+200B ระหว่างตัวอักษร\n" +
                "   มี ZWSP = bit 1 / ไม่มี = bit 0\n" +
                "   ความจุ = จำนวนตัวอักษรใน Covertext - 1\n\n" +
                "4. Synonym\n" +
                "   สลับคำพ้องความหมายในกลุ่มที่เลือก\n" +
                "   คำแรกในกลุ่ม = bit 0 / คำที่สองในกลุ่ม = bit 1\n" +
                "   มีกลุ่มคำพ้องให้เลือก 64 กลุ่ม\n\n" +
                "Payload format: [32-bit length][data bits] (UTF-8, MSB first)",
                "ข้อมูล Steganography",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        // เมนู "รูปแบบไฟล์" — อธิบายชนิดไฟล์ที่รองรับ + ข้อควรระวัง
        // เหตุผล: เตือนว่าห้ามเปิด steganotext ด้วย editor ที่ลบ invisible chars (เช่น Word)
        private void fileFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "รูปแบบไฟล์ที่รองรับ\n\n" +
                "• .txt  — Text file (UTF-8) แนะนำ\n" +
                "• .pdf / .doc / .docx — อ่านได้เฉพาะ plain text เท่านั้น\n\n" +
                "Steganotext ที่บันทึกจะเป็น .txt (UTF-8)\n" +
                "ห้ามเปิดไฟล์ด้วย editor ที่ strip invisible characters\n" +
                "เพราะจะลบ ZWSP ออก (สำหรับเทคนิค Space)",
                "รูปแบบไฟล์",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        // เมนู "Security Warning" — แจ้งเตือนความปลอดภัย + ข้อจำกัด
        // เหตุผล: โปรแกรมนี้สร้างเพื่อการศึกษา ไม่ควรใช้กับข้อมูลลับจริง
        //         Salt คงที่ทำให้ AES อ่อนแอกว่ามาตรฐาน และเทคนิค steganography ตรวจจับได้
        private void securityNoticeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "คำเตือนด้านความปลอดภัย\n\n" +
                "โปรแกรมนี้จัดทำขึ้นเพื่อการศึกษาเท่านั้น\n" +
                "ไม่ควรใช้กับข้อมูลลับหรือข้อมูลสำคัญจริง\n\n" +
                "ข้อจำกัดที่ควรทราบ:\n" +
                "• AES Salt คงที่ (ลด security เล็กน้อย)\n" +
                "• Homoglyph/Misspelling ตรวจจับได้ด้วยสายตา\n" +
                "• ZWSP ตรวจจับได้ด้วย text analyzer\n\n" +
                "ผู้พัฒนาไม่รับผิดชอบต่อความเสียหายใดๆ",
                "Security Warning",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }

    }
}
