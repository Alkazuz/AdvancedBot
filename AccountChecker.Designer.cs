namespace AdvancedBot
{
    partial class AccountChecker
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AccountChecker));
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.listView1 = new System.Windows.Forms.ListView();
            this.ACC = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Status = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnStart = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copiarFuncionandoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copiarOfflinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.percentageProgressBar1 = new AdvancedBot.Controls.PercentageProgressBar();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(1, 1);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(217, 282);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "email:senha\nemail:senha\n...";
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ACC,
            this.Status});
            this.listView1.Location = new System.Drawing.Point(224, 1);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(224, 282);
            this.listView1.TabIndex = 1;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // ACC
            // 
            this.ACC.Tag = "Email:Senha";
            this.ACC.Text = "Email:Senha";
            this.ACC.Width = 175;
            // 
            // Status
            // 
            this.Status.Text = "Status";
            this.Status.Width = 45;
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(373, 289);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 2;
            this.btnStart.Text = "Iniciar";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copiarFuncionandoToolStripMenuItem,
            this.copiarOfflinesToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(181, 70);
            // 
            // copiarFuncionandoToolStripMenuItem
            // 
            this.copiarFuncionandoToolStripMenuItem.Name = "copiarFuncionandoToolStripMenuItem";
            this.copiarFuncionandoToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.copiarFuncionandoToolStripMenuItem.Text = "Copiar funcionando";
            this.copiarFuncionandoToolStripMenuItem.Click += new System.EventHandler(this.copiarFuncionandoToolStripMenuItem_Click);
            // 
            // copiarOfflinesToolStripMenuItem
            // 
            this.copiarOfflinesToolStripMenuItem.Name = "copiarOfflinesToolStripMenuItem";
            this.copiarOfflinesToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.copiarOfflinesToolStripMenuItem.Text = "Copiar offlines";
            this.copiarOfflinesToolStripMenuItem.Click += new System.EventHandler(this.copiarOfflinesToolStripMenuItem_Click);
            // 
            // percentageProgressBar1
            // 
            this.percentageProgressBar1.Location = new System.Drawing.Point(224, 289);
            this.percentageProgressBar1.Name = "percentageProgressBar1";
            this.percentageProgressBar1.Size = new System.Drawing.Size(143, 23);
            this.percentageProgressBar1.TabIndex = 4;
            this.percentageProgressBar1.Visible = false;
            // 
            // AccountChecker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(451, 318);
            this.Controls.Add(this.percentageProgressBar1);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.richTextBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "AccountChecker";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Testador de Conta - Mojang";
            this.Load += new System.EventHandler(this.AccountChecker_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader ACC;
        private System.Windows.Forms.ColumnHeader Status;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem copiarFuncionandoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copiarOfflinesToolStripMenuItem;
        private AdvancedBot.Controls.PercentageProgressBar percentageProgressBar1;
    }
}