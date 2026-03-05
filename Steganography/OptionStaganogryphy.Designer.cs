namespace Steganography
{
    partial class OptionStaganography
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
            this.ChkbxDochada = new System.Windows.Forms.CheckBox();
            this.ChkbxKhoKhai = new System.Windows.Forms.CheckBox();
            this.ChkbxChoChang = new System.Windows.Forms.CheckBox();
            this.LblTitle = new System.Windows.Forms.Label();
            this.LblDesc = new System.Windows.Forms.Label();
            this.BtnOK = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // LblTitle
            //
            this.LblTitle.AutoSize = true;
            this.LblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold);
            this.LblTitle.Location = new System.Drawing.Point(30, 18);
            this.LblTitle.Name = "LblTitle";
            this.LblTitle.Size = new System.Drawing.Size(280, 26);
            this.LblTitle.TabIndex = 10;
            this.LblTitle.Text = "เลือกคู่ตัวอักษร Homoglyph";
            //
            // LblDesc
            //
            this.LblDesc.AutoSize = true;
            this.LblDesc.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.LblDesc.ForeColor = System.Drawing.Color.Gray;
            this.LblDesc.Location = new System.Drawing.Point(30, 50);
            this.LblDesc.Name = "LblDesc";
            this.LblDesc.Size = new System.Drawing.Size(380, 22);
            this.LblDesc.TabIndex = 11;
            this.LblDesc.Text = "ตัวอักษรซ้ายคือตัวจริง (bit=0), ขวาคือตัวแทน (bit=1)";
            //
            // ChkbxDochada
            //
            this.ChkbxDochada.AutoSize = true;
            this.ChkbxDochada.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ChkbxDochada.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.ChkbxDochada.Location = new System.Drawing.Point(72, 90);
            this.ChkbxDochada.Name = "ChkbxDochada";
            this.ChkbxDochada.Size = new System.Drawing.Size(200, 30);
            this.ChkbxDochada.TabIndex = 0;
            this.ChkbxDochada.Text = "ฎ  →  ฏ  (ดอชะดา / ปะตัก)";
            this.ChkbxDochada.UseVisualStyleBackColor = true;
            //
            // ChkbxKhoKhai
            //
            this.ChkbxKhoKhai.AutoSize = true;
            this.ChkbxKhoKhai.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.ChkbxKhoKhai.Location = new System.Drawing.Point(72, 135);
            this.ChkbxKhoKhai.Name = "ChkbxKhoKhai";
            this.ChkbxKhoKhai.Size = new System.Drawing.Size(190, 30);
            this.ChkbxKhoKhai.TabIndex = 1;
            this.ChkbxKhoKhai.Text = "ข  →  ฃ  (ขอไข่ / ขอขวด)";
            this.ChkbxKhoKhai.UseVisualStyleBackColor = true;
            //
            // ChkbxChoChang
            //
            this.ChkbxChoChang.AutoSize = true;
            this.ChkbxChoChang.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.ChkbxChoChang.Location = new System.Drawing.Point(72, 180);
            this.ChkbxChoChang.Name = "ChkbxChoChang";
            this.ChkbxChoChang.Size = new System.Drawing.Size(200, 30);
            this.ChkbxChoChang.TabIndex = 2;
            this.ChkbxChoChang.Text = "ช  →  ซ  (ชอช้าง / โซ่โซ)";
            this.ChkbxChoChang.UseVisualStyleBackColor = true;
            //
            // BtnOK
            //
            this.BtnOK.BackColor = System.Drawing.Color.FromArgb(192, 255, 192);
            this.BtnOK.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.BtnOK.ForeColor = System.Drawing.Color.DarkGreen;
            this.BtnOK.Location = new System.Drawing.Point(530, 370);
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.Size = new System.Drawing.Size(110, 40);
            this.BtnOK.TabIndex = 3;
            this.BtnOK.Text = "ตกลง";
            this.BtnOK.UseVisualStyleBackColor = false;
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            //
            // BtnCancel
            //
            this.BtnCancel.BackColor = System.Drawing.Color.FromArgb(255, 200, 200);
            this.BtnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.BtnCancel.ForeColor = System.Drawing.Color.DarkRed;
            this.BtnCancel.Location = new System.Drawing.Point(650, 370);
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Size = new System.Drawing.Size(110, 40);
            this.BtnCancel.TabIndex = 4;
            this.BtnCancel.Text = "ยกเลิก";
            this.BtnCancel.UseVisualStyleBackColor = false;
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            //
            // OptionStaganography
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnOK);
            this.Controls.Add(this.ChkbxChoChang);
            this.Controls.Add(this.ChkbxKhoKhai);
            this.Controls.Add(this.ChkbxDochada);
            this.Controls.Add(this.LblDesc);
            this.Controls.Add(this.LblTitle);
            this.Name = "OptionStaganography";
            this.Text = "Homoglyph Options";
            this.Load += new System.EventHandler(this.OptionStaganography_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.CheckBox ChkbxDochada;
        private System.Windows.Forms.CheckBox ChkbxKhoKhai;
        private System.Windows.Forms.CheckBox ChkbxChoChang;
        private System.Windows.Forms.Label LblTitle;
        private System.Windows.Forms.Label LblDesc;
        private System.Windows.Forms.Button BtnOK;
        private System.Windows.Forms.Button BtnCancel;
    }
}
