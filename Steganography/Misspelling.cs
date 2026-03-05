using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Steganography
{
    /// <summary>
    /// หน้าต่างเลือกคู่คำสะกดผิด-ถูก สำหรับเทคนิค Misspelling
    /// ใช้ทั้งฝั่ง Embed และ Extract
    /// </summary>
    public partial class Misspelling : Form
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

        public Misspelling()
        {
            InitializeComponent();
            Load += (s, ev) =>
                Text = IsEmbedMode ? "Misspelling — เลือกคู่คำสำหรับฝัง" : "Misspelling — เลือกคู่คำสำหรับถอด";
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // ใช้สำหรับ preview หรือ validation ในอนาคต
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            ExecuteMisspelling();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void BtnSelectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
                checkedListBox1.SetItemChecked(i, true);
        }

        private void BtnClearAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
                checkedListBox1.SetItemChecked(i, false);
        }

        // ============================================================
        //  Core
        // ============================================================

        public void ExecuteMisspelling()
        {
            var activePairs = GetActivePairIndices();
            if (activePairs.Count == 0)
            {
                MessageBox.Show("กรุณาเลือกคู่คำอย่างน้อย 1 คู่",
                    "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (IsEmbedMode)
                    ResultText = SteganographyEngine.MisspellingEmbed(CoverText, CipherText, activePairs);
                else
                    ResultText = SteganographyEngine.MisspellingExtract(SteganotextInput, activePairs);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "เกิดข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>คืนรายการ index ของคู่คำที่ผู้ใช้ติ๊กเลือก</summary>
        public List<int> GetActivePairIndices()
        {
            var list = new List<int>();
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                if (checkedListBox1.GetItemChecked(i))
                    list.Add(i);
            }
            return list;
        }
    }
}
