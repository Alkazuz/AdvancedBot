namespace AdvancedBot
{
    partial class BanCheck
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
            if (disposing && (components != null)) {
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tbSvAddr = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbVersion = new System.Windows.Forms.ComboBox();
            this.cbSendPing = new System.Windows.Forms.CheckBox();
            this.lvResult = new System.Windows.Forms.ListView();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.progress = new AdvancedBot.Controls.PercentageProgressBar();
            this.rtbAccounts = new System.Windows.Forms.RichTextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cbSendPing);
            this.groupBox1.Controls.Add(this.cbVersion);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.tbSvAddr);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(345, 73);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Servidor";
            // 
            // tbSvAddr
            // 
            this.tbSvAddr.Location = new System.Drawing.Point(68, 19);
            this.tbSvAddr.Name = "tbSvAddr";
            this.tbSvAddr.Size = new System.Drawing.Size(188, 20);
            this.tbSvAddr.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Endereço:";
            // 
            // cbVersion
            // 
            this.cbVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbVersion.FormattingEnabled = true;
            this.cbVersion.Location = new System.Drawing.Point(262, 19);
            this.cbVersion.Name = "cbVersion";
            this.cbVersion.Size = new System.Drawing.Size(76, 21);
            this.cbVersion.TabIndex = 2;
            // 
            // cbSendPing
            // 
            this.cbSendPing.AutoSize = true;
            this.cbSendPing.Location = new System.Drawing.Point(9, 45);
            this.cbSendPing.Name = "cbSendPing";
            this.cbSendPing.Size = new System.Drawing.Size(79, 17);
            this.cbSendPing.TabIndex = 3;
            this.cbSendPing.Text = "Enviar ping";
            this.cbSendPing.UseVisualStyleBackColor = true;
            // 
            // lvResult
            // 
            this.lvResult.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.lvResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvResult.Location = new System.Drawing.Point(3, 16);
            this.lvResult.Name = "lvResult";
            this.lvResult.Size = new System.Drawing.Size(339, 174);
            this.lvResult.TabIndex = 1;
            this.lvResult.UseCompatibleStateImageBehavior = false;
            this.lvResult.View = System.Windows.Forms.View.Details;
            this.lvResult.Visible = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.rtbAccounts);
            this.groupBox2.Controls.Add(this.lvResult);
            this.groupBox2.Location = new System.Drawing.Point(12, 91);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(345, 193);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Contas";
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(282, 287);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 3;
            this.btnStart.Text = "OK";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Conta";
            this.columnHeader1.Width = 130;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Resultado";
            this.columnHeader2.Width = 200;
            // 
            // progress
            // 
            this.progress.Location = new System.Drawing.Point(15, 287);
            this.progress.Name = "progress";
            this.progress.Size = new System.Drawing.Size(261, 23);
            this.progress.TabIndex = 4;
            // 
            // rtbAccounts
            // 
            this.rtbAccounts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbAccounts.Location = new System.Drawing.Point(3, 16);
            this.rtbAccounts.Name = "rtbAccounts";
            this.rtbAccounts.Size = new System.Drawing.Size(339, 174);
            this.rtbAccounts.TabIndex = 2;
            this.rtbAccounts.Text = "Nick1\nNick2\n...";
            this.rtbAccounts.Enter += new System.EventHandler(this.rtbAccounts_Enter);
            // 
            // BanCheck
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(369, 317);
            this.Controls.Add(this.progress);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "BanCheck";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Checador de ban";
            this.Load += new System.EventHandler(this.BanCheck_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox tbSvAddr;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbVersion;
        private System.Windows.Forms.CheckBox cbSendPing;
        private System.Windows.Forms.ListView lvResult;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnStart;
        private AdvancedBot.Controls.PercentageProgressBar progress;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.RichTextBox rtbAccounts;
    }
}