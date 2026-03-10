// ฟอร์มเลือกคู่คำสะกดผิด-ถูก สำหรับ Misspelling technique (65 คู่)

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Steganography
{
    public partial class Misspelling : Form
    {
        public bool IsEmbedMode { get; set; } = true;
        public string CipherText { get; set; } = "";
        public string CoverText { get; set; } = "";
        public string SteganotextInput { get; set; } = "";
        public string ResultText { get; private set; } = "";

        public Misspelling()
        {
            InitializeComponent();

            Load += (s, ev) =>
                Text = IsEmbedMode
                    ? "Misspelling — เลือกคู่คำสำหรับฝัง"
                    : "Misspelling — เลือกคู่คำสำหรับถอด";
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
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

        // ฝัง/ถอดด้วย misspelling — เช็คว่าเลือกคู่คำอะไรบ้างแล้วส่งให้ Engine
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
