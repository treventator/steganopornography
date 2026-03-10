// ZWSP form

using System;
using System.Windows.Forms;

namespace Steganography
{
    public partial class ChkbxZWSP : Form
    {
        public bool IsEmbedMode { get; set; } = true;
        public string CipherText { get; set; } = "";
        public string CoverText { get; set; } = "";
        public string SteganotextInput { get; set; } = "";
        public string ResultText { get; private set; } = "";

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
                  // capacity = จำนวนช่องระหว่างตัวอักษร
                  $"Covertext ที่ป้อน: {CoverText.Length} ตัวอักษร " +
                  $"(รองรับได้สูงสุด {Math.Max(0, CoverText.Length - 1)} bits)"
                : "อ่าน Zero-Width Space จาก steganotext เพื่อถอดรหัส\n" +
                  "ไม่ต้องตั้งค่าใดๆ กดตกลงได้เลย";
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            try
            {
                // embed or extract
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
