using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Steganography
{
    /// <summary>
    /// หน้าต่างเลือกกลุ่มคำพ้องความหมาย สำหรับเทคนิค Synonym
    /// ใช้ทั้งฝั่ง Embed และ Extract
    /// </summary>
    public partial class Synonym : Form
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

        public Synonym()
        {
            InitializeComponent();
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            ExecuteSynonym();
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

        public void ExecuteSynonym()
        {
            var activeGroups = GetActiveGroupIndices();
            if (activeGroups.Count == 0)
            {
                MessageBox.Show("กรุณาเลือกกลุ่มคำพ้องอย่างน้อย 1 กลุ่ม",
                    "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (IsEmbedMode)
                    ResultText = SteganographyEngine.SynonymEmbed(CoverText, CipherText, activeGroups);
                else
                    ResultText = SteganographyEngine.SynonymExtract(SteganotextInput, activeGroups);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "เกิดข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>คืนรายการ index ของกลุ่มคำที่ผู้ใช้ติ๊กเลือก</summary>
        public List<int> GetActiveGroupIndices()
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
