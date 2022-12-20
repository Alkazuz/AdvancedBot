namespace AdvancedBot
{
    partial class ProxyForm
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
            this.label3 = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnChecker = new System.Windows.Forms.Button();
            this.lvMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiRemove = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiSelect = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiSelectS4 = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiSelectS5 = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiSelectHTTP = new System.Windows.Forms.ToolStripMenuItem();
            this.btnSave = new System.Windows.Forms.Button();
            this.saveMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiS4 = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiS5 = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiHttp = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiAll = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiSaveCsv = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiLoadCsv = new System.Windows.Forms.ToolStripMenuItem();
            this.lvProxies = new AdvancedBot.Controls.ProxyListView();
            this.ipPort = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.type = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.country = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lvMenu.SuspendLayout();
            this.saveMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(0, 13);
            this.label3.TabIndex = 4;
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(319, 271);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 6;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnChecker
            // 
            this.btnChecker.Location = new System.Drawing.Point(227, 271);
            this.btnChecker.Name = "btnChecker";
            this.btnChecker.Size = new System.Drawing.Size(86, 23);
            this.btnChecker.TabIndex = 9;
            this.btnChecker.Text = "Proxy checker";
            this.btnChecker.UseVisualStyleBackColor = true;
            this.btnChecker.Click += new System.EventHandler(this.btnChecker_Click);
            // 
            // lvMenu
            // 
            this.lvMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiAdd,
            this.tsmiRemove,
            this.tsmiCopy,
            this.tsmiSelect});
            this.lvMenu.Name = "contextMenuStrip1";
            this.lvMenu.Size = new System.Drawing.Size(181, 114);
            // 
            // tsmiAdd
            // 
            this.tsmiAdd.Name = "tsmiAdd";
            this.tsmiAdd.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.tsmiAdd.Size = new System.Drawing.Size(180, 22);
            this.tsmiAdd.Text = "Adicionar";
            this.tsmiAdd.Click += new System.EventHandler(this.tsmiAdd_Click);
            // 
            // tsmiRemove
            // 
            this.tsmiRemove.Name = "tsmiRemove";
            this.tsmiRemove.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.tsmiRemove.Size = new System.Drawing.Size(180, 22);
            this.tsmiRemove.Text = "Remover";
            this.tsmiRemove.Click += new System.EventHandler(this.tsmiRemove_Click);
            // 
            // tsmiCopy
            // 
            this.tsmiCopy.Name = "tsmiCopy";
            this.tsmiCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.tsmiCopy.Size = new System.Drawing.Size(180, 22);
            this.tsmiCopy.Text = "Copiar";
            this.tsmiCopy.Click += new System.EventHandler(this.tsmiCopy_Click);
            // 
            // tsmiSelect
            // 
            this.tsmiSelect.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiSelectS4,
            this.tsmiSelectS5,
            this.tsmiSelectHTTP});
            this.tsmiSelect.Name = "tsmiSelect";
            this.tsmiSelect.Size = new System.Drawing.Size(180, 22);
            this.tsmiSelect.Text = "Selecionar";
            // 
            // tsmiSelectS4
            // 
            this.tsmiSelectS4.Name = "tsmiSelectS4";
            this.tsmiSelectS4.Size = new System.Drawing.Size(180, 22);
            this.tsmiSelectS4.Text = "Socks4";
            this.tsmiSelectS4.Click += new System.EventHandler(this.tsmiSelectS4_Click);
            // 
            // tsmiSelectS5
            // 
            this.tsmiSelectS5.Name = "tsmiSelectS5";
            this.tsmiSelectS5.Size = new System.Drawing.Size(180, 22);
            this.tsmiSelectS5.Text = "Socks5";
            this.tsmiSelectS5.Click += new System.EventHandler(this.tsmiSelectS5_Click);
            // 
            // tsmiSelectHTTP
            // 
            this.tsmiSelectHTTP.Name = "tsmiSelectHTTP";
            this.tsmiSelectHTTP.Size = new System.Drawing.Size(180, 22);
            this.tsmiSelectHTTP.Text = "HTTP";
            this.tsmiSelectHTTP.Click += new System.EventHandler(this.tsmiSelectHTTP_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(146, 271);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 11;
            this.btnSave.Text = "Salvar/Abrir";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.button2_Click);
            // 
            // saveMenu
            // 
            this.saveMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiS4,
            this.tsmiS5,
            this.tsmiHttp,
            this.tsmiAll,
            this.tsmiSaveCsv,
            this.toolStripSeparator1,
            this.tsmiLoadCsv});
            this.saveMenu.Name = "saveMenu";
            this.saveMenu.Size = new System.Drawing.Size(139, 142);
            // 
            // tsmiS4
            // 
            this.tsmiS4.Name = "tsmiS4";
            this.tsmiS4.Size = new System.Drawing.Size(138, 22);
            this.tsmiS4.Text = "Socks4";
            this.tsmiS4.Click += new System.EventHandler(this.tsmiS4_Click);
            // 
            // tsmiS5
            // 
            this.tsmiS5.Name = "tsmiS5";
            this.tsmiS5.Size = new System.Drawing.Size(138, 22);
            this.tsmiS5.Text = "Socks5";
            this.tsmiS5.Click += new System.EventHandler(this.tsmiS5_Click);
            // 
            // tsmiHttp
            // 
            this.tsmiHttp.Name = "tsmiHttp";
            this.tsmiHttp.Size = new System.Drawing.Size(138, 22);
            this.tsmiHttp.Text = "HTTP";
            this.tsmiHttp.Click += new System.EventHandler(this.tsmiHttp_Click);
            // 
            // tsmiAll
            // 
            this.tsmiAll.Name = "tsmiAll";
            this.tsmiAll.Size = new System.Drawing.Size(138, 22);
            this.tsmiAll.Text = "Todas";
            this.tsmiAll.Click += new System.EventHandler(this.tsmiAll_Click);
            // 
            // tsmiSaveCsv
            // 
            this.tsmiSaveCsv.Name = "tsmiSaveCsv";
            this.tsmiSaveCsv.Size = new System.Drawing.Size(138, 22);
            this.tsmiSaveCsv.Text = "Todas (CSV)";
            this.tsmiSaveCsv.Click += new System.EventHandler(this.tsmiSaveCsv_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(135, 6);
            // 
            // tsmiLoadCsv
            // 
            this.tsmiLoadCsv.Name = "tsmiLoadCsv";
            this.tsmiLoadCsv.Size = new System.Drawing.Size(138, 22);
            this.tsmiLoadCsv.Text = "Abrir (CSV)";
            this.tsmiLoadCsv.Click += new System.EventHandler(this.tsmiLoadCsv_Click);
            // 
            // lvProxies
            // 
            this.lvProxies.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ipPort,
            this.type,
            this.country});
            this.lvProxies.ContextMenuStrip = this.lvMenu;
            this.lvProxies.Location = new System.Drawing.Point(12, 12);
            this.lvProxies.Name = "lvProxies";
            this.lvProxies.OwnerDraw = true;
            this.lvProxies.Size = new System.Drawing.Size(390, 251);
            this.lvProxies.TabIndex = 10;
            this.lvProxies.UseCompatibleStateImageBehavior = false;
            this.lvProxies.View = System.Windows.Forms.View.Details;
            this.lvProxies.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lvProxies_KeyDown);
            // 
            // ipPort
            // 
            this.ipPort.Text = "Endereço";
            this.ipPort.Width = 142;
            // 
            // type
            // 
            this.type.Text = "Tipo";
            this.type.Width = 69;
            // 
            // country
            // 
            this.country.Text = "País ";
            this.country.Width = 136;
            // 
            // ProxyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(414, 302);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.lvProxies);
            this.Controls.Add(this.btnChecker);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.label3);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "ProxyForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Lista de proxies";
            this.Load += new System.EventHandler(this.ProxyForm_Load);
            this.lvMenu.ResumeLayout(false);
            this.saveMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnChecker;
        private AdvancedBot.Controls.ProxyListView lvProxies;
        private System.Windows.Forms.ColumnHeader ipPort;
        private System.Windows.Forms.ColumnHeader type;
        private System.Windows.Forms.ColumnHeader country;
        private System.Windows.Forms.ContextMenuStrip lvMenu;
        private System.Windows.Forms.ToolStripMenuItem tsmiAdd;
        private System.Windows.Forms.ToolStripMenuItem tsmiRemove;
        private System.Windows.Forms.ToolStripMenuItem tsmiCopy;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.ContextMenuStrip saveMenu;
        private System.Windows.Forms.ToolStripMenuItem tsmiS4;
        private System.Windows.Forms.ToolStripMenuItem tsmiS5;
        private System.Windows.Forms.ToolStripMenuItem tsmiHttp;
        private System.Windows.Forms.ToolStripMenuItem tsmiAll;
        private System.Windows.Forms.ToolStripMenuItem tsmiSaveCsv;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem tsmiLoadCsv;
        private System.Windows.Forms.ToolStripMenuItem tsmiSelect;
        private System.Windows.Forms.ToolStripMenuItem tsmiSelectS4;
        private System.Windows.Forms.ToolStripMenuItem tsmiSelectS5;
        private System.Windows.Forms.ToolStripMenuItem tsmiSelectHTTP;
    }
}