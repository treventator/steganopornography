// ฟอร์มเลือกกลุ่มคำพ้อง (Synonym) — ทำงานเหมือน Misspelling แต่ใช้คำพ้อง

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Steganography
{
    public partial class Synonym : Form
    {
        public bool IsEmbedMode { get; set; } = true;
        public string CipherText { get; set; } = "";
        public string CoverText { get; set; } = "";
        public string SteganotextInput { get; set; } = "";
        public string ResultText { get; private set; } = "";

        public Synonym()
        {
            InitializeComponent();

            Load += (s, ev) =>
                Text = IsEmbedMode
                    ? "Synonym — เลือกกลุ่มคำสำหรับฝัง"
                    : "Synonym — เลือกกลุ่มคำสำหรับถอด";
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

        // ฝัง/ถอด synonym — คำ index 0 = bit 0, คำ index 1 = bit 1
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
                    // ต้องเลือกกลุ่มเดิมกับตอน embed ไม่งั้นถอดผิด
                    ResultText = SteganographyEngine.SynonymExtract(SteganotextInput, activeGroups);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "เกิดข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

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
