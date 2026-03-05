using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Steganography
{
    public partial class SteganographyForm : Form
    {
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB
        System.Windows.Forms.ToolTip tt = new System.Windows.Forms.ToolTip();

        public SteganographyForm()
        {
            InitializeComponent();
        }

        // ============================================================
        //  Form Load — ตั้งค่าเริ่มต้น
        // ============================================================

        private void SteganographyForm_Load(object sender, EventArgs e)
        {
            tsslLabel.Text = "พร้อมใช้งาน";
            tt.SetToolTip(InputKey, "Key ที่ใช้เข้ารหัส AES-256 (PBKDF2, 10,000 iterations)");
            tt.SetToolTip(InputkeyDecry, "Key เดียวกับที่ใช้ตอนเข้ารหัส");
            tt.SetToolTip(TbStegano, "Steganotext ที่ซ่อนข้อมูลลับไว้ (ReadOnly)");

            // ซ่อน Key เป็น ●●●● เพื่อความปลอดภัย
            InputKey.PasswordChar      = '●';
            InputkeyDecry.PasswordChar = '●';

            // Drag & Drop — InputPlaintext (Encryption tab)
            InputPlaintext.AllowDrop = true;
            InputPlaintext.DragEnter += (s, ev) =>
                ev.Effect = ev.Data.GetDataPresent(DataFormats.FileDrop)
                    ? DragDropEffects.Copy
                    : DragDropEffects.None;
            InputPlaintext.DragDrop += (s, ev) =>
            {
                var files = (string[])ev.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                    LoadFileIntoTextBox(files[0], InputPlaintext, namefile);
            };

            // Drag & Drop — InputCovertext (Encryption tab)
            InputCovertext.AllowDrop = true;
            InputCovertext.DragEnter += (s, ev) =>
                ev.Effect = ev.Data.GetDataPresent(DataFormats.FileDrop)
                    ? DragDropEffects.Copy
                    : DragDropEffects.None;
            InputCovertext.DragDrop += (s, ev) =>
            {
                var files = (string[])ev.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                    LoadFileIntoTextBox(files[0], InputCovertext, null);
            };

            // Drag & Drop — InputChipertext (Decryption tab)
            InputChipertext.AllowDrop = true;
            InputChipertext.DragEnter += (s, ev) =>
                ev.Effect = ev.Data.GetDataPresent(DataFormats.FileDrop)
                    ? DragDropEffects.Copy
                    : DragDropEffects.None;
            InputChipertext.DragDrop += (s, ev) =>
            {
                var files = (string[])ev.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                    LoadFileIntoTextBox(files[0], InputChipertext, null);
            };
        }

        /// <summary>
        /// โหลดเนื้อหาจากไฟล์ .txt เข้า TextBox ที่กำหนด
        /// </summary>
        private void LoadFileIntoTextBox(string filePath, System.Windows.Forms.TextBox target, System.Windows.Forms.TextBox fileNameLabel)
        {
            try
            {
                var fi = new FileInfo(filePath);
                if (fi.Length > MaxFileSizeBytes)
                {
                    MessageBox.Show($"ไฟล์ใหญ่เกินไป ({fi.Length / 1024} KB)\nรองรับสูงสุด {MaxFileSizeBytes / 1024} KB",
                        "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                string content = File.ReadAllText(filePath, Encoding.UTF8);
                target.Text = content;
                if (fileNameLabel != null)
                    fileNameLabel.Text = Path.GetFileName(filePath);
                tsslLabel.Text = "โหลดไฟล์: " + Path.GetFileName(filePath) + "  |  " + content.Length + " chars";
            }
            catch (Exception ex)
            {
                MessageBox.Show("ไม่สามารถอ่านไฟล์ได้:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============================================================
        //  Tab Changed — Sync Ciphertext อัตโนมัติ
        // ============================================================

        private void TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // เมื่อเปลี่ยนไป Steganography tab → sync ciphertext จาก Encryption tab
            if (TabControl1.SelectedTab == TabSteganography)
            {
                if (string.IsNullOrEmpty(textBox1.Text) && !string.IsNullOrEmpty(InputPayload.Text))
                {
                    textBox1.Text = InputPayload.Text;
                    tsslLabel.Text = "Sync Ciphertext จาก Encryption tab แล้ว";
                }
                UpdateSteganoBitCapacity();
            }
        }

        // ============================================================
        //  แท็บ Encryption
        // ============================================================

        private void EncrytionButton1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(InputKey.Text))
            {
                MessageBox.Show("กรุณาใส่ Key ก่อนเข้ารหัส", "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(InputPlaintext.Text))
            {
                MessageBox.Show("กรุณาใส่ข้อความ Plaintext ก่อนเข้ารหัส", "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string cipherBase64 = CryptoHelper.AesEncrypt(InputPlaintext.Text, InputKey.Text);
                InputPayload.Text = cipherBase64;
                // sync ไปที่ Steganography tab ทันที
                textBox1.Text = cipherBase64;
                tsslLabel.Text = "เข้ารหัสสำเร็จ  |  Ciphertext: " + cipherBase64.Length + " chars";
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาดในการเข้ารหัส:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Browser_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "Text file (*.txt)|*.txt|All files (*.*)|*.*";
                dlg.Multiselect = false;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    var fi = new FileInfo(dlg.FileName);
                    if (fi.Length > MaxFileSizeBytes)
                    {
                        MessageBox.Show($"ไฟล์ใหญ่เกินไป ({fi.Length / 1024} KB)\nรองรับสูงสุด {MaxFileSizeBytes / 1024} KB",
                            "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    namefile.Text = Path.GetFileName(dlg.FileName);
                    try
                    {
                        string content = File.ReadAllText(dlg.FileName, Encoding.UTF8);
                        InputPlaintext.Text = content;
                        tsslLabel.Text = "โหลดไฟล์: " + namefile.Text + "  |  " + content.Length + " chars";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("ไม่สามารถอ่านไฟล์ได้:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // Copy/Reset สำหรับ Encryption tab
        private void CopyButton1_Click(object sender, EventArgs e)
        {
            // CopyButton1 = Copy Key
            if (!string.IsNullOrEmpty(InputKey.Text))
            {
                Clipboard.SetText(InputKey.Text);
                tsslLabel.Text = "คัดลอก Key แล้ว";
            }
            else
                MessageBox.Show("ไม่มี Key ให้ copy", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void CopyButton2_Click(object sender, EventArgs e)
        {
            // CopyButton2 = Copy Ciphertext (อยู่ใน Encryption tab)
            if (!string.IsNullOrEmpty(InputPayload.Text))
            {
                Clipboard.SetText(InputPayload.Text);
                tsslLabel.Text = "คัดลอก Ciphertext แล้ว";
            }
            else
                MessageBox.Show("ไม่มี Ciphertext ให้ copy — กด Encryption ก่อน", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void ResetBotton1_Click(object sender, EventArgs e)
        {
            // BtRs1 = Reset Plaintext + ชื่อไฟล์
            InputPlaintext.Clear();
            namefile.Clear();
            tsslLabel.Text = "ล้าง Plaintext แล้ว";
        }

        private void ResetBotton2_Click(object sender, EventArgs e)
        {
            // BtRs2 = Reset Key (Encryption tab)
            InputKey.Clear();
        }

        private void ResetBotton3_Click(object sender, EventArgs e)
        {
            // ResetBotton3 = Reset Covertext (Encryption tab)
            InputCovertext.Clear();
        }

        // ============================================================
        //  แท็บ Steganography — Embed
        // ============================================================

        private void btOptions_Click(object sender, EventArgs e)
        {
            // BtHm = Homoglyph Embed
            if (!ValidateSteganographyInput()) return;

            var form = new OptionStaganography
            {
                IsEmbedMode = true,
                CipherText  = textBox1.Text.Trim(),
                CoverText   = GetCovertext()
            };

            if (form.ShowDialog() == DialogResult.OK)
            {
                TbStegano.Text = form.ResultText;
                tsslLabel.Text = "Homoglyph Embed สำเร็จ  |  Steganotext: " + form.ResultText.Length + " chars";
            }
        }

        private void BtMs_Click(object sender, EventArgs e)
        {
            // Misspelling Embed
            if (!ValidateSteganographyInput()) return;

            var form = new Misspelling
            {
                IsEmbedMode = true,
                CipherText  = textBox1.Text.Trim(),
                CoverText   = GetCovertext()
            };

            if (form.ShowDialog() == DialogResult.OK)
            {
                TbStegano.Text = form.ResultText;
                tsslLabel.Text = "Misspelling Embed สำเร็จ  |  Steganotext: " + form.ResultText.Length + " chars";
            }
        }

        private void Btsp_Click(object sender, EventArgs e)
        {
            // ZWSP Embed
            if (!ValidateSteganographyInput()) return;

            var form = new ChkbxZWSP
            {
                IsEmbedMode = true,
                CipherText  = textBox1.Text.Trim(),
                CoverText   = GetCovertext()
            };

            if (form.ShowDialog() == DialogResult.OK)
            {
                TbStegano.Text = form.ResultText;
                tsslLabel.Text = "ZWSP Embed สำเร็จ  |  Steganotext: " + form.ResultText.Length + " chars";
            }
        }

        private void BtNb_Click(object sender, EventArgs e)
        {
            // NBSP Embed
            if (!ValidateSteganographyInput()) return;

            var form = new NbspForm
            {
                IsEmbedMode = true,
                CipherText  = textBox1.Text.Trim(),
                CoverText   = GetCovertext()
            };

            if (form.ShowDialog() == DialogResult.OK)
            {
                TbStegano.Text = form.ResultText;
                tsslLabel.Text = "NBSP Embed สำเร็จ  |  Steganotext: " + form.ResultText.Length + " chars";
            }
        }

        private void BtSnn_Click(object sender, EventArgs e)
        {
            // Synonym Embed
            if (!ValidateSteganographyInput()) return;

            var form = new Synonym
            {
                IsEmbedMode = true,
                CipherText  = textBox1.Text.Trim(),
                CoverText   = GetCovertext()
            };

            if (form.ShowDialog() == DialogResult.OK)
            {
                TbStegano.Text = form.ResultText;
                tsslLabel.Text = "Synonym Embed สำเร็จ  |  Steganotext: " + form.ResultText.Length + " chars";
            }
        }

        private void BtRs3_Click(object sender, EventArgs e)
        {
            // BtRs3 = Reset Ciphertext ใน Steganography tab
            textBox1.Clear();
            TbStegano.Clear();
            tsslLabel.Text = "ล้างข้อมูล Steganography tab แล้ว";
        }

        private void CopyStegano_Click(object sender, EventArgs e)
        {
            // Copy Steganotext
            if (!string.IsNullOrEmpty(TbStegano.Text))
            {
                Clipboard.SetText(TbStegano.Text);
                tsslLabel.Text = "คัดลอก Steganotext แล้ว";
            }
            else
                MessageBox.Show("ไม่มี Steganotext ให้ copy", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void BtS_Click(object sender, EventArgs e)
        {
            // Save Steganotext
            if (string.IsNullOrEmpty(TbStegano.Text))
            {
                MessageBox.Show("ไม่มี Steganotext ให้บันทึก", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using (var dlg = new SaveFileDialog
            {
                Title       = "บันทึก Steganotext",
                Filter      = "Text file (*.txt)|*.txt|All files (*.*)|*.*",
                DefaultExt  = "txt",
                FileName    = "steganotext.txt"
            })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(dlg.FileName, TbStegano.Text, Encoding.UTF8);
                    MessageBox.Show("บันทึก Steganotext เรียบร้อย", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    tsslLabel.Text = "บันทึกไฟล์: " + Path.GetFileName(dlg.FileName);
                }
            }
        }

        // textBox1 (Ciphertext ใน Stegano tab) เปลี่ยน → อัปเดต capacity
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            UpdateSteganoBitCapacity();
        }

        // InputCovertext เปลี่ยน → อัปเดต capacity
        private void InputCovertext_TextChanged(object sender, EventArgs e)
        {
            UpdateSteganoBitCapacity();
        }

        /// <summary>
        /// คำนวณ bits ที่ต้องการ vs ความจุ covertext แล้วแสดงใน status bar
        /// </summary>
        private void UpdateSteganoBitCapacity()
        {
            string cipherText = textBox1.Text.Trim();
            string coverText  = GetCovertext();

            if (string.IsNullOrEmpty(cipherText) || string.IsNullOrEmpty(coverText))
            {
                tsslLabel.Text = "พร้อมใช้งาน";
                return;
            }

            try
            {
                bool[] bits    = SteganographyEngine.StringToPayloadBits(cipherText);
                int needed     = bits.Length;
                int zwspSlots  = Math.Max(0, coverText.Length - 1);
                int nbspSlots  = 0;
                foreach (char c in coverText)
                    if (c == ' ' || c == '\u00A0') nbspSlots++;
                tsslLabel.Text = $"ต้องการ {needed} bits  |  ZWSP: {zwspSlots}  NBSP: {nbspSlots}" +
                                 (zwspSlots >= needed ? "  ✓" : "  ✗ (Covertext สั้นเกินไป)");
            }
            catch { }
        }

        /// <summary>ดึง covertext จาก InputCovertext (อยู่ใน Encryption tab)</summary>
        private string GetCovertext() => InputCovertext.Text;

        private bool ValidateSteganographyInput()
        {
            // auto-sync จาก Encryption tab
            if (string.IsNullOrEmpty(textBox1.Text) && !string.IsNullOrEmpty(InputPayload.Text))
                textBox1.Text = InputPayload.Text;

            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show(
                    "กรุณาใส่ Ciphertext ในช่องด้านบน\n(เข้ารหัสจากแท็บ Encryption ก่อน แล้วมาที่แท็บนี้)",
                    "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (string.IsNullOrEmpty(GetCovertext()))
            {
                MessageBox.Show(
                    "กรุณาใส่ Covertext (ข้อความปกปิด) ในแท็บ Encryption ด้านล่าง",
                    "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        // ============================================================
        //  แท็บ Decryption — Extract + Decrypt
        // ============================================================

        private void BtHmD_Click(object sender, EventArgs e)
        {
            if (!ValidateDecryptionInput()) return;
            var form = new OptionStaganography
            {
                IsEmbedMode      = false,
                SteganotextInput = InputChipertext.Text
            };
            if (form.ShowDialog() == DialogResult.OK)
                TriggerDecryption(form.ResultText);
        }

        private void BtMsD_Click(object sender, EventArgs e)
        {
            if (!ValidateDecryptionInput()) return;
            var form = new Misspelling
            {
                IsEmbedMode      = false,
                SteganotextInput = InputChipertext.Text
            };
            if (form.ShowDialog() == DialogResult.OK)
                TriggerDecryption(form.ResultText);
        }

        private void BtSpD_Click(object sender, EventArgs e)
        {
            if (!ValidateDecryptionInput()) return;
            var form = new ChkbxZWSP
            {
                IsEmbedMode      = false,
                SteganotextInput = InputChipertext.Text
            };
            if (form.ShowDialog() == DialogResult.OK)
                TriggerDecryption(form.ResultText);
        }

        private void BtNbD_Click(object sender, EventArgs e)
        {
            // NBSP Extract
            if (!ValidateDecryptionInput()) return;

            var form = new NbspForm
            {
                IsEmbedMode      = false,
                SteganotextInput = InputChipertext.Text
            };

            if (form.ShowDialog() == DialogResult.OK)
                TriggerDecryption(form.ResultText);
        }

        private void BtSnnD_Click(object sender, EventArgs e)
        {
            // Synonym Extract
            if (!ValidateDecryptionInput()) return;

            var form = new Synonym
            {
                IsEmbedMode      = false,
                SteganotextInput = InputChipertext.Text
            };

            if (form.ShowDialog() == DialogResult.OK)
                TriggerDecryption(form.ResultText);
        }

        private void DecrytionBotton10_Click(object sender, EventArgs e)
        {
            // ปุ่ม Decryption หลัก: ถอดรหัส AES จาก ciphertext ที่กรอกตรงๆ (ไม่ผ่าน extract)
            if (string.IsNullOrEmpty(InputChipertext.Text))
            {
                MessageBox.Show("กรุณาใส่ Steganotext หรือ Ciphertext (Base64) ก่อน",
                    "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            TriggerDecryption(InputChipertext.Text.Trim());
        }

        private void TriggerDecryption(string cipherBase64)
        {
            if (string.IsNullOrEmpty(InputkeyDecry.Text))
            {
                MessageBox.Show("กรุณาใส่ Key สำหรับถอดรหัส",
                    "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string plaintext = CryptoHelper.AesDecrypt(cipherBase64, InputkeyDecry.Text);
                OutputPlaintext.Text = plaintext;
                tsslLabel.Text = "ถอดรหัสสำเร็จ  |  Plaintext: " + plaintext.Length + " chars";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "ถอดรหัสไม่สำเร็จ อาจเกิดจาก:\n" +
                    "• Key ผิด\n" +
                    "• เลือกเทคนิค Steganography ผิด (ต้องใช้เทคนิคเดียวกับตอนฝัง)\n" +
                    "• เลือกคู่ตัวอักษร/คำผิด (ต้องเลือกเหมือนตอนฝัง)\n" +
                    "• Steganotext เสียหายหรือถูกแก้ไข\n\n" +
                    "รายละเอียด: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateDecryptionInput()
        {
            if (string.IsNullOrEmpty(InputChipertext.Text))
            {
                MessageBox.Show("กรุณาวาง Steganotext ในช่องด้านบน",
                    "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (string.IsNullOrEmpty(InputkeyDecry.Text))
            {
                MessageBox.Show("กรุณาใส่ Key สำหรับถอดรหัสก่อนเลือกเทคนิค",
                    "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        // Paste key ใน Decryption tab
        private void PasteButton1_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                InputkeyDecry.Text = Clipboard.GetText();
                tsslLabel.Text = "วาง Key แล้ว";
            }
        }

        // BtRs4 = Reset Steganotext input (Decryption tab)
        private void ResetBotton4_Click(object sender, EventArgs e)
        {
            InputChipertext.Clear();
            tsslLabel.Text = "ล้าง Steganotext แล้ว";
        }

        // BtRs5 = Reset Key (Decryption tab)
        private void BtRs5_Click(object sender, EventArgs e)
        {
            InputkeyDecry.Clear();
        }

        // BtRs6 = Reset Plaintext output (Decryption tab)
        private void BtRs6_Click(object sender, EventArgs e)
        {
            OutputPlaintext.Clear();
            tsslLabel.Text = "ล้าง Plaintext แล้ว";
        }

        // CopyButton3 = Copy Plaintext output
        private void CopyButton3_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(OutputPlaintext.Text))
            {
                Clipboard.SetText(OutputPlaintext.Text);
                tsslLabel.Text = "คัดลอก Plaintext แล้ว";
            }
            else
                MessageBox.Show("ไม่มี Plaintext ให้ copy — ถอดรหัสก่อน", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        // บันทึก Plaintext จาก Decryption tab
        private void BtSavePlain_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(OutputPlaintext.Text))
            {
                MessageBox.Show("ไม่มี Plaintext ให้บันทึก — ถอดรหัสก่อน", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using (var dlg = new SaveFileDialog
            {
                Title      = "บันทึก Plaintext",
                Filter     = "Text file (*.txt)|*.txt|All files (*.*)|*.*",
                DefaultExt = "txt",
                FileName   = "plaintext.txt"
            })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(dlg.FileName, OutputPlaintext.Text, Encoding.UTF8);
                    MessageBox.Show("บันทึก Plaintext เรียบร้อย", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    tsslLabel.Text = "บันทึกไฟล์: " + Path.GetFileName(dlg.FileName);
                }
            }
        }


        // ============================================================
        //  Menu Handlers
        // ============================================================

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) => Close();

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
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
                "   • เลือกเทคนิค: Homoglyph / Misspelling / Space / NBSP / Synonym\n" +
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

        private void steganographyInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "เทคนิค Steganography ที่รองรับ\n\n" +
                "1. Homoglyph\n" +
                "   แทนตัวอักษรไทยที่หน้าตาคล้ายกัน\n" +
                "   ฎ↔ฏ  ข↔ฃ  ช↔ซ\n" +
                "   ข้อจำกัด: Covertext ต้องมีตัวอักษรที่เลือกเพียงพอ\n\n" +
                "2. Misspelling\n" +
                "   แทนคำด้วยการสะกดผิดที่กำหนดไว้ล่วงหน้า\n" +
                "   คำถูก = bit 0 / คำผิด = bit 1\n" +
                "   มีคู่คำให้เลือก 64 คู่\n\n" +
                "3. Zero-Width Space (ZWSP)\n" +
                "   แทรก U+200B ระหว่างตัวอักษร\n" +
                "   มี ZWSP = bit 1 / ไม่มี = bit 0\n" +
                "   ความจุ = จำนวนตัวอักษรใน Covertext - 1\n\n" +
                "4. Non-Breaking Space (NBSP)\n" +
                "   แทนที่ space ปกติ (U+0020) ด้วย NBSP (U+00A0)\n" +
                "   NBSP = bit 1 / space ปกติ = bit 0\n" +
                "   ความจุ = จำนวน space ใน Covertext\n\n" +
                "5. Synonym\n" +
                "   สลับคำพ้องความหมายในกลุ่มที่เลือก\n" +
                "   คำแรกในกลุ่ม = bit 0 / คำที่สองในกลุ่ม = bit 1\n" +
                "   มีกลุ่มคำพ้องให้เลือก 64 กลุ่ม\n\n" +
                "Payload format: [32-bit length][16-bit CRC][data bits] (UTF-8, MSB first)",
                "ข้อมูล Steganography",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void fileFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "รูปแบบไฟล์ที่รองรับ\n\n" +
                "• .txt  — Text file (UTF-8) แนะนำ\n" +
                "• .pdf / .doc / .docx — อ่านได้เฉพาะ plain text เท่านั้น\n\n" +
                "Steganotext ที่บันทึกจะเป็น .txt (UTF-8)\n" +
                "ห้ามเปิดไฟล์ด้วย editor ที่ strip invisible characters\n" +
                "เพราะจะลบ ZWSP (U+200B) หรือ normalize NBSP (U+00A0) เป็น space ปกติ",
                "รูปแบบไฟล์",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void securityNoticeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "คำเตือนด้านความปลอดภัย\n\n" +
                "โปรแกรมนี้จัดทำขึ้นเพื่อการศึกษาเท่านั้น\n" +
                "ไม่ควรใช้กับข้อมูลลับหรือข้อมูลสำคัญจริง\n\n" +
                "คุณสมบัติ:\n" +
                "• AES-256-CBC + Salt สุ่มใหม่ทุกครั้ง\n" +
                "• HMAC-SHA256 ป้องกันการดัดแปลง\n" +
                "• CRC-16 ตรวจสอบ payload integrity\n\n" +
                "ข้อจำกัด:\n" +
                "• Homoglyph/Misspelling ตรวจจับได้ด้วยสายตา\n" +
                "• ZWSP/NBSP ตรวจจับได้ด้วย text analyzer\n" +
                "• PBKDF2 ควรใช้ 100,000+ รอบสำหรับงานจริง\n\n" +
                "ผู้พัฒนาไม่รับผิดชอบต่อความเสียหายใดๆ",
                "Security Warning",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }

    }
}
