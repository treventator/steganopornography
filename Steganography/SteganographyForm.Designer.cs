namespace Steganography
{
    partial class SteganographyForm
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
            if (disposing)
            {
                tt?.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        #endregion

        {
            this.TabControl1 = new System.Windows.Forms.TabControl();
            this.TabEncrytion = new System.Windows.Forms.TabPage();
            this.LbFn = new System.Windows.Forms.Label();
            this.namefile = new System.Windows.Forms.TextBox();
            this.openbrowser = new System.Windows.Forms.Button();
            this.CopyButton1 = new System.Windows.Forms.Button();
            this.InputCovertext = new System.Windows.Forms.TextBox();
            this.InputPayload = new System.Windows.Forms.TextBox();
            this.BtRs1 = new System.Windows.Forms.Button();
            this.BtRs2 = new System.Windows.Forms.Button();
            this.EncrytionButton1 = new System.Windows.Forms.Button();
            this.CopyButton2 = new System.Windows.Forms.Button();
            this.ResetBotton3 = new System.Windows.Forms.Button();
            this.CoverText = new System.Windows.Forms.Label();
            this.InputKey = new System.Windows.Forms.TextBox();
            this.Key = new System.Windows.Forms.Label();
            this.ciphertext = new System.Windows.Forms.Label();
            this.Plaintext = new System.Windows.Forms.Label();
            this.InputPlaintext = new System.Windows.Forms.TextBox();
            this.Encrytion = new System.Windows.Forms.Label();
            this.TabSteganography = new System.Windows.Forms.TabPage();
            this.BtS = new System.Windows.Forms.Button();
            this.CopyStegano = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.BtSnn = new System.Windows.Forms.Button();
            this.BtSp = new System.Windows.Forms.Button();
            this.BtMs = new System.Windows.Forms.Button();
            this.BtHm = new System.Windows.Forms.Button();
            this.Steganotext = new System.Windows.Forms.Label();
            this.TbStegano = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.BtRs3 = new System.Windows.Forms.Button();
            this.Ciphertextname = new System.Windows.Forms.Label();
            this.Steganogranophy = new System.Windows.Forms.Label();
            this.TabDecrytion = new System.Windows.Forms.TabPage();
            this.BtSnnD = new System.Windows.Forms.Button();
            this.BtSpD = new System.Windows.Forms.Button();
            this.BtMsD = new System.Windows.Forms.Button();
            this.BtHmD = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.OutputPlaintext = new System.Windows.Forms.TextBox();
            this.PasteButton1 = new System.Windows.Forms.Button();
            this.BtRs4 = new System.Windows.Forms.Button();
            this.BtRs5 = new System.Windows.Forms.Button();
            this.DecrytionBotton10 = new System.Windows.Forms.Button();
            this.CopyButton3 = new System.Windows.Forms.Button();
            this.BtRs6 = new System.Windows.Forms.Button();
            this.BtSavePlain = new System.Windows.Forms.Button();
            this.PlaintextDecry = new System.Windows.Forms.Label();
            this.InputkeyDecry = new System.Windows.Forms.TextBox();
            this.KeyDecry = new System.Windows.Forms.Label();
            this.Steganotext2 = new System.Windows.Forms.Label();
            this.InputChipertext = new System.Windows.Forms.TextBox();
            this.Decrytion = new System.Windows.Forms.Label();
            this.ssMain = new System.Windows.Forms.StatusStrip();
            this.tsslLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.steganographyInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileFormatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.securityNoticeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.TabControl1.SuspendLayout();
            this.TabEncrytion.SuspendLayout();
            this.TabSteganography.SuspendLayout();
            this.TabDecrytion.SuspendLayout();
            this.ssMain.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // TabControl1
            // 
            this.TabControl1.Controls.Add(this.TabEncrytion);
            this.TabControl1.Controls.Add(this.TabSteganography);
            this.TabControl1.Controls.Add(this.TabDecrytion);
            this.TabControl1.Location = new System.Drawing.Point(8, 51);
            this.TabControl1.Name = "TabControl1";
            this.TabControl1.SelectedIndex = 0;
            this.TabControl1.Size = new System.Drawing.Size(806, 696);
            this.TabControl1.TabIndex = 6;
            this.TabControl1.SelectedIndexChanged += new System.EventHandler(this.TabControl1_SelectedIndexChanged);
            // 
            // TabEncrytion
            // 
            this.TabEncrytion.Controls.Add(this.LbFn);
            this.TabEncrytion.Controls.Add(this.namefile);
            this.TabEncrytion.Controls.Add(this.openbrowser);
            this.TabEncrytion.Controls.Add(this.CopyButton1);
            this.TabEncrytion.Controls.Add(this.InputCovertext);
            this.TabEncrytion.Controls.Add(this.InputPayload);
            this.TabEncrytion.Controls.Add(this.BtRs1);
            this.TabEncrytion.Controls.Add(this.BtRs2);
            this.TabEncrytion.Controls.Add(this.EncrytionButton1);
            this.TabEncrytion.Controls.Add(this.CopyButton2);
            this.TabEncrytion.Controls.Add(this.ResetBotton3);
            this.TabEncrytion.Controls.Add(this.CoverText);
            this.TabEncrytion.Controls.Add(this.InputKey);
            this.TabEncrytion.Controls.Add(this.Key);
            this.TabEncrytion.Controls.Add(this.ciphertext);
            this.TabEncrytion.Controls.Add(this.Plaintext);
            this.TabEncrytion.Controls.Add(this.InputPlaintext);
            this.TabEncrytion.Controls.Add(this.Encrytion);
            this.TabEncrytion.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TabEncrytion.Location = new System.Drawing.Point(4, 29);
            this.TabEncrytion.Name = "TabEncrytion";
            this.TabEncrytion.Padding = new System.Windows.Forms.Padding(3);
            this.TabEncrytion.Size = new System.Drawing.Size(798, 663);
            this.TabEncrytion.TabIndex = 0;
            this.TabEncrytion.Text = "Encryption";
            this.TabEncrytion.UseVisualStyleBackColor = true;
            // 
            // LbFn
            // 
            this.LbFn.AutoSize = true;
            this.LbFn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LbFn.Location = new System.Drawing.Point(273, 197);
            this.LbFn.Name = "LbFn";
            this.LbFn.Size = new System.Drawing.Size(88, 22);
            this.LbFn.TabIndex = 27;
            this.LbFn.Text = "File name";
            // 
            // namefile
            // 
            this.namefile.BackColor = System.Drawing.SystemColors.Control;
            this.namefile.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.namefile.Enabled = false;
            this.namefile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.namefile.ForeColor = System.Drawing.Color.Black;
            this.namefile.Location = new System.Drawing.Point(380, 191);
            this.namefile.Name = "namefile";
            this.namefile.Size = new System.Drawing.Size(178, 28);
            this.namefile.TabIndex = 26;
            // 
            // openbrowser
            // 
            this.openbrowser.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.openbrowser.Location = new System.Drawing.Point(568, 188);
            this.openbrowser.Name = "openbrowser";
            this.openbrowser.Size = new System.Drawing.Size(85, 35);
            this.openbrowser.TabIndex = 25;
            this.openbrowser.Text = "Browser";
            this.openbrowser.UseVisualStyleBackColor = true;
            this.openbrowser.Click += new System.EventHandler(this.Browser_Click);
            // 
            // CopyButton1
            // 
            this.CopyButton1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.CopyButton1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CopyButton1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.CopyButton1.Location = new System.Drawing.Point(380, 248);
            this.CopyButton1.Name = "CopyButton1";
            this.CopyButton1.Size = new System.Drawing.Size(80, 35);
            this.CopyButton1.TabIndex = 23;
            this.CopyButton1.Text = "Copy";
            this.CopyButton1.UseVisualStyleBackColor = false;
            this.CopyButton1.Click += new System.EventHandler(this.CopyButton1_Click);
            // 
            // InputCovertext
            // 
            this.InputCovertext.BackColor = System.Drawing.SystemColors.Window;
            this.InputCovertext.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.InputCovertext.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InputCovertext.Location = new System.Drawing.Point(60, 524);
            this.InputCovertext.Multiline = true;
            this.InputCovertext.Name = "InputCovertext";
            this.InputCovertext.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.InputCovertext.Size = new System.Drawing.Size(677, 90);
            this.InputCovertext.TabIndex = 22;
            this.InputCovertext.TextChanged += new System.EventHandler(this.InputCovertext_TextChanged);
            // 
            // InputPayload
            // 
            this.InputPayload.BackColor = System.Drawing.SystemColors.Control;
            this.InputPayload.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.InputPayload.Enabled = false;
            this.InputPayload.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InputPayload.Location = new System.Drawing.Point(60, 367);
            this.InputPayload.Multiline = true;
            this.InputPayload.Name = "InputPayload";
            this.InputPayload.ReadOnly = true;
            this.InputPayload.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.InputPayload.Size = new System.Drawing.Size(677, 97);
            this.InputPayload.TabIndex = 21;
            // 
            // BtRs1
            // 
            this.BtRs1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.BtRs1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtRs1.ForeColor = System.Drawing.Color.SaddleBrown;
            this.BtRs1.Location = new System.Drawing.Point(658, 188);
            this.BtRs1.Name = "BtRs1";
            this.BtRs1.Size = new System.Drawing.Size(80, 35);
            this.BtRs1.TabIndex = 19;
            this.BtRs1.Text = "Reset";
            this.BtRs1.UseVisualStyleBackColor = false;
            this.BtRs1.Click += new System.EventHandler(this.ResetBotton1_Click);
            // 
            // BtRs2
            // 
            this.BtRs2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.BtRs2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtRs2.ForeColor = System.Drawing.Color.SaddleBrown;
            this.BtRs2.Location = new System.Drawing.Point(465, 248);
            this.BtRs2.Name = "BtRs2";
            this.BtRs2.Size = new System.Drawing.Size(80, 35);
            this.BtRs2.TabIndex = 18;
            this.BtRs2.Text = "Reset";
            this.BtRs2.UseVisualStyleBackColor = false;
            this.BtRs2.Click += new System.EventHandler(this.ResetBotton2_Click);
            // 
            // EncrytionButton1
            // 
            this.EncrytionButton1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.EncrytionButton1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EncrytionButton1.ForeColor = System.Drawing.Color.Green;
            this.EncrytionButton1.Location = new System.Drawing.Point(550, 248);
            this.EncrytionButton1.Name = "EncrytionButton1";
            this.EncrytionButton1.Size = new System.Drawing.Size(103, 35);
            this.EncrytionButton1.TabIndex = 17;
            this.EncrytionButton1.Text = "Encryption";
            this.EncrytionButton1.UseVisualStyleBackColor = false;
            this.EncrytionButton1.Click += new System.EventHandler(this.EncrytionButton1_Click);
            // 
            // CopyButton2
            // 
            this.CopyButton2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.CopyButton2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.CopyButton2.Location = new System.Drawing.Point(573, 625);
            this.CopyButton2.Name = "CopyButton2";
            this.CopyButton2.Size = new System.Drawing.Size(80, 35);
            this.CopyButton2.TabIndex = 16;
            this.CopyButton2.Text = "Copy";
            this.CopyButton2.UseVisualStyleBackColor = false;
            this.CopyButton2.Click += new System.EventHandler(this.CopyButton2_Click);
            // 
            // ResetBotton3
            // 
            this.ResetBotton3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.ResetBotton3.ForeColor = System.Drawing.Color.SaddleBrown;
            this.ResetBotton3.Location = new System.Drawing.Point(658, 625);
            this.ResetBotton3.Name = "ResetBotton3";
            this.ResetBotton3.Size = new System.Drawing.Size(80, 35);
            this.ResetBotton3.TabIndex = 15;
            this.ResetBotton3.Text = "Reset";
            this.ResetBotton3.UseVisualStyleBackColor = false;
            this.ResetBotton3.Click += new System.EventHandler(this.ResetBotton3_Click);
            // 
            // CoverText
            // 
            this.CoverText.AutoSize = true;
            this.CoverText.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CoverText.Location = new System.Drawing.Point(56, 491);
            this.CoverText.Name = "CoverText";
            this.CoverText.Size = new System.Drawing.Size(87, 22);
            this.CoverText.TabIndex = 13;
            this.CoverText.Text = "Covertext";
            // 
            // InputKey
            // 
            this.InputKey.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.InputKey.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InputKey.Location = new System.Drawing.Point(105, 256);
            this.InputKey.Name = "InputKey";
            this.InputKey.Size = new System.Drawing.Size(256, 28);
            this.InputKey.TabIndex = 8;
            // 
            // Key
            // 
            this.Key.AutoSize = true;
            this.Key.BackColor = System.Drawing.Color.Transparent;
            this.Key.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Key.ForeColor = System.Drawing.Color.Black;
            this.Key.Location = new System.Drawing.Point(64, 258);
            this.Key.Name = "Key";
            this.Key.Size = new System.Drawing.Size(41, 22);
            this.Key.TabIndex = 7;
            this.Key.Text = "Key";
            // 
            // ciphertext
            // 
            this.ciphertext.AutoSize = true;
            this.ciphertext.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ciphertext.Location = new System.Drawing.Point(56, 325);
            this.ciphertext.Name = "ciphertext";
            this.ciphertext.Size = new System.Drawing.Size(92, 22);
            this.ciphertext.TabIndex = 4;
            this.ciphertext.Text = "Ciphertext";
            // 
            // Plaintext
            // 
            this.Plaintext.AutoSize = true;
            this.Plaintext.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Plaintext.Location = new System.Drawing.Point(56, 49);
            this.Plaintext.Name = "Plaintext";
            this.Plaintext.Size = new System.Drawing.Size(79, 22);
            this.Plaintext.TabIndex = 2;
            this.Plaintext.Text = "Plaintext";
            // 
            // InputPlaintext
            // 
            this.InputPlaintext.BackColor = System.Drawing.SystemColors.Window;
            this.InputPlaintext.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.InputPlaintext.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InputPlaintext.Location = new System.Drawing.Point(60, 83);
            this.InputPlaintext.Multiline = true;
            this.InputPlaintext.Name = "InputPlaintext";
            this.InputPlaintext.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.InputPlaintext.Size = new System.Drawing.Size(677, 94);
            this.InputPlaintext.TabIndex = 1;
            // 
            // Encrytion
            // 
            this.Encrytion.AutoSize = true;
            this.Encrytion.BackColor = System.Drawing.Color.Transparent;
            this.Encrytion.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Encrytion.ForeColor = System.Drawing.Color.Black;
            this.Encrytion.Location = new System.Drawing.Point(56, 18);
            this.Encrytion.Name = "Encrytion";
            this.Encrytion.Size = new System.Drawing.Size(172, 22);
            this.Encrytion.TabIndex = 0;
            this.Encrytion.Text = "Encryption AES-256";
            // 
            // TabSteganography
            // 
            this.TabSteganography.Controls.Add(this.CopyStegano);
            this.TabSteganography.Controls.Add(this.BtS);
            this.TabSteganography.Controls.Add(this.textBox1);
            this.TabSteganography.Controls.Add(this.BtSnn);
            this.TabSteganography.Controls.Add(this.BtSp);
            this.TabSteganography.Controls.Add(this.BtMs);
            this.TabSteganography.Controls.Add(this.BtHm);
            this.TabSteganography.Controls.Add(this.Steganotext);
            this.TabSteganography.Controls.Add(this.TbStegano);
            this.TabSteganography.Controls.Add(this.label2);
            this.TabSteganography.Controls.Add(this.BtRs3);
            this.TabSteganography.Controls.Add(this.Ciphertextname);
            this.TabSteganography.Controls.Add(this.Steganogranophy);
            this.TabSteganography.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TabSteganography.Location = new System.Drawing.Point(4, 29);
            this.TabSteganography.Name = "TabSteganography";
            this.TabSteganography.Padding = new System.Windows.Forms.Padding(3);
            this.TabSteganography.Size = new System.Drawing.Size(798, 663);
            this.TabSteganography.TabIndex = 1;
            this.TabSteganography.Text = "Steganography";
            this.TabSteganography.UseVisualStyleBackColor = true;
            // 
            // CopyStegano
            // 
            this.CopyStegano.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.CopyStegano.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.CopyStegano.Location = new System.Drawing.Point(563, 537);
            this.CopyStegano.Name = "CopyStegano";
            this.CopyStegano.Size = new System.Drawing.Size(75, 33);
            this.CopyStegano.TabIndex = 34;
            this.CopyStegano.Text = "Copy";
            this.CopyStegano.UseVisualStyleBackColor = false;
            this.CopyStegano.Click += new System.EventHandler(this.CopyStegano_Click);
            // 
            // BtS
            // 
            this.BtS.Location = new System.Drawing.Point(644, 537);
            this.BtS.Name = "BtS";
            this.BtS.Size = new System.Drawing.Size(75, 33);
            this.BtS.TabIndex = 33;
            this.BtS.Text = "Save";
            this.BtS.UseVisualStyleBackColor = true;
            this.BtS.Click += new System.EventHandler(this.BtS_Click);
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.Control;
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.Location = new System.Drawing.Point(48, 92);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(677, 96);
            this.textBox1.TabIndex = 32;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // BtSnn
            // 
            this.BtSnn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtSnn.Location = new System.Drawing.Point(445, 267);
            this.BtSnn.Name = "BtSnn";
            this.BtSnn.Size = new System.Drawing.Size(100, 37);
            this.BtSnn.TabIndex = 31;
            this.BtSnn.Text = "Synonym";
            this.BtSnn.UseVisualStyleBackColor = true;
            this.BtSnn.Click += new System.EventHandler(this.BtSnn_Click);
            // 
            // BtSp
            // 
            this.BtSp.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtSp.Location = new System.Drawing.Point(364, 267);
            this.BtSp.Name = "BtSp";
            this.BtSp.Size = new System.Drawing.Size(75, 37);
            this.BtSp.TabIndex = 30;
            this.BtSp.Text = "Space";
            this.BtSp.UseVisualStyleBackColor = true;
            this.BtSp.Click += new System.EventHandler(this.Btsp_Click);
            // 
            // BtMs
            // 
            this.BtMs.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtMs.Location = new System.Drawing.Point(250, 267);
            this.BtMs.Name = "BtMs";
            this.BtMs.Size = new System.Drawing.Size(108, 37);
            this.BtMs.TabIndex = 29;
            this.BtMs.Text = "Misspelling";
            this.BtMs.UseVisualStyleBackColor = true;
            this.BtMs.Click += new System.EventHandler(this.BtMs_Click);
            // 
            // BtHm
            // 
            this.BtHm.Location = new System.Drawing.Point(134, 267);
            this.BtHm.Name = "BtHm";
            this.BtHm.Size = new System.Drawing.Size(110, 37);
            this.BtHm.TabIndex = 28;
            this.BtHm.Text = "Homoglyph";
            this.BtHm.UseVisualStyleBackColor = true;
            this.BtHm.Click += new System.EventHandler(this.btOptions_Click);
            // 
            // Steganotext
            // 
            this.Steganotext.AutoSize = true;
            this.Steganotext.Location = new System.Drawing.Point(43, 373);
            this.Steganotext.Name = "Steganotext";
            this.Steganotext.Size = new System.Drawing.Size(106, 22);
            this.Steganotext.TabIndex = 27;
            this.Steganotext.Text = "Steganotext";
            // 
            // TbStegano
            // 
            this.TbStegano.BackColor = System.Drawing.SystemColors.Control;
            this.TbStegano.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TbStegano.Location = new System.Drawing.Point(48, 407);
            this.TbStegano.Multiline = true;
            this.TbStegano.Name = "TbStegano";
            this.TbStegano.ReadOnly = true;
            this.TbStegano.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TbStegano.Size = new System.Drawing.Size(677, 113);
            this.TbStegano.TabIndex = 26;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Black;
            this.label2.Location = new System.Drawing.Point(43, 274);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(85, 22);
            this.label2.TabIndex = 24;
            this.label2.Text = "Algorithm";
            // 
            // BtRs3
            // 
            this.BtRs3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.BtRs3.ForeColor = System.Drawing.Color.SaddleBrown;
            this.BtRs3.Location = new System.Drawing.Point(644, 220);
            this.BtRs3.Name = "BtRs3";
            this.BtRs3.Size = new System.Drawing.Size(80, 35);
            this.BtRs3.TabIndex = 23;
            this.BtRs3.Text = "Reset";
            this.BtRs3.UseVisualStyleBackColor = false;
            this.BtRs3.Click += new System.EventHandler(this.BtRs3_Click);
            // 
            // Ciphertextname
            // 
            this.Ciphertextname.AutoSize = true;
            this.Ciphertextname.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Ciphertextname.Location = new System.Drawing.Point(43, 67);
            this.Ciphertextname.Name = "Ciphertextname";
            this.Ciphertextname.Size = new System.Drawing.Size(92, 22);
            this.Ciphertextname.TabIndex = 22;
            this.Ciphertextname.Text = "Ciphertext";
            // 
            // Steganogranophy
            // 
            this.Steganogranophy.AutoSize = true;
            this.Steganogranophy.BackColor = System.Drawing.Color.Transparent;
            this.Steganogranophy.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Steganogranophy.ForeColor = System.Drawing.Color.Black;
            this.Steganogranophy.Location = new System.Drawing.Point(43, 29);
            this.Steganogranophy.Name = "Steganogranophy";
            this.Steganogranophy.Size = new System.Drawing.Size(152, 22);
            this.Steganogranophy.TabIndex = 20;
            this.Steganogranophy.Text = "Steganography";
            // 
            // TabDecrytion
            // 
            this.TabDecrytion.Controls.Add(this.BtSnnD);
            this.TabDecrytion.Controls.Add(this.BtSpD);
            this.TabDecrytion.Controls.Add(this.BtMsD);
            this.TabDecrytion.Controls.Add(this.BtHmD);
            this.TabDecrytion.Controls.Add(this.label1);
            this.TabDecrytion.Controls.Add(this.OutputPlaintext);
            this.TabDecrytion.Controls.Add(this.PasteButton1);
            this.TabDecrytion.Controls.Add(this.BtRs4);
            this.TabDecrytion.Controls.Add(this.BtRs5);
            this.TabDecrytion.Controls.Add(this.DecrytionBotton10);
            this.TabDecrytion.Controls.Add(this.CopyButton3);
            this.TabDecrytion.Controls.Add(this.BtRs6);
            this.TabDecrytion.Controls.Add(this.BtSavePlain);
            this.TabDecrytion.Controls.Add(this.PlaintextDecry);
            this.TabDecrytion.Controls.Add(this.InputkeyDecry);
            this.TabDecrytion.Controls.Add(this.KeyDecry);
            this.TabDecrytion.Controls.Add(this.Steganotext2);
            this.TabDecrytion.Controls.Add(this.InputChipertext);
            this.TabDecrytion.Controls.Add(this.Decrytion);
            this.TabDecrytion.Location = new System.Drawing.Point(4, 29);
            this.TabDecrytion.Name = "TabDecrytion";
            this.TabDecrytion.Size = new System.Drawing.Size(798, 663);
            this.TabDecrytion.TabIndex = 2;
            this.TabDecrytion.Text = "Decryption";
            this.TabDecrytion.UseVisualStyleBackColor = true;
            // 
            // BtSnnD
            // 
            this.BtSnnD.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtSnnD.Location = new System.Drawing.Point(448, 309);
            this.BtSnnD.Name = "BtSnnD";
            this.BtSnnD.Size = new System.Drawing.Size(100, 37);
            this.BtSnnD.TabIndex = 44;
            this.BtSnnD.Text = "Synonym";
            this.BtSnnD.UseVisualStyleBackColor = true;
            this.BtSnnD.Click += new System.EventHandler(this.BtSnnD_Click);
            // 
            // BtSpD
            // 
            this.BtSpD.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtSpD.Location = new System.Drawing.Point(367, 309);
            this.BtSpD.Name = "BtSpD";
            this.BtSpD.Size = new System.Drawing.Size(75, 37);
            this.BtSpD.TabIndex = 43;
            this.BtSpD.Text = "Space";
            this.BtSpD.UseVisualStyleBackColor = true;
            this.BtSpD.Click += new System.EventHandler(this.BtSpD_Click);
            // 
            // BtMsD
            // 
            this.BtMsD.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtMsD.Location = new System.Drawing.Point(253, 309);
            this.BtMsD.Name = "BtMsD";
            this.BtMsD.Size = new System.Drawing.Size(108, 37);
            this.BtMsD.TabIndex = 42;
            this.BtMsD.Text = "Misspelling";
            this.BtMsD.UseVisualStyleBackColor = true;
            this.BtMsD.Click += new System.EventHandler(this.BtMsD_Click);
            // 
            // BtHmD
            // 
            this.BtHmD.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtHmD.Location = new System.Drawing.Point(137, 309);
            this.BtHmD.Name = "BtHmD";
            this.BtHmD.Size = new System.Drawing.Size(110, 37);
            this.BtHmD.TabIndex = 41;
            this.BtHmD.Text = "Homoglyph";
            this.BtHmD.UseVisualStyleBackColor = true;
            this.BtHmD.Click += new System.EventHandler(this.BtHmD_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(46, 316);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 22);
            this.label1.TabIndex = 40;
            this.label1.Text = "Algorithm";
            // 
            // OutputPlaintext
            // 
            this.OutputPlaintext.BackColor = System.Drawing.SystemColors.Control;
            this.OutputPlaintext.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.OutputPlaintext.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OutputPlaintext.Location = new System.Drawing.Point(52, 421);
            this.OutputPlaintext.Multiline = true;
            this.OutputPlaintext.Name = "OutputPlaintext";
            this.OutputPlaintext.ReadOnly = true;
            this.OutputPlaintext.Size = new System.Drawing.Size(677, 113);
            this.OutputPlaintext.TabIndex = 39;
            // 
            // PasteButton1
            // 
            this.PasteButton1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.PasteButton1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PasteButton1.ForeColor = System.Drawing.Color.Purple;
            this.PasteButton1.Location = new System.Drawing.Point(364, 246);
            this.PasteButton1.Name = "PasteButton1";
            this.PasteButton1.Size = new System.Drawing.Size(80, 35);
            this.PasteButton1.TabIndex = 37;
            this.PasteButton1.Text = "Paste";
            this.PasteButton1.UseVisualStyleBackColor = false;
            this.PasteButton1.Click += new System.EventHandler(this.PasteButton1_Click);
            // 
            // BtRs4
            // 
            this.BtRs4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.BtRs4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtRs4.ForeColor = System.Drawing.Color.SaddleBrown;
            this.BtRs4.Location = new System.Drawing.Point(651, 205);
            this.BtRs4.Name = "BtRs4";
            this.BtRs4.Size = new System.Drawing.Size(80, 35);
            this.BtRs4.TabIndex = 36;
            this.BtRs4.Text = "Reset";
            this.BtRs4.UseVisualStyleBackColor = false;
            this.BtRs4.Click += new System.EventHandler(this.ResetBotton4_Click);
            // 
            // BtRs5
            // 
            this.BtRs5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.BtRs5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtRs5.ForeColor = System.Drawing.Color.SaddleBrown;
            this.BtRs5.Location = new System.Drawing.Point(448, 246);
            this.BtRs5.Name = "BtRs5";
            this.BtRs5.Size = new System.Drawing.Size(80, 35);
            this.BtRs5.TabIndex = 35;
            this.BtRs5.Text = "Reset";
            this.BtRs5.UseVisualStyleBackColor = false;
            this.BtRs5.Click += new System.EventHandler(this.BtRs5_Click);
            // 
            // DecrytionBotton10
            // 
            this.DecrytionBotton10.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.DecrytionBotton10.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DecrytionBotton10.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.DecrytionBotton10.Location = new System.Drawing.Point(616, 354);
            this.DecrytionBotton10.Name = "DecrytionBotton10";
            this.DecrytionBotton10.Size = new System.Drawing.Size(113, 35);
            this.DecrytionBotton10.TabIndex = 34;
            this.DecrytionBotton10.Text = "Decryption";
            this.DecrytionBotton10.UseVisualStyleBackColor = false;
            this.DecrytionBotton10.Click += new System.EventHandler(this.DecrytionBotton10_Click);
            // 
            // CopyButton3
            // 
            this.CopyButton3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.CopyButton3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CopyButton3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.CopyButton3.Location = new System.Drawing.Point(566, 549);
            this.CopyButton3.Name = "CopyButton3";
            this.CopyButton3.Size = new System.Drawing.Size(80, 35);
            this.CopyButton3.TabIndex = 33;
            this.CopyButton3.Text = "Copy";
            this.CopyButton3.UseVisualStyleBackColor = false;
            this.CopyButton3.Click += new System.EventHandler(this.CopyButton3_Click);
            // 
            // BtRs6
            // 
            this.BtRs6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.BtRs6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtRs6.ForeColor = System.Drawing.Color.SaddleBrown;
            this.BtRs6.Location = new System.Drawing.Point(651, 549);
            this.BtRs6.Name = "BtRs6";
            this.BtRs6.Size = new System.Drawing.Size(80, 35);
            this.BtRs6.TabIndex = 32;
            this.BtRs6.Text = "Reset";
            this.BtRs6.UseVisualStyleBackColor = false;
            this.BtRs6.Click += new System.EventHandler(this.BtRs6_Click);
            // 
            // BtSavePlain
            // 
            this.BtSavePlain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.BtSavePlain.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtSavePlain.ForeColor = System.Drawing.Color.DarkGreen;
            this.BtSavePlain.Location = new System.Drawing.Point(566, 590);
            this.BtSavePlain.Name = "BtSavePlain";
            this.BtSavePlain.Size = new System.Drawing.Size(165, 35);
            this.BtSavePlain.TabIndex = 45;
            this.BtSavePlain.Text = "Save Plaintext";
            this.BtSavePlain.UseVisualStyleBackColor = false;
            this.BtSavePlain.Click += new System.EventHandler(this.BtSavePlain_Click);
            // 
            // PlaintextDecry
            // 
            this.PlaintextDecry.AutoSize = true;
            this.PlaintextDecry.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PlaintextDecry.Location = new System.Drawing.Point(50, 389);
            this.PlaintextDecry.Name = "PlaintextDecry";
            this.PlaintextDecry.Size = new System.Drawing.Size(79, 22);
            this.PlaintextDecry.TabIndex = 31;
            this.PlaintextDecry.Text = "Plaintext";
            // 
            // InputkeyDecry
            // 
            this.InputkeyDecry.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.InputkeyDecry.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InputkeyDecry.Location = new System.Drawing.Point(102, 250);
            this.InputkeyDecry.Name = "InputkeyDecry";
            this.InputkeyDecry.Size = new System.Drawing.Size(248, 28);
            this.InputkeyDecry.TabIndex = 30;
            // 
            // KeyDecry
            // 
            this.KeyDecry.AutoSize = true;
            this.KeyDecry.BackColor = System.Drawing.Color.Transparent;
            this.KeyDecry.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyDecry.ForeColor = System.Drawing.Color.Black;
            this.KeyDecry.Location = new System.Drawing.Point(55, 252);
            this.KeyDecry.Name = "KeyDecry";
            this.KeyDecry.Size = new System.Drawing.Size(41, 22);
            this.KeyDecry.TabIndex = 29;
            this.KeyDecry.Text = "Key";
            // 
            // Steganotext2
            // 
            this.Steganotext2.AutoSize = true;
            this.Steganotext2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Steganotext2.Location = new System.Drawing.Point(50, 52);
            this.Steganotext2.Name = "Steganotext2";
            this.Steganotext2.Size = new System.Drawing.Size(106, 22);
            this.Steganotext2.TabIndex = 25;
            this.Steganotext2.Text = "Steganotext";
            // 
            // InputChipertext
            // 
            this.InputChipertext.BackColor = System.Drawing.SystemColors.Control;
            this.InputChipertext.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.InputChipertext.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InputChipertext.ForeColor = System.Drawing.Color.Black;
            this.InputChipertext.Location = new System.Drawing.Point(52, 86);
            this.InputChipertext.Multiline = true;
            this.InputChipertext.Name = "InputChipertext";
            this.InputChipertext.Size = new System.Drawing.Size(677, 113);
            this.InputChipertext.TabIndex = 24;
            // 
            // Decrytion
            // 
            this.Decrytion.AutoSize = true;
            this.Decrytion.BackColor = System.Drawing.Color.Transparent;
            this.Decrytion.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Decrytion.ForeColor = System.Drawing.Color.Black;
            this.Decrytion.Location = new System.Drawing.Point(50, 15);
            this.Decrytion.Name = "Decrytion";
            this.Decrytion.Size = new System.Drawing.Size(96, 22);
            this.Decrytion.TabIndex = 23;
            this.Decrytion.Text = "Decryption";
            // 
            // ssMain
            // 
            this.ssMain.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.ssMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsslLabel});
            this.ssMain.Location = new System.Drawing.Point(0, 750);
            this.ssMain.Name = "ssMain";
            this.ssMain.Size = new System.Drawing.Size(826, 32);
            this.ssMain.TabIndex = 7;
            this.ssMain.Text = "statusStrip1";
            // 
            // tsslLabel
            // 
            this.tsslLabel.Name = "tsslLabel";
            this.tsslLabel.Size = new System.Drawing.Size(179, 25);
            this.tsslLabel.Text = "toolStripStatusLabel1";
            // 
            // menuStrip1
            // 
            this.menuStrip1.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(826, 33);
            this.menuStrip1.TabIndex = 8;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(54, 29);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(141, 34);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem,
            this.steganographyInfoToolStripMenuItem,
            this.fileFormatToolStripMenuItem,
            this.securityNoticeToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(65, 29);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(272, 34);
            this.aboutToolStripMenuItem.Text = "How to Use";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // steganographyInfoToolStripMenuItem
            // 
            this.steganographyInfoToolStripMenuItem.Name = "steganographyInfoToolStripMenuItem";
            this.steganographyInfoToolStripMenuItem.Size = new System.Drawing.Size(272, 34);
            this.steganographyInfoToolStripMenuItem.Text = "Steganography Info";
            this.steganographyInfoToolStripMenuItem.Click += new System.EventHandler(this.steganographyInfoToolStripMenuItem_Click);
            // 
            // fileFormatToolStripMenuItem
            // 
            this.fileFormatToolStripMenuItem.Name = "fileFormatToolStripMenuItem";
            this.fileFormatToolStripMenuItem.Size = new System.Drawing.Size(272, 34);
            this.fileFormatToolStripMenuItem.Text = "File Format";
            this.fileFormatToolStripMenuItem.Click += new System.EventHandler(this.fileFormatToolStripMenuItem_Click);
            // 
            // securityNoticeToolStripMenuItem
            // 
            this.securityNoticeToolStripMenuItem.Name = "securityNoticeToolStripMenuItem";
            this.securityNoticeToolStripMenuItem.Size = new System.Drawing.Size(272, 34);
            this.securityNoticeToolStripMenuItem.Text = "Security Notice";
            this.securityNoticeToolStripMenuItem.Click += new System.EventHandler(this.securityNoticeToolStripMenuItem_Click);
            // 
            // SteganographyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(826, 782);
            this.Controls.Add(this.ssMain);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.TabControl1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "SteganographyForm";
            this.Text = "Steganography";
            this.Load += new System.EventHandler(this.SteganographyForm_Load);
            this.TabControl1.ResumeLayout(false);
            this.TabEncrytion.ResumeLayout(false);
            this.TabEncrytion.PerformLayout();
            this.TabSteganography.ResumeLayout(false);
            this.TabSteganography.PerformLayout();
            this.TabDecrytion.ResumeLayout(false);
            this.TabDecrytion.PerformLayout();
            this.ssMain.ResumeLayout(false);
            this.ssMain.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

      
        private System.Windows.Forms.TabControl TabControl1;
        private System.Windows.Forms.TabPage TabEncrytion;
        private System.Windows.Forms.TabPage TabSteganography;
        private System.Windows.Forms.TabPage TabDecrytion;
        private System.Windows.Forms.Label ciphertext;
        private System.Windows.Forms.Label Plaintext;
        private System.Windows.Forms.TextBox InputPlaintext;
        private System.Windows.Forms.Label Encrytion;
        private System.Windows.Forms.TextBox InputKey;
        private System.Windows.Forms.Label Key;
        private System.Windows.Forms.Label CoverText;
        private System.Windows.Forms.Button BtRs1;
        private System.Windows.Forms.Button BtRs2;
        private System.Windows.Forms.Button EncrytionButton1;
        private System.Windows.Forms.Button CopyButton2;
        private System.Windows.Forms.Button ResetBotton3;
        private System.Windows.Forms.TextBox InputCovertext;
        private System.Windows.Forms.TextBox InputPayload;
        private System.Windows.Forms.TextBox OutputPlaintext;
        private System.Windows.Forms.Button PasteButton1;
        private System.Windows.Forms.Button BtRs4;
        private System.Windows.Forms.Button BtRs5;
        private System.Windows.Forms.Button DecrytionBotton10;
        private System.Windows.Forms.Button CopyButton3;
        private System.Windows.Forms.Button BtRs6;
        private System.Windows.Forms.Label PlaintextDecry;
        private System.Windows.Forms.TextBox InputkeyDecry;
        private System.Windows.Forms.Label KeyDecry;
        private System.Windows.Forms.Label Steganotext2;
        private System.Windows.Forms.TextBox InputChipertext;
        private System.Windows.Forms.Label Decrytion;
        private System.Windows.Forms.Button CopyButton1;
        private System.Windows.Forms.Button BtRs3;
        private System.Windows.Forms.Label Ciphertextname;
        private System.Windows.Forms.Label Steganogranophy;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label Steganotext;
        private System.Windows.Forms.TextBox TbStegano;
        private System.Windows.Forms.Button openbrowser;
        private System.Windows.Forms.TextBox namefile;
        private System.Windows.Forms.Button BtHm;
        private System.Windows.Forms.StatusStrip ssMain;
        private System.Windows.Forms.ToolStripStatusLabel tsslLabel;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.Button BtSp;
        private System.Windows.Forms.Button BtMs;
        private System.Windows.Forms.Label LbFn;
        private System.Windows.Forms.Button BtSnn;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button BtS;
        private System.Windows.Forms.Button CopyStegano;
        private System.Windows.Forms.Button BtSnnD;
        private System.Windows.Forms.Button BtSpD;
        private System.Windows.Forms.Button BtMsD;
        private System.Windows.Forms.Button BtHmD;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripMenuItem steganographyInfoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileFormatToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem securityNoticeToolStripMenuItem;
        private System.Windows.Forms.Button BtSavePlain;
    }
}
