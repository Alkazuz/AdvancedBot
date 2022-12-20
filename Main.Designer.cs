namespace AdvancedBot
{
    partial class Main
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.tsUtils = new System.Windows.Forms.ToolStripMenuItem();
            this.tsSpammer = new System.Windows.Forms.ToolStripMenuItem();
            this.tsStatistics = new System.Windows.Forms.ToolStripMenuItem();
            this.scriptsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsOpt = new System.Windows.Forms.ToolStripMenuItem();
            this.tsKnockback = new System.Windows.Forms.ToolStripMenuItem();
            this.tsAutoReco = new System.Windows.Forms.ToolStripMenuItem();
            this.mineradorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.protetorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsInfo = new System.Windows.Forms.ToolStripMenuItem();
            this.tsChangelog = new System.Windows.Forms.ToolStripMenuItem();
            this.tsAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.lbOps = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.tsRemove = new System.Windows.Forms.ToolStripMenuItem();
            this.selecionadosToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.movimentarToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.tsView = new System.Windows.Forms.ToolStripMenuItem();
            this.tsSave = new System.Windows.Forms.ToolStripMenuItem();
            this.tsRemDiscon = new System.Windows.Forms.ToolStripMenuItem();
            this.conectarDesconectadosToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.desconectarTodosToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rtbChat = new System.Windows.Forms.RichTextBox();
            this.chatOptions = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.rtbOptCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.rtbOptChangeBkColor = new System.Windows.Forms.ToolStripMenuItem();
            this.rtbOptChatChangeFont = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiHistorySize = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tabChat = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.btnSendInput = new System.Windows.Forms.Button();
            this.tbChatInput = new System.Windows.Forms.TextBox();
            this.chatUpdater = new System.Windows.Forms.Timer(this.components);
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.chatTip = new System.Windows.Forms.ToolTip(this.components);
            this.lbUsers = new AdvancedBot.Controls.UserListBox();
            this.menuStrip1.SuspendLayout();
            this.lbOps.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.chatOptions.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tabChat.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.SystemColors.Control;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsUtils,
            this.tsOpt,
            this.tsInfo});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(646, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // tsUtils
            // 
            this.tsUtils.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsSpammer,
            this.tsStatistics,
            this.scriptsToolStripMenuItem});
            this.tsUtils.Name = "tsUtils";
            this.tsUtils.Size = new System.Drawing.Size(71, 20);
            this.tsUtils.Text = "Utilidades";
            // 
            // tsSpammer
            // 
            this.tsSpammer.Enabled = false;
            this.tsSpammer.Name = "tsSpammer";
            this.tsSpammer.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.F)));
            this.tsSpammer.Size = new System.Drawing.Size(197, 22);
            this.tsSpammer.Text = "Spammer";
            this.tsSpammer.Click += new System.EventHandler(this.tsSpammer_Click);
            // 
            // tsStatistics
            // 
            this.tsStatistics.Name = "tsStatistics";
            this.tsStatistics.Size = new System.Drawing.Size(197, 22);
            this.tsStatistics.Text = "Estatísticas";
            this.tsStatistics.Click += new System.EventHandler(this.tsDebug_Click);
            // 
            // scriptsToolStripMenuItem
            // 
            this.scriptsToolStripMenuItem.Name = "scriptsToolStripMenuItem";
            this.scriptsToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.scriptsToolStripMenuItem.Text = "Editor de macros";
            this.scriptsToolStripMenuItem.Click += new System.EventHandler(this.scriptsToolStripMenuItem_Click);
            // 
            // tsOpt
            // 
            this.tsOpt.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsKnockback,
            this.tsAutoReco,
            this.mineradorToolStripMenuItem,
            this.protetorToolStripMenuItem});
            this.tsOpt.Name = "tsOpt";
            this.tsOpt.Size = new System.Drawing.Size(59, 20);
            this.tsOpt.Text = "Opções";
            // 
            // tsKnockback
            // 
            this.tsKnockback.CheckOnClick = true;
            this.tsKnockback.Name = "tsKnockback";
            this.tsKnockback.Size = new System.Drawing.Size(156, 22);
            this.tsKnockback.Text = "Knockback";
            this.tsKnockback.Click += new System.EventHandler(this.tsKnockback_Click);
            // 
            // tsAutoReco
            // 
            this.tsAutoReco.CheckOnClick = true;
            this.tsAutoReco.Name = "tsAutoReco";
            this.tsAutoReco.Size = new System.Drawing.Size(156, 22);
            this.tsAutoReco.Text = "Auto reconnect";
            this.tsAutoReco.Click += new System.EventHandler(this.tsAutoReco_Click);
            // 
            // mineradorToolStripMenuItem
            // 
            this.mineradorToolStripMenuItem.Name = "mineradorToolStripMenuItem";
            this.mineradorToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.mineradorToolStripMenuItem.Text = "Minerador...";
            this.mineradorToolStripMenuItem.Click += new System.EventHandler(this.mineradorToolStripMenuItem_Click);
            // 
            // protetorToolStripMenuItem
            // 
            this.protetorToolStripMenuItem.Name = "protetorToolStripMenuItem";
            this.protetorToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.protetorToolStripMenuItem.Text = "Protetor";
            this.protetorToolStripMenuItem.Click += new System.EventHandler(this.protetorToolStripMenuItem_Click);
            // 
            // tsInfo
            // 
            this.tsInfo.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsChangelog,
            this.tsAbout});
            this.tsInfo.Name = "tsInfo";
            this.tsInfo.Size = new System.Drawing.Size(85, 20);
            this.tsInfo.Text = "Informações";
            // 
            // tsChangelog
            // 
            this.tsChangelog.Image = global::AdvancedBot.Properties.Resources.changelog;
            this.tsChangelog.Name = "tsChangelog";
            this.tsChangelog.Size = new System.Drawing.Size(132, 22);
            this.tsChangelog.Text = "Changelog";
            this.tsChangelog.Click += new System.EventHandler(this.tsChangelog_Click);
            // 
            // tsAbout
            // 
            this.tsAbout.Image = global::AdvancedBot.Properties.Resources.about;
            this.tsAbout.Name = "tsAbout";
            this.tsAbout.Size = new System.Drawing.Size(132, 22);
            this.tsAbout.Text = "Sobre";
            this.tsAbout.Click += new System.EventHandler(this.tsAbout_Click);
            // 
            // lbOps
            // 
            this.lbOps.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsAdd,
            this.tsRemove,
            this.selecionadosToolStripMenuItem,
            this.tsView,
            this.tsSave,
            this.tsRemDiscon,
            this.conectarDesconectadosToolStripMenuItem,
            this.desconectarTodosToolStripMenuItem});
            this.lbOps.Name = "lbOps";
            this.lbOps.Size = new System.Drawing.Size(287, 180);
            // 
            // tsAdd
            // 
            this.tsAdd.Name = "tsAdd";
            this.tsAdd.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.A)));
            this.tsAdd.Size = new System.Drawing.Size(286, 22);
            this.tsAdd.Text = "Adicionar...";
            this.tsAdd.Click += new System.EventHandler(this.tsAdd_Click);
            // 
            // tsRemove
            // 
            this.tsRemove.Name = "tsRemove";
            this.tsRemove.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Delete)));
            this.tsRemove.Size = new System.Drawing.Size(286, 22);
            this.tsRemove.Text = "Remover";
            this.tsRemove.Click += new System.EventHandler(this.removerToolStripMenuItem_Click);
            // 
            // selecionadosToolStripMenuItem
            // 
            this.selecionadosToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.movimentarToolStripMenuItem1});
            this.selecionadosToolStripMenuItem.Name = "selecionadosToolStripMenuItem";
            this.selecionadosToolStripMenuItem.Size = new System.Drawing.Size(286, 22);
            this.selecionadosToolStripMenuItem.Text = "Selecionados";
            // 
            // movimentarToolStripMenuItem1
            // 
            this.movimentarToolStripMenuItem1.Name = "movimentarToolStripMenuItem1";
            this.movimentarToolStripMenuItem1.Size = new System.Drawing.Size(139, 22);
            this.movimentarToolStripMenuItem1.Text = "Movimentar";
            this.movimentarToolStripMenuItem1.Click += new System.EventHandler(this.movimentarToolStripMenuItem1_Click);
            // 
            // tsView
            // 
            this.tsView.Enabled = false;
            this.tsView.Name = "tsView";
            this.tsView.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.V)));
            this.tsView.Size = new System.Drawing.Size(286, 22);
            this.tsView.Text = "Visualizar";
            this.tsView.Click += new System.EventHandler(this.tsView_Click);
            // 
            // tsSave
            // 
            this.tsSave.Name = "tsSave";
            this.tsSave.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
            this.tsSave.Size = new System.Drawing.Size(286, 22);
            this.tsSave.Text = "Salvar estado...";
            this.tsSave.Click += new System.EventHandler(this.tsSave_Click);
            // 
            // tsRemDiscon
            // 
            this.tsRemDiscon.Name = "tsRemDiscon";
            this.tsRemDiscon.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.Delete)));
            this.tsRemDiscon.Size = new System.Drawing.Size(286, 22);
            this.tsRemDiscon.Text = "Remover desconectados";
            this.tsRemDiscon.Click += new System.EventHandler(this.tsRemDiscon_Click);
            // 
            // conectarDesconectadosToolStripMenuItem
            // 
            this.conectarDesconectadosToolStripMenuItem.Name = "conectarDesconectadosToolStripMenuItem";
            this.conectarDesconectadosToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.R)));
            this.conectarDesconectadosToolStripMenuItem.Size = new System.Drawing.Size(286, 22);
            this.conectarDesconectadosToolStripMenuItem.Text = "Conectar desconectados";
            this.conectarDesconectadosToolStripMenuItem.Click += new System.EventHandler(this.conectarDesconectadosToolStripMenuItem_Click);
            // 
            // desconectarTodosToolStripMenuItem
            // 
            this.desconectarTodosToolStripMenuItem.Name = "desconectarTodosToolStripMenuItem";
            this.desconectarTodosToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.Delete)));
            this.desconectarTodosToolStripMenuItem.Size = new System.Drawing.Size(286, 22);
            this.desconectarTodosToolStripMenuItem.Text = "Remover todos";
            this.desconectarTodosToolStripMenuItem.Click += new System.EventHandler(this.desconectarTodosToolStripMenuItem_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.BackColor = System.Drawing.SystemColors.Control;
            this.groupBox1.Controls.Add(this.lbUsers);
            this.groupBox1.Location = new System.Drawing.Point(12, 35);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(134, 293);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Bots";
            // 
            // rtbChat
            // 
            this.rtbChat.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbChat.BackColor = System.Drawing.Color.Black;
            this.rtbChat.ContextMenuStrip = this.chatOptions;
            this.rtbChat.ForeColor = System.Drawing.SystemColors.WindowText;
            this.rtbChat.HideSelection = false;
            this.rtbChat.Location = new System.Drawing.Point(-4, 0);
            this.rtbChat.Name = "rtbChat";
            this.rtbChat.ReadOnly = true;
            this.rtbChat.Size = new System.Drawing.Size(466, 216);
            this.rtbChat.TabIndex = 2;
            this.rtbChat.Text = "";
            // 
            // chatOptions
            // 
            this.chatOptions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.rtbOptCopy,
            this.rtbOptChangeBkColor,
            this.rtbOptChatChangeFont,
            this.tsmiHistorySize});
            this.chatOptions.Name = "contextMenuStrip1";
            this.chatOptions.Size = new System.Drawing.Size(191, 92);
            this.chatOptions.Opening += new System.ComponentModel.CancelEventHandler(this.ChatOptions_Opening);
            // 
            // rtbOptCopy
            // 
            this.rtbOptCopy.Name = "rtbOptCopy";
            this.rtbOptCopy.Size = new System.Drawing.Size(190, 22);
            this.rtbOptCopy.Text = "Copiar";
            this.rtbOptCopy.Click += new System.EventHandler(this.rtbOptCopy_Click);
            // 
            // rtbOptChangeBkColor
            // 
            this.rtbOptChangeBkColor.Name = "rtbOptChangeBkColor";
            this.rtbOptChangeBkColor.Size = new System.Drawing.Size(190, 22);
            this.rtbOptChangeBkColor.Text = "Mudar cor de fundo...";
            this.rtbOptChangeBkColor.Click += new System.EventHandler(this.rtbOptChangeBkColor_Click);
            // 
            // rtbOptChatChangeFont
            // 
            this.rtbOptChatChangeFont.Name = "rtbOptChatChangeFont";
            this.rtbOptChatChangeFont.Size = new System.Drawing.Size(190, 22);
            this.rtbOptChatChangeFont.Text = "Mudar fonte...";
            this.rtbOptChatChangeFont.Click += new System.EventHandler(this.rtbOptChatChangeFont_Click);
            // 
            // tsmiHistorySize
            // 
            this.tsmiHistorySize.Name = "tsmiHistorySize";
            this.tsmiHistorySize.Size = new System.Drawing.Size(190, 22);
            this.tsmiHistorySize.Text = "Tamanho do histórico";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.tabChat);
            this.groupBox2.Controls.Add(this.btnSendInput);
            this.groupBox2.Controls.Add(this.tbChatInput);
            this.groupBox2.Location = new System.Drawing.Point(152, 35);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(482, 293);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Chat";
            // 
            // tabChat
            // 
            this.tabChat.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabChat.Controls.Add(this.tabPage1);
            this.tabChat.Controls.Add(this.tabPage2);
            this.tabChat.Location = new System.Drawing.Point(6, 16);
            this.tabChat.Name = "tabChat";
            this.tabChat.SelectedIndex = 0;
            this.tabChat.Size = new System.Drawing.Size(470, 239);
            this.tabChat.TabIndex = 5;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.rtbChat);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(462, 213);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Normal";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.AutoScroll = true;
            this.tabPage2.BackColor = System.Drawing.Color.Black;
            this.tabPage2.ForeColor = System.Drawing.Color.Black;
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(462, 213);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Chat Event";
            // 
            // btnSendInput
            // 
            this.btnSendInput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSendInput.Location = new System.Drawing.Point(401, 261);
            this.btnSendInput.Name = "btnSendInput";
            this.btnSendInput.Size = new System.Drawing.Size(75, 23);
            this.btnSendInput.TabIndex = 4;
            this.btnSendInput.Text = "Enviar";
            this.btnSendInput.UseVisualStyleBackColor = true;
            this.btnSendInput.Click += new System.EventHandler(this.btnSendInput_Click);
            // 
            // tbChatInput
            // 
            this.tbChatInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbChatInput.Location = new System.Drawing.Point(10, 263);
            this.tbChatInput.MaxLength = 100;
            this.tbChatInput.Name = "tbChatInput";
            this.tbChatInput.Size = new System.Drawing.Size(384, 20);
            this.tbChatInput.TabIndex = 3;
            this.tbChatInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbChatInput_KeyDown);
            this.tbChatInput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbChatInput_KeyPress);
            this.tbChatInput.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbChatInput_KeyUp);
            // 
            // chatUpdater
            // 
            this.chatUpdater.Enabled = true;
            this.chatUpdater.Tick += new System.EventHandler(this.chatUpdater_Tick);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(599, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(35, 30);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 41;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.PictureBox1_Click);
            // 
            // chatTip
            // 
            this.chatTip.AutoPopDelay = 100;
            this.chatTip.InitialDelay = 100;
            this.chatTip.IsBalloon = true;
            this.chatTip.ReshowDelay = 100;
            this.chatTip.UseFading = false;
            // 
            // lbUsers
            // 
            this.lbUsers.AutoScroll = true;
            this.lbUsers.AutoScrollMinSize = new System.Drawing.Size(0, 2);
            this.lbUsers.BackColor = System.Drawing.Color.White;
            this.lbUsers.ContextMenuStrip = this.lbOps;
            this.lbUsers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbUsers.Location = new System.Drawing.Point(3, 16);
            this.lbUsers.Name = "lbUsers";
            this.lbUsers.Size = new System.Drawing.Size(128, 274);
            this.lbUsers.TabIndex = 0;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(646, 334);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Main";
            this.Load += new System.EventHandler(this.Main_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.lbOps.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.chatOptions.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tabChat.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem tsUtils;
        private System.Windows.Forms.ToolStripMenuItem tsInfo;
        private System.Windows.Forms.RichTextBox rtbChat;
        private System.Windows.Forms.Button btnSendInput;
        private System.Windows.Forms.TextBox tbChatInput;
        public System.Windows.Forms.GroupBox groupBox1;
        public System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ContextMenuStrip lbOps;
        private System.Windows.Forms.ToolStripMenuItem tsRemove;
        public System.Windows.Forms.ToolStripMenuItem tsSpammer;
        private System.Windows.Forms.ToolStripMenuItem tsOpt;
        private System.Windows.Forms.Timer chatUpdater;
        public System.Windows.Forms.ToolStripMenuItem tsKnockback;
        private System.Windows.Forms.ToolStripMenuItem tsStatistics;
        public AdvancedBot.Controls.UserListBox lbUsers;
        public System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tsAdd;
        private System.Windows.Forms.ToolStripMenuItem tsView;
        private System.Windows.Forms.ToolStripMenuItem tsSave;
        private System.Windows.Forms.ToolStripMenuItem tsRemDiscon;
        private System.Windows.Forms.ContextMenuStrip chatOptions;
        private System.Windows.Forms.ToolStripMenuItem rtbOptCopy;
        private System.Windows.Forms.ToolStripMenuItem rtbOptChangeBkColor;
        private System.Windows.Forms.ToolStripMenuItem tsAutoReco;
        private System.Windows.Forms.ToolStripMenuItem conectarDesconectadosToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mineradorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rtbOptChatChangeFont;
        private System.Windows.Forms.ToolStripMenuItem scriptsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tsmiHistorySize;
        private System.Windows.Forms.ToolStripMenuItem desconectarTodosToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tsChangelog;
        private System.Windows.Forms.ToolStripMenuItem tsAbout;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolStripMenuItem protetorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selecionadosToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem movimentarToolStripMenuItem1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TabControl tabChat;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ToolTip chatTip;
    }
}

