namespace AdvancedBot
{
    partial class MinerOptions
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
            this.btnDownPriority = new System.Windows.Forms.Button();
            this.btnUpPriority = new System.Windows.Forms.Button();
            this.btnDelSel = new System.Windows.Forms.Button();
            this.nudPriority = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.cbBlock = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnAddBlock = new System.Windows.Forms.Button();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnOK = new System.Windows.Forms.Button();
            this.cbStopInvFull = new System.Windows.Forms.CheckBox();
            this.tbCmds = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.cbExec = new System.Windows.Forms.CheckBox();
            this.nudMinerRadius = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.cbAutoTool = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPriority)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinerRadius)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnDownPriority);
            this.groupBox1.Controls.Add(this.btnUpPriority);
            this.groupBox1.Controls.Add(this.btnDelSel);
            this.groupBox1.Controls.Add(this.nudPriority);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.cbBlock);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.btnAddBlock);
            this.groupBox1.Controls.Add(this.listView1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(359, 182);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Minérios";
            // 
            // btnDownPriority
            // 
            this.btnDownPriority.Location = new System.Drawing.Point(329, 83);
            this.btnDownPriority.Name = "btnDownPriority";
            this.btnDownPriority.Size = new System.Drawing.Size(25, 23);
            this.btnDownPriority.TabIndex = 8;
            this.btnDownPriority.Text = "\\/";
            this.btnDownPriority.UseVisualStyleBackColor = true;
            this.btnDownPriority.Click += new System.EventHandler(this.btnDownPriority_Click);
            // 
            // btnUpPriority
            // 
            this.btnUpPriority.Location = new System.Drawing.Point(329, 60);
            this.btnUpPriority.Name = "btnUpPriority";
            this.btnUpPriority.Size = new System.Drawing.Size(25, 23);
            this.btnUpPriority.TabIndex = 7;
            this.btnUpPriority.Text = "/\\";
            this.btnUpPriority.UseVisualStyleBackColor = true;
            this.btnUpPriority.Click += new System.EventHandler(this.btnUpPriority_Click);
            // 
            // btnDelSel
            // 
            this.btnDelSel.Location = new System.Drawing.Point(317, 152);
            this.btnDelSel.Name = "btnDelSel";
            this.btnDelSel.Size = new System.Drawing.Size(36, 23);
            this.btnDelSel.TabIndex = 6;
            this.btnDelSel.Text = "Del";
            this.btnDelSel.UseVisualStyleBackColor = true;
            this.btnDelSel.Click += new System.EventHandler(this.btnDelSel_Click);
            // 
            // nudPriority
            // 
            this.nudPriority.Location = new System.Drawing.Point(212, 154);
            this.nudPriority.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.nudPriority.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudPriority.Name = "nudPriority";
            this.nudPriority.Size = new System.Drawing.Size(57, 20);
            this.nudPriority.TabIndex = 5;
            this.nudPriority.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(195, 157);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "#:";
            // 
            // cbBlock
            // 
            this.cbBlock.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cbBlock.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cbBlock.FormattingEnabled = true;
            this.cbBlock.Location = new System.Drawing.Point(43, 154);
            this.cbBlock.Name = "cbBlock";
            this.cbBlock.Size = new System.Drawing.Size(146, 21);
            this.cbBlock.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 157);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Bloco:";
            // 
            // btnAddBlock
            // 
            this.btnAddBlock.Location = new System.Drawing.Point(279, 152);
            this.btnAddBlock.Name = "btnAddBlock";
            this.btnAddBlock.Size = new System.Drawing.Size(36, 23);
            this.btnAddBlock.TabIndex = 1;
            this.btnAddBlock.Text = "Add";
            this.btnAddBlock.UseVisualStyleBackColor = true;
            this.btnAddBlock.Click += new System.EventHandler(this.btnAddBlock_Click);
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listView1.FullRowSelect = true;
            this.listView1.LabelEdit = true;
            this.listView1.Location = new System.Drawing.Point(6, 19);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(322, 129);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.listView1_AfterLabelEdit);
            this.listView1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseDown);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Prioridade";
            this.columnHeader1.Width = 65;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Bloco";
            this.columnHeader2.Width = 218;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(318, 381);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(53, 23);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "Salvar";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // cbStopInvFull
            // 
            this.cbStopInvFull.AutoSize = true;
            this.cbStopInvFull.Location = new System.Drawing.Point(12, 19);
            this.cbStopInvFull.Name = "cbStopInvFull";
            this.cbStopInvFull.Size = new System.Drawing.Size(51, 17);
            this.cbStopInvFull.TabIndex = 5;
            this.cbStopInvFull.Text = "Parar";
            this.cbStopInvFull.UseVisualStyleBackColor = true;
            // 
            // tbCmds
            // 
            this.tbCmds.Location = new System.Drawing.Point(69, 19);
            this.tbCmds.Multiline = true;
            this.tbCmds.Name = "tbCmds";
            this.tbCmds.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbCmds.Size = new System.Drawing.Size(284, 76);
            this.tbCmds.TabIndex = 6;
            this.tbCmds.Text = "/home\r\nwait(1500)\r\n$dropall 36,37\r\nwait(8000)\r\n/warp mina\r\n$miner";
            this.tbCmds.TextChanged += new System.EventHandler(this.tbCmds_TextChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.cbExec);
            this.groupBox2.Controls.Add(this.cbStopInvFull);
            this.groupBox2.Controls.Add(this.tbCmds);
            this.groupBox2.Location = new System.Drawing.Point(12, 200);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(359, 101);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Quando o inventário estiver cheio:";
            // 
            // cbExec
            // 
            this.cbExec.AutoSize = true;
            this.cbExec.Location = new System.Drawing.Point(12, 42);
            this.cbExec.Name = "cbExec";
            this.cbExec.Size = new System.Drawing.Size(53, 17);
            this.cbExec.TabIndex = 7;
            this.cbExec.Text = "Exec:";
            this.cbExec.UseVisualStyleBackColor = true;
            // 
            // nudMinerRadius
            // 
            this.nudMinerRadius.Location = new System.Drawing.Point(98, 16);
            this.nudMinerRadius.Maximum = new decimal(new int[] {
            48,
            0,
            0,
            0});
            this.nudMinerRadius.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.nudMinerRadius.Name = "nudMinerRadius";
            this.nudMinerRadius.Size = new System.Drawing.Size(53, 20);
            this.nudMinerRadius.TabIndex = 9;
            this.nudMinerRadius.Value = new decimal(new int[] {
            8,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 18);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(86, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Raio de procura:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(154, 19);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "blocos";
            // 
            // cbAutoTool
            // 
            this.cbAutoTool.AutoSize = true;
            this.cbAutoTool.Location = new System.Drawing.Point(6, 39);
            this.cbAutoTool.Name = "cbAutoTool";
            this.cbAutoTool.Size = new System.Drawing.Size(294, 17);
            this.cbAutoTool.TabIndex = 12;
            this.cbAutoTool.Text = "Selecionar o melhor item para quebrar o bloco (AutoTool)";
            this.cbAutoTool.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.cbAutoTool);
            this.groupBox3.Controls.Add(this.nudMinerRadius);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Location = new System.Drawing.Point(12, 307);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(359, 68);
            this.groupBox3.TabIndex = 13;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Opções";
            // 
            // MinerOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(383, 414);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MinerOptions";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Opções do minerador";
            this.Load += new System.EventHandler(this.MinerOptions_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPriority)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinerRadius)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ComboBox cbBlock;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnAddBlock;
        private System.Windows.Forms.NumericUpDown nudPriority;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnDelSel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.CheckBox cbStopInvFull;
        private System.Windows.Forms.TextBox tbCmds;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox cbExec;
        private System.Windows.Forms.Button btnDownPriority;
        private System.Windows.Forms.Button btnUpPriority;
        private System.Windows.Forms.NumericUpDown nudMinerRadius;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox cbAutoTool;
        private System.Windows.Forms.GroupBox groupBox3;
    }
}