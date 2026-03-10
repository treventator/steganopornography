// ฟอร์มสำหรับเทคนิค Homoglyph
// ให้ user เลือกคู่ตัวอักษรที่หน้าตาคล้ายกัน เช่น ฎ↔ฏ, ข↔ฃ, ช↔ซ แล้วเอาไปฝัง/ถอดข้อมูล

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Steganography
{
    public partial class OptionStaganography : Form
    {
        // === Properties สำหรับคุยกับ main form ===

        // true = embed mode, false = extract mode
        public bool IsEmbedMode { get; set; } = true;

        // ciphertext ที่จะฝัง (Base64)
        public string CipherText { get; set; } = "";

        // covertext ภาษาไทยที่จะใช้ซ่อนข้อมูล
        public string CoverText { get; set; } = "";

        // steganotext สำหรับถอด
        public string SteganotextInput { get; set; } = "";

        // ผลลัพธ์หลังกด OK — main form จะมาอ่านตัวนี้
        public string ResultText { get; private set; } = "";

        public OptionStaganography()
        {
            InitializeComponent();
        }

        private void OptionStaganography_Load(object sender, EventArgs e)
        {
            // เปลี่ยน title bar ตามโหมด
            Text = IsEmbedMode
                ? "Homoglyph — เลือกคู่ตัวอักษรสำหรับฝัง"
                : "Homoglyph — เลือกคู่ตัวอักษรสำหรับถอด";
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

        /// <summary>
        /// ฝังหรือถอดข้อมูลด้วย Homoglyph
        /// อ่าน checkbox → เช็คว่าเลือกคู่ไหน → เรียก Engine
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
                    // ฝัง: แทนตัวอักษร original ด้วย glyph ตาม bit
                    ResultText = SteganographyEngine.HomoglyphEmbed(CoverText, CipherText, activePairs);
                else
                    // ถอด: อ่านว่าตัวไหนเป็น original(0) ตัวไหนเป็น glyph(1)
                    ResultText = SteganographyEngine.HomoglyphExtract(SteganotextInput, activePairs);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "เกิดข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// ดึง index คู่ที่ user ติ๊กเลือก
        /// index ต้องตรงกับ SteganographyEngine.HomoglyphPairs
        ///   0 = ฎ↔ฏ, 1 = ข↔ฃ, 2 = ช↔ซ
        /// </summary>
        public List<int> GetActivePairIndices()
        {
            var list = new List<int>();
            if (ChkbxDochada.Checked)  list.Add(0);  // ฎ ↔ ฏ
            if (ChkbxKhoKhai.Checked)  list.Add(1);  // ข ↔ ฃ
            if (ChkbxChoChang.Checked) list.Add(2);  // ช ↔ ซ
            return list;
        }
    }
}
