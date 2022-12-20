namespace AdvancedBot
{
    partial class Start
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
            this.components = new System.ComponentModel.Container();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.svTb = new System.Windows.Forms.TextBox();
            this.gbAccounts = new System.Windows.Forms.GroupBox();
            this.rtbAccounts = new System.Windows.Forms.RichTextBox();
            this.rtbMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.gen5Random = new System.Windows.Forms.ToolStripMenuItem();
            this.gen10Random = new System.Windows.Forms.ToolStripMenuItem();
            this.gen5Pseudo = new System.Windows.Forms.ToolStripMenuItem();
            this.gen5Prefix = new System.Windows.Forms.ToolStripMenuItem();
            this.gen5PrefixSeq = new System.Windows.Forms.ToolStripMenuItem();
            this.definirPrefixoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnProxy = new System.Windows.Forms.Button();
            this.btnLoadState = new System.Windows.Forms.Button();
            this.btnChkAccs = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.cbAutoLogin = new System.Windows.Forms.CheckBox();
            this.nudConnLimit = new System.Windows.Forms.NumericUpDown();
            this.cbLimitChunks = new System.Windows.Forms.CheckBox();
            this.cbDoPing = new System.Windows.Forms.CheckBox();
            this.cbPhysics = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.cbVersion = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.nudDelay = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.gbAccounts.SuspendLayout();
            this.rtbMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudConnLimit)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudDelay)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.svTb);
            this.groupBox1.Location = new System.Drawing.Point(12, 11);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(319, 48);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Servidor";
            // 
            // svTb
            // 
            this.svTb.Location = new System.Drawing.Point(6, 19);
            this.svTb.Name = "svTb";
            this.svTb.Size = new System.Drawing.Size(307, 20);
            this.svTb.TabIndex = 0;
            this.svTb.Text = "jogar.craftlandia.com.br";
            this.toolTip1.SetToolTip(this.svTb, "Endereço do servidor no formato IP[:porta].");
            // 
            // gbAccounts
            // 
            this.gbAccounts.Controls.Add(this.rtbAccounts);
            this.gbAccounts.Location = new System.Drawing.Point(12, 65);
            this.gbAccounts.Name = "gbAccounts";
            this.gbAccounts.Size = new System.Drawing.Size(319, 201);
            this.gbAccounts.TabIndex = 1;
            this.gbAccounts.TabStop = false;
            this.gbAccounts.Text = "Contas";
            // 
            // rtbAccounts
            // 
            this.rtbAccounts.ContextMenuStrip = this.rtbMenuStrip;
            this.rtbAccounts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbAccounts.Location = new System.Drawing.Point(3, 16);
            this.rtbAccounts.Name = "rtbAccounts";
            this.rtbAccounts.Size = new System.Drawing.Size(313, 182);
            this.rtbAccounts.TabIndex = 1;
            this.rtbAccounts.Text = "Nick1:SenhaDoNick1\nNick2:SenhaDoNick2\nNick3:SenhaDoNick3\n...\n";
            this.toolTip1.SetToolTip(this.rtbAccounts, "Lista de contas. (Clique com o botão direito para gerar..)");
            this.rtbAccounts.TextChanged += new System.EventHandler(this.rtbAccounts_TextChanged);
            // 
            // rtbMenuStrip
            // 
            this.rtbMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gen5Random,
            this.gen10Random,
            this.gen5Pseudo,
            this.gen5Prefix,
            this.gen5PrefixSeq,
            this.definirPrefixoToolStripMenuItem});
            this.rtbMenuStrip.Name = "contextMenuStrip1";
            this.rtbMenuStrip.Size = new System.Drawing.Size(359, 136);
            this.rtbMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.rtbMenuStrip_Opening);
            // 
            // gen5Random
            // 
            this.gen5Random.Name = "gen5Random";
            this.gen5Random.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.G)));
            this.gen5Random.Size = new System.Drawing.Size(358, 22);
            this.gen5Random.Text = "Gerar 5 nicks (aleatório)";
            this.gen5Random.Click += new System.EventHandler(this.gen5Random_Click);
            // 
            // gen10Random
            // 
            this.gen10Random.Name = "gen10Random";
            this.gen10Random.Size = new System.Drawing.Size(358, 22);
            this.gen10Random.Text = "Gerar 10 nicks (aleatório)";
            this.gen10Random.Click += new System.EventHandler(this.gen10Random_Click);
            // 
            // gen5Pseudo
            // 
            this.gen5Pseudo.Name = "gen5Pseudo";
            this.gen5Pseudo.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.G)));
            this.gen5Pseudo.Size = new System.Drawing.Size(358, 22);
            this.gen5Pseudo.Text = "Gerar 5 nicks (pseudo)";
            this.gen5Pseudo.Click += new System.EventHandler(this.gen5Pseudo_Click);
            // 
            // gen5Prefix
            // 
            this.gen5Prefix.Name = "gen5Prefix";
            this.gen5Prefix.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
            this.gen5Prefix.Size = new System.Drawing.Size(358, 22);
            this.gen5Prefix.Text = "Gerar 5 nicks com o prefixo \'\'";
            this.gen5Prefix.Click += new System.EventHandler(this.gen5Prefix_Click);
            // 
            // gen5PrefixSeq
            // 
            this.gen5PrefixSeq.Name = "gen5PrefixSeq";
            this.gen5PrefixSeq.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.P)));
            this.gen5PrefixSeq.Size = new System.Drawing.Size(358, 22);
            this.gen5PrefixSeq.Text = "Gerar 5 nicks com o prefixo \'\' sequencial";
            this.gen5PrefixSeq.Click += new System.EventHandler(this.gen5PrefixSeq_Click);
            // 
            // definirPrefixoToolStripMenuItem
            // 
            this.definirPrefixoToolStripMenuItem.Name = "definirPrefixoToolStripMenuItem";
            this.definirPrefixoToolStripMenuItem.Size = new System.Drawing.Size(358, 22);
            this.definirPrefixoToolStripMenuItem.Text = "Definir prefixo...";
            this.definirPrefixoToolStripMenuItem.Click += new System.EventHandler(this.definirPrefixoToolStripMenuItem_Click);
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(262, 387);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(69, 23);
            this.btnOk.TabIndex = 6;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnProxy
            // 
            this.btnProxy.Location = new System.Drawing.Point(188, 387);
            this.btnProxy.Name = "btnProxy";
            this.btnProxy.Size = new System.Drawing.Size(69, 23);
            this.btnProxy.TabIndex = 7;
            this.btnProxy.Text = "Proxy...";
            this.toolTip1.SetToolTip(this.btnProxy, "Usar proxies...");
            this.btnProxy.UseVisualStyleBackColor = true;
            this.btnProxy.Click += new System.EventHandler(this.btnProxy_Click);
            // 
            // btnLoadState
            // 
            this.btnLoadState.Location = new System.Drawing.Point(113, 387);
            this.btnLoadState.Name = "btnLoadState";
            this.btnLoadState.Size = new System.Drawing.Size(69, 23);
            this.btnLoadState.TabIndex = 8;
            this.btnLoadState.Text = "Abrir...";
            this.toolTip1.SetToolTip(this.btnLoadState, "Abrir estado...\r\n");
            this.btnLoadState.UseVisualStyleBackColor = true;
            this.btnLoadState.Click += new System.EventHandler(this.btnLoadState_Click);
            // 
            // btnChkAccs
            // 
            this.btnChkAccs.Location = new System.Drawing.Point(14, 387);
            this.btnChkAccs.Name = "btnChkAccs";
            this.btnChkAccs.Size = new System.Drawing.Size(93, 23);
            this.btnChkAccs.TabIndex = 9;
            this.btnChkAccs.Text = "Checar contas...\r\n";
            this.toolTip1.SetToolTip(this.btnChkAccs, "Checador de contas. (Mojang)");
            this.btnChkAccs.UseVisualStyleBackColor = true;
            this.btnChkAccs.Click += new System.EventHandler(this.btnChkAccs_Click);
            // 
            // cbAutoLogin
            // 
            this.cbAutoLogin.AutoSize = true;
            this.cbAutoLogin.Location = new System.Drawing.Point(179, 75);
            this.cbAutoLogin.Name = "cbAutoLogin";
            this.cbAutoLogin.Size = new System.Drawing.Size(77, 17);
            this.cbAutoLogin.TabIndex = 21;
            this.cbAutoLogin.Text = "Auto Login";
            this.toolTip1.SetToolTip(this.cbAutoLogin, "Login / Registration automatically when entering some Server");
            this.cbAutoLogin.UseVisualStyleBackColor = true;
            this.cbAutoLogin.CheckedChanged += new System.EventHandler(this.CbAutoLogin_CheckedChanged);
            // 
            // nudConnLimit
            // 
            this.nudConnLimit.Location = new System.Drawing.Point(113, 73);
            this.nudConnLimit.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.nudConnLimit.Name = "nudConnLimit";
            this.nudConnLimit.Size = new System.Drawing.Size(50, 20);
            this.nudConnLimit.TabIndex = 19;
            this.toolTip1.SetToolTip(this.nudConnLimit, "Limit amount of connected accounts");
            // 
            // cbLimitChunks
            // 
            this.cbLimitChunks.AutoSize = true;
            this.cbLimitChunks.Location = new System.Drawing.Point(209, 46);
            this.cbLimitChunks.Name = "cbLimitChunks";
            this.cbLimitChunks.Size = new System.Drawing.Size(86, 17);
            this.cbLimitChunks.TabIndex = 14;
            this.cbLimitChunks.Text = "Limit Chunks";
            this.toolTip1.SetToolTip(this.cbLimitChunks, "Limit world chunks");
            this.cbLimitChunks.UseVisualStyleBackColor = true;
            this.cbLimitChunks.CheckedChanged += new System.EventHandler(this.CbLimitChunks_CheckedChanged);
            // 
            // cbDoPing
            // 
            this.cbDoPing.AutoSize = true;
            this.cbDoPing.Location = new System.Drawing.Point(126, 46);
            this.cbDoPing.Name = "cbDoPing";
            this.cbDoPing.Size = new System.Drawing.Size(74, 17);
            this.cbDoPing.TabIndex = 5;
            this.cbDoPing.Text = "Send ping";
            this.toolTip1.SetToolTip(this.cbDoPing, "Simulates having the server in the list of Servers");
            this.cbDoPing.UseVisualStyleBackColor = true;
            // 
            // cbPhysics
            // 
            this.cbPhysics.AutoSize = true;
            this.cbPhysics.Checked = true;
            this.cbPhysics.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbPhysics.Location = new System.Drawing.Point(172, 17);
            this.cbPhysics.Name = "cbPhysics";
            this.cbPhysics.Size = new System.Drawing.Size(122, 17);
            this.cbPhysics.TabIndex = 3;
            this.cbPhysics.Text = "Physics, movements";
            this.toolTip1.SetToolTip(this.cbPhysics, "It makes the bot run, jump, walk etc ...");
            this.cbPhysics.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.cbAutoLogin);
            this.groupBox3.Controls.Add(this.cbVersion);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.nudConnLimit);
            this.groupBox3.Controls.Add(this.cbLimitChunks);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.cbDoPing);
            this.groupBox3.Controls.Add(this.cbPhysics);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.nudDelay);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Location = new System.Drawing.Point(12, 272);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(316, 102);
            this.groupBox3.TabIndex = 13;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Options";
            // 
            // cbVersion
            // 
            this.cbVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbVersion.FormattingEnabled = true;
            this.cbVersion.IntegralHeight = false;
            this.cbVersion.Location = new System.Drawing.Point(52, 44);
            this.cbVersion.MaxDropDownItems = 6;
            this.cbVersion.Name = "cbVersion";
            this.cbVersion.Size = new System.Drawing.Size(69, 21);
            this.cbVersion.TabIndex = 4;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(23, 76);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(75, 13);
            this.label6.TabIndex = 20;
            this.label6.Text = "Limit accounts";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 47);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(45, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "Version:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(148, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(20, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "ms";
            // 
            // nudDelay
            // 
            this.nudDelay.Increment = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudDelay.Location = new System.Drawing.Point(87, 16);
            this.nudDelay.Maximum = new decimal(new int[] {
            120000,
            0,
            0,
            0});
            this.nudDelay.Name = "nudDelay";
            this.nudDelay.Size = new System.Drawing.Size(60, 20);
            this.nudDelay.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Login Delay";
            // 
            // Start
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(343, 424);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.btnChkAccs);
            this.Controls.Add(this.btnLoadState);
            this.Controls.Add(this.btnProxy);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.gbAccounts);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Start";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Inicio";
            this.Load += new System.EventHandler(this.Start_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.gbAccounts.ResumeLayout(false);
            this.rtbMenuStrip.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nudConnLimit)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudDelay)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox gbAccounts;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnProxy;
        private System.Windows.Forms.ContextMenuStrip rtbMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem gen5Random;
        internal System.Windows.Forms.TextBox svTb;
        internal System.Windows.Forms.RichTextBox rtbAccounts;
        private System.Windows.Forms.ToolStripMenuItem gen10Random;
        private System.Windows.Forms.ToolStripMenuItem gen5Pseudo;
        private System.Windows.Forms.Button btnLoadState;
        private System.Windows.Forms.Button btnChkAccs;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolStripMenuItem gen5Prefix;
        private System.Windows.Forms.ToolStripMenuItem definirPrefixoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gen5PrefixSeq;
        private System.Windows.Forms.GroupBox groupBox3;
        public System.Windows.Forms.CheckBox cbAutoLogin;
        private System.Windows.Forms.ComboBox cbVersion;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown nudConnLimit;
        private System.Windows.Forms.CheckBox cbLimitChunks;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.CheckBox cbDoPing;
        private System.Windows.Forms.CheckBox cbPhysics;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown nudDelay;
        private System.Windows.Forms.Label label1;
    }
}