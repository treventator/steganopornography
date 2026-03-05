using System;
using System.Windows.Forms;

namespace Steganography
{
    /// <summary>
    /// หน้าต่างสำหรับเทคนิค Non-Breaking Space (NBSP)
    /// แทนที่ space ปกติ (U+0020) ด้วย NBSP (U+00A0) เพื่อแทน bit 0/1
    /// </summary>
    public partial class NbspForm : Form
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

        public NbspForm()
        {
            InitializeComponent();
        }

        private void NbspForm_Load(object sender, EventArgs e)
        {
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
