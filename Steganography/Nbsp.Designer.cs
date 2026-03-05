namespace Steganography
{
    partial class NbspForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.LblTitle = new System.Windows.Forms.Label();
            this.LblDesc = new System.Windows.Forms.Label();
            this.LblInfo = new System.Windows.Forms.Label();
            this.BtnOK = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // LblTitle
            //
            this.LblTitle.AutoSize = true;
            this.LblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Bold);
            this.LblTitle.Location = new System.Drawing.Point(30, 25);
            this.LblTitle.Name = "LblTitle";
            this.LblTitle.Size = new System.Drawing.Size(380, 32);
            this.LblTitle.TabIndex = 0;
            this.LblTitle.Text = "Non-Breaking Space (NBSP)";
            //
            // LblDesc
            //
            this.LblDesc.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.LblDesc.Location = new System.Drawing.Point(30, 80);
            this.LblDesc.Name = "LblDesc";
            this.LblDesc.Size = new System.Drawing.Size(740, 120);
            this.LblDesc.TabIndex = 1;
            this.LblDesc.Text = "กำลังโหลด...";
            //
            // LblInfo
            //
            this.LblInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.LblInfo.ForeColor = System.Drawing.Color.DarkBlue;
            this.LblInfo.Location = new System.Drawing.Point(30, 220);
            this.LblInfo.Name = "LblInfo";
            this.LblInfo.Size = new System.Drawing.Size(740, 120);
            this.LblInfo.TabIndex = 2;
            this.LblInfo.Text = "หลักการทำงาน:\r\n" +
                "• ฝัง: วนผ่านทุก space (U+0020) ใน covertext\r\n" +
                "  - bit=1 → แทนที่ด้วย NBSP (U+00A0)\r\n" +
                "  - bit=0 → คง space ปกติไว้\r\n" +
                "• ถอด: ตรวจสอบแต่ละตำแหน่ง space/NBSP\r\n" +
                "  - NBSP → bit=1, space ปกติ → bit=0";
            //
            // BtnOK
            //
            this.BtnOK.BackColor = System.Drawing.Color.FromArgb(192, 255, 192);
            this.BtnOK.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold);
            this.BtnOK.ForeColor = System.Drawing.Color.DarkGreen;
            this.BtnOK.Location = new System.Drawing.Point(560, 380);
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.Size = new System.Drawing.Size(100, 40);
            this.BtnOK.TabIndex = 3;
            this.BtnOK.Text = "ตกลง";
            this.BtnOK.UseVisualStyleBackColor = false;
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            //
            // BtnCancel
            //
            this.BtnCancel.BackColor = System.Drawing.Color.FromArgb(255, 200, 200);
            this.BtnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.BtnCancel.ForeColor = System.Drawing.Color.DarkRed;
            this.BtnCancel.Location = new System.Drawing.Point(670, 380);
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Size = new System.Drawing.Size(100, 40);
            this.BtnCancel.TabIndex = 4;
            this.BtnCancel.Text = "ยกเลิก";
            this.BtnCancel.UseVisualStyleBackColor = false;
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            //
            // NbspForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnOK);
            this.Controls.Add(this.LblInfo);
            this.Controls.Add(this.LblDesc);
            this.Controls.Add(this.LblTitle);
            this.Name = "NbspForm";
            this.Text = "Non-Breaking Space";
            this.Load += new System.EventHandler(this.NbspForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label LblTitle;
        private System.Windows.Forms.Label LblDesc;
        private System.Windows.Forms.Label LblInfo;
        private System.Windows.Forms.Button BtnOK;
        private System.Windows.Forms.Button BtnCancel;
    }
}
