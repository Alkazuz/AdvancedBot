namespace AdvancedBot
{
    partial class FilterCountryForm
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
            this.btnOK = new System.Windows.Forms.Button();
            this.lbCountries = new AdvancedBot.Controls.CountryListBox();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(304, 218);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lbCountries
            // 
            this.lbCountries.AutoScroll = true;
            this.lbCountries.AutoScrollMinSize = new System.Drawing.Size(0, 16);
            this.lbCountries.BackColor = System.Drawing.Color.White;
            this.lbCountries.Location = new System.Drawing.Point(12, 12);
            this.lbCountries.Name = "lbCountries";
            this.lbCountries.Size = new System.Drawing.Size(367, 200);
            this.lbCountries.TabIndex = 0;
            this.lbCountries.Text = "countryListBox1";
            // 
            // FilterCountryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(391, 253);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lbCountries);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FilterCountryForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Filtrar países";
            this.Load += new System.EventHandler(this.FilterCountryForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        public AdvancedBot.Controls.CountryListBox lbCountries;
        private System.Windows.Forms.Button btnOK;
    }
}