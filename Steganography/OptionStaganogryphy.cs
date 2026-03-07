using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Steganography
{
    /// <summary>
    /// หน้าต่างสำหรับเลือกคู่ตัวอักษร Homoglyph
    /// ใช้ทั้งฝั่ง Embed (Steganography tab) และ Extract (Decryption tab)
    /// </summary>
    public partial class OptionStaganography : Form
    {
        // ============================================================
        //  Properties: รับ/ส่งข้อมูลกับ SteganographyForm
        // ============================================================

        /// <summary>โหมดการทำงาน: true = Embed, false = Extract</summary>
        public bool IsEmbedMode { get; set; } = true;

        /// <summary>Ciphertext (Base64) ที่จะฝัง (ใช้ตอน Embed)</summary>
        public string CipherText { get; set; } = "";

        /// <summary>Covertext ที่จะใช้ฝัง (ใช้ตอน Embed)</summary>
        public string CoverText { get; set; } = "";

        /// <summary>Steganotext ที่จะอ่าน (ใช้ตอน Extract)</summary>
        public string SteganotextInput { get; set; } = "";

        /// <summary>ผลลัพธ์ที่ได้หลัง OK (Steganotext หรือ Ciphertext)</summary>
        public string ResultText { get; private set; } = "";

        // ============================================================
        //  Constructor
        // ============================================================

        public OptionStaganography()
        {
            InitializeComponent();
        }

        private void OptionStaganography_Load(object sender, EventArgs e)
        {
            // ปรับ UI ตามโหมด
            Text = IsEmbedMode ? "Homoglyph — เลือกคู่ตัวอักษรสำหรับฝัง" : "Homoglyph — เลือกคู่ตัวอักษรสำหรับถอด";
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            ExecuteHomoglyph();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Close();
        }

        // ============================================================
        //  OK Button (ถูก inject ผ่าน Designer หรือ สร้างที่นี่)
        // ============================================================

        /// <summary>
        /// เรียกจาก SteganographyForm หลัง ShowDialog() หรือผ่านปุ่ม OK ใน form นี้
        /// </summary>
        public void ExecuteHomoglyph()
        {
            var activePairs = GetActivePairIndices();
            if (activePairs.Count == 0)
            {
                MessageBox.Show("กรุณาเลือกคู่ตัวอักษร Homoglyph อย่างน้อย 1 คู่",
                    "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (IsEmbedMode)
                    ResultText = SteganographyEngine.HomoglyphEmbed(CoverText, CipherText, activePairs);
                else
                    ResultText = SteganographyEngine.HomoglyphExtract(SteganotextInput, activePairs);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "เกิดข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============================================================
        //  Helper
        // ============================================================

        /// <summary>
        /// คืนรายการ index ของคู่ที่ผู้ใช้ติ๊กเลือก
        /// 3 คู่: ฎ↔ฏ (0), ข↔ฃ (1), ช↔ซ (2)
        /// </summary>
        public List<int> GetActivePairIndices()
        {
            var list = new List<int>();
            if (ChkbxDochada.Checked)  list.Add(0); // ฎ ฏ
            if (ChkbxKhoKhai.Checked)  list.Add(1); // ข ฃ
            if (ChkbxChoChang.Checked) list.Add(2); // ช ซ
            return list;
        }
    }
}
