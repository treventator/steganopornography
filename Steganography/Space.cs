using System;
using System.Windows.Forms;

namespace Steganography
{
    /// <summary>
    /// หน้าต่างสำหรับเทคนิค Zero-Width Space (ZWSP)
    /// แทรก U+200B ระหว่างตัวอักษรใน covertext เพื่อแทน bit 0/1
    /// </summary>
    public partial class ChkbxZWSP : Form
    {
        // ============================================================
        //  Properties
        // ============================================================

        /// <summary>โหมดการทำงาน: true = Embed, false = Extract</summary>
        public bool IsEmbedMode { get; set; } = true;

        /// <summary>Ciphertext (Base64) ที่จะฝัง</summary>
        public string CipherText { get; set; } = "";

        /// <summary>Covertext สำหรับฝัง</summary>
        public string CoverText { get; set; } = "";

        /// <summary>Steganotext สำหรับถอด</summary>
        public string SteganotextInput { get; set; } = "";

        /// <summary>ผลลัพธ์หลัง OK</summary>
        public string ResultText { get; private set; } = "";

        // ============================================================
        //  Constructor
        // ============================================================

        public ChkbxZWSP()
        {
            InitializeComponent();
        }

        private void ChkbxZWSP_Load(object sender, EventArgs e)
        {
            Text = IsEmbedMode
                ? "Zero-Width Space — ฝังข้อมูล"
                : "Zero-Width Space — ถอดข้อมูล";

            LblDesc.Text = IsEmbedMode
                ? "เทคนิคนี้แทรกอักขระ Zero-Width Space (U+200B) ระหว่างตัวอักษรใน covertext\n" +
                  "bit 1 = แทรก ZWSP, bit 0 = ไม่แทรก\n\n" +
                  $"Covertext ที่ป้อน: {CoverText.Length} ตัวอักษร " +
                  $"(รองรับได้สูงสุด {Math.Max(0, CoverText.Length - 1)} bits)"
                : "อ่าน Zero-Width Space จาก steganotext เพื่อถอดรหัส\n" +
                  "ไม่ต้องตั้งค่าใดๆ กดตกลงได้เลย";
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (IsEmbedMode)
                    ResultText = SteganographyEngine.ZWSPEmbed(CoverText, CipherText);
                else
                    ResultText = SteganographyEngine.ZWSPExtract(SteganotextInput);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "เกิดข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
