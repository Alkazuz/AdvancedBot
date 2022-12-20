namespace AdvancedBot
{
    partial class ProxyCheckerForm
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
            this.btnStart = new System.Windows.Forms.Button();
            this.rtfProxies = new System.Windows.Forms.RichTextBox();
            this.cms2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiFilter = new System.Windows.Forms.ToolStripMenuItem();
            this.cms1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiCopys4 = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiCopys5 = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiCopyhttp = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiRemoveInvalid = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiRemoveProxyPing = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiRemoveSel = new System.Windows.Forms.ToolStripMenuItem();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.cbServer = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.rbPing = new System.Windows.Forms.RadioButton();
            this.rbLogin = new System.Windows.Forms.RadioButton();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.percentageProgressBar1 = new AdvancedBot.Controls.PercentageProgressBar();
            this.lvProxies = new AdvancedBot.Controls.ProxyListView();
            this.ipPort = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.type = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ping = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.country = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label3 = new System.Windows.Forms.Label();
            this.nudTimeout = new System.Windows.Forms.NumericUpDown();
            this.cms2.SuspendLayout();
            this.cms1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudTimeout)).BeginInit();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(341, 241);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(107, 23);
            this.btnStart.TabIndex = 1;
            this.btnStart.Text = "Iniciar";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // rtfProxies
            // 
            this.rtfProxies.ContextMenuStrip = this.cms2;
            this.rtfProxies.Location = new System.Drawing.Point(12, 26);
            this.rtfProxies.Name = "rtfProxies";
            this.rtfProxies.Size = new System.Drawing.Size(436, 209);
            this.rtfProxies.TabIndex = 3;
            this.rtfProxies.Text = "Insira as proxies\n";
            this.rtfProxies.Click += new System.EventHandler(this.rtfProxies_Click);
            // 
            // cms2
            // 
            this.cms2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiFilter});
            this.cms2.Name = "cms2";
            this.cms2.Size = new System.Drawing.Size(149, 26);
            // 
            // tsmiFilter
            // 
            this.tsmiFilter.Name = "tsmiFilter";
            this.tsmiFilter.Size = new System.Drawing.Size(148, 22);
            this.tsmiFilter.Text = "Filtrar países...";
            this.tsmiFilter.Click += new System.EventHandler(this.tsmiFilter_Click);
            // 
            // cms1
            // 
            this.cms1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiCopys4,
            this.tsmiCopys5,
            this.tsmiCopyhttp,
            this.tsmiRemoveInvalid,
            this.tsmiRemoveProxyPing,
            this.tsmiRemoveSel});
            this.cms1.Name = "cms1";
            this.cms1.Size = new System.Drawing.Size(260, 136);
            // 
            // tsmiCopys4
            // 
            this.tsmiCopys4.Name = "tsmiCopys4";
            this.tsmiCopys4.Size = new System.Drawing.Size(259, 22);
            this.tsmiCopys4.Text = "Copiar proxies Socks4";
            this.tsmiCopys4.Click += new System.EventHandler(this.tsmiCopys4_Click);
            // 
            // tsmiCopys5
            // 
            this.tsmiCopys5.Name = "tsmiCopys5";
            this.tsmiCopys5.Size = new System.Drawing.Size(259, 22);
            this.tsmiCopys5.Text = "Copiar proxies Socks5";
            this.tsmiCopys5.Click += new System.EventHandler(this.tsmiCopys5_Click);
            // 
            // tsmiCopyhttp
            // 
            this.tsmiCopyhttp.Name = "tsmiCopyhttp";
            this.tsmiCopyhttp.Size = new System.Drawing.Size(259, 22);
            this.tsmiCopyhttp.Text = "Copiar proxies HTTP";
            this.tsmiCopyhttp.Click += new System.EventHandler(this.tsmiCopyhttp_Click);
            // 
            // tsmiRemoveInvalid
            // 
            this.tsmiRemoveInvalid.Name = "tsmiRemoveInvalid";
            this.tsmiRemoveInvalid.Size = new System.Drawing.Size(259, 22);
            this.tsmiRemoveInvalid.Text = "Remover proxies inválidas";
            this.tsmiRemoveInvalid.Click += new System.EventHandler(this.tsmiRemoveInvalid_Click);
            // 
            // tsmiRemoveProxyPing
            // 
            this.tsmiRemoveProxyPing.Name = "tsmiRemoveProxyPing";
            this.tsmiRemoveProxyPing.Size = new System.Drawing.Size(259, 22);
            this.tsmiRemoveProxyPing.Text = "Remover proxies com o ping > que";
            // 
            // tsmiRemoveSel
            // 
            this.tsmiRemoveSel.Name = "tsmiRemoveSel";
            this.tsmiRemoveSel.Size = new System.Drawing.Size(259, 22);
            this.tsmiRemoveSel.Text = "Remover proxies selecionadas";
            this.tsmiRemoveSel.Click += new System.EventHandler(this.tsmiRemoveSel_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // cbServer
            // 
            this.cbServer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbServer.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.cbServer.Items.AddRange(new object[] {
            "Nenhum",
            "Craftlandia (jogar.craftlandia.com.br:25565)",
            "SkyMinigames (minigames.redesky.com:25565)",
            "SkySurvival (survival.redesky.com:25565)",
            "Mojang (authserver.mojang.com:80)",
            "Mojang SSL (authserver.mojang.com:443)"});
            this.cbServer.Location = new System.Drawing.Point(111, 2);
            this.cbServer.Name = "cbServer";
            this.cbServer.Size = new System.Drawing.Size(274, 21);
            this.cbServer.TabIndex = 4;
            this.cbServer.SelectedIndexChanged += new System.EventHandler(this.cbServer_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Testar proxies em:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 246);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Modo de chk:";
            // 
            // rbPing
            // 
            this.rbPing.AutoSize = true;
            this.rbPing.Checked = true;
            this.rbPing.Location = new System.Drawing.Point(91, 244);
            this.rbPing.Name = "rbPing";
            this.rbPing.Size = new System.Drawing.Size(46, 17);
            this.rbPing.TabIndex = 7;
            this.rbPing.TabStop = true;
            this.rbPing.Text = "Ping";
            this.toolTip1.SetToolTip(this.rbPing, "Envia um ping (query) para o servidor. Se ele não responder, a proxy provavelment" +
        "e não está funcionando.");
            this.rbPing.UseVisualStyleBackColor = true;
            // 
            // rbLogin
            // 
            this.rbLogin.AutoSize = true;
            this.rbLogin.Location = new System.Drawing.Point(143, 244);
            this.rbLogin.Name = "rbLogin";
            this.rbLogin.Size = new System.Drawing.Size(51, 17);
            this.rbLogin.TabIndex = 8;
            this.rbLogin.Text = "Login";
            this.toolTip1.SetToolTip(this.rbLogin, "Entra um bot no servidor. Se o servidor não responder ou o bot for kick, a proxy " +
        "provavelmente foi bloqueada.");
            this.rbLogin.UseVisualStyleBackColor = true;
            // 
            // percentageProgressBar1
            // 
            this.percentageProgressBar1.Location = new System.Drawing.Point(12, 241);
            this.percentageProgressBar1.Name = "percentageProgressBar1";
            this.percentageProgressBar1.Size = new System.Drawing.Size(323, 23);
            this.percentageProgressBar1.TabIndex = 2;
            this.percentageProgressBar1.Visible = false;
            // 
            // lvProxies
            // 
            this.lvProxies.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ipPort,
            this.type,
            this.ping,
            this.country});
            this.lvProxies.Location = new System.Drawing.Point(12, 26);
            this.lvProxies.Name = "lvProxies";
            this.lvProxies.OwnerDraw = true;
            this.lvProxies.Size = new System.Drawing.Size(436, 209);
            this.lvProxies.TabIndex = 0;
            this.lvProxies.UseCompatibleStateImageBehavior = false;
            this.lvProxies.View = System.Windows.Forms.View.Details;
            this.lvProxies.Visible = false;
            // 
            // ipPort
            // 
            this.ipPort.Text = "IP:Porta";
            this.ipPort.Width = 155;
            // 
            // type
            // 
            this.type.Text = "Tipo";
            this.type.Width = 69;
            // 
            // ping
            // 
            this.ping.Text = "Ping";
            this.ping.Width = 67;
            // 
            // country
            // 
            this.country.Text = "País ";
            this.country.Width = 111;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(200, 246);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(74, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Timeout (sec):";
            // 
            // nudTimeout
            // 
            this.nudTimeout.Location = new System.Drawing.Point(278, 244);
            this.nudTimeout.Maximum = new decimal(new int[] {
            40,
            0,
            0,
            0});
            this.nudTimeout.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.nudTimeout.Name = "nudTimeout";
            this.nudTimeout.Size = new System.Drawing.Size(44, 20);
            this.nudTimeout.TabIndex = 10;
            this.nudTimeout.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
            // 
            // ProxyCheckerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(460, 272);
            this.Controls.Add(this.nudTimeout);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.rbLogin);
            this.Controls.Add(this.rbPing);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbServer);
            this.Controls.Add(this.rtfProxies);
            this.Controls.Add(this.percentageProgressBar1);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.lvProxies);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "ProxyCheckerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Proxy Checker";
            this.Load += new System.EventHandler(this.ProxyCheckerForm_Load);
            this.cms2.ResumeLayout(false);
            this.cms1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nudTimeout)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private AdvancedBot.Controls.ProxyListView lvProxies;
        private System.Windows.Forms.ColumnHeader ipPort;
        private System.Windows.Forms.ColumnHeader ping;
        private System.Windows.Forms.ColumnHeader type;
        private System.Windows.Forms.Button btnStart;
        private AdvancedBot.Controls.PercentageProgressBar percentageProgressBar1;
        private System.Windows.Forms.ContextMenuStrip cms1;
        private System.Windows.Forms.ToolStripMenuItem tsmiCopys4;
        private System.Windows.Forms.ToolStripMenuItem tsmiCopys5;
        private System.Windows.Forms.ToolStripMenuItem tsmiCopyhttp;
        private System.Windows.Forms.ToolStripMenuItem tsmiRemoveInvalid;
        private System.Windows.Forms.Timer timer1;
        public System.Windows.Forms.ComboBox cbServer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripMenuItem tsmiRemoveProxyPing;
        private System.Windows.Forms.ColumnHeader country;
        private System.Windows.Forms.ToolStripMenuItem tsmiRemoveSel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton rbPing;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.RadioButton rbLogin;
        private System.Windows.Forms.ContextMenuStrip cms2;
        private System.Windows.Forms.ToolStripMenuItem tsmiFilter;
        public System.Windows.Forms.RichTextBox rtfProxies;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown nudTimeout;
    }
}

