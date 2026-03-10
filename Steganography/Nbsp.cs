// NBSP form — แทนที่ space ปกติ (U+0020) ด้วย Non-Breaking Space (U+00A0)
// คล้ายๆ ZWSP แต่ใช้ space แทน

using System;
using System.Windows.Forms;

namespace Steganography
{
    public partial class NbspForm : Form
    {
        public bool IsEmbedMode { get; set; } = true;
        public string CipherText { get; set; } = "";
        public string CoverText { get; set; } = "";
        public string SteganotextInput { get; set; } = "";
        public string ResultText { get; private set; } = "";

        public NbspForm()
        {
            InitializeComponent();
        }

        private void NbspForm_Load(object sender, EventArgs e)
        {
            // นับ space ใน covertext เพื่อคำนวณ capacity
            int spaceCount = 0;
            foreach (char c in CoverText)
                if (c == ' ') spaceCount++;

            Text = IsEmbedMode
                ? "Non-Breaking Space — ฝังข้อมูล"
                : "Non-Breaking Space — ถอดข้อมูล";

            LblDesc.Text = IsEmbedMode
                ? "เทคนิคนี้แทนที่ช่องว่างปกติ (U+0020) ด้วย Non-Breaking Space (U+00A0)\n" +
                  "bit 1 = NBSP, bit 0 = space ปกติ\n\n" +
                  $"Covertext ที่ป้อน: {CoverText.Length} ตัวอักษร ({spaceCount} spaces)" +
                  $" — รองรับได้สูงสุด {spaceCount} bits"
                : "อ่าน Non-Breaking Space จาก steganotext เพื่อถอดรหัส\n" +
                  "ไม่ต้องตั้งค่าใดๆ กดตกลงได้เลย";
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (IsEmbedMode)
                    ResultText = SteganographyEngine.NBSPEmbed(CoverText, CipherText);
                else
                    ResultText = SteganographyEngine.NBSPExtract(SteganotextInput);

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
