namespace AdvancedBot.Forms
{
    partial class LoginSettings
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
            this.txtLogin = new System.Windows.Forms.TextBox();
            this.lblRegister = new System.Windows.Forms.Label();
            this.txtRegister = new System.Windows.Forms.TextBox();
            this.lblLogin = new System.Windows.Forms.Label();
            this.btnSendInput = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // txtLogin
            // 
            this.txtLogin.Location = new System.Drawing.Point(163, 14);
            this.txtLogin.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.txtLogin.Name = "txtLogin";
            this.txtLogin.Size = new System.Drawing.Size(174, 22);
            this.txtLogin.TabIndex = 22;
            this.txtLogin.Text = "/login @pass";
            this.toolTip1.SetToolTip(this.txtLogin, "Comando para registrar,\r\nuse @senha para usar a senha insirida nas contas\r\n");
            // 
            // lblRegister
            // 
            this.lblRegister.AutoSize = true;
            this.lblRegister.Location = new System.Drawing.Point(19, 43);
            this.lblRegister.Name = "lblRegister";
            this.lblRegister.Size = new System.Drawing.Size(121, 16);
            this.lblRegister.TabIndex = 21;
            this.lblRegister.Text = "Register command:";
            // 
            // txtRegister
            // 
            this.txtRegister.Location = new System.Drawing.Point(163, 40);
            this.txtRegister.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.txtRegister.Name = "txtRegister";
            this.txtRegister.Size = new System.Drawing.Size(174, 22);
            this.txtRegister.TabIndex = 20;
            this.txtRegister.Text = "/register @pass @pass";
            this.toolTip1.SetToolTip(this.txtRegister, "Comando para registrar,\r\nuse @senha para usar a senha insirida nas contas\r\nuse @e" +
        "mail para usar um email aleatorio");
            // 
            // lblLogin
            // 
            this.lblLogin.AutoSize = true;
            this.lblLogin.Location = new System.Drawing.Point(19, 17);
            this.lblLogin.Name = "lblLogin";
            this.lblLogin.Size = new System.Drawing.Size(104, 16);
            this.lblLogin.TabIndex = 19;
            this.lblLogin.Text = "Login command:";
            // 
            // btnSendInput
            // 
            this.btnSendInput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSendInput.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnSendInput.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnSendInput.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnSendInput.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSendInput.Font = new System.Drawing.Font("Arial", 9F);
            this.btnSendInput.Location = new System.Drawing.Point(233, 68);
            this.btnSendInput.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSendInput.Name = "btnSendInput";
            this.btnSendInput.Size = new System.Drawing.Size(104, 24);
            this.btnSendInput.TabIndex = 23;
            this.btnSendInput.Text = "Done";
            this.btnSendInput.UseVisualStyleBackColor = true;
            this.btnSendInput.Click += new System.EventHandler(this.btnSendInput_Click);
            // 
            // LoginSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(351, 98);
            this.Controls.Add(this.btnSendInput);
            this.Controls.Add(this.txtLogin);
            this.Controls.Add(this.lblRegister);
            this.Controls.Add(this.txtRegister);
            this.Controls.Add(this.lblLogin);
            this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LoginSettings";
            this.Padding = new System.Windows.Forms.Padding(23, 74, 23, 25);
            this.Text = "Login and Register";
            this.Load += new System.EventHandler(this.LoginSettings_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtLogin;
        private System.Windows.Forms.Label lblRegister;
        private System.Windows.Forms.TextBox txtRegister;
        private System.Windows.Forms.Label lblLogin;
        private System.Windows.Forms.Button btnSendInput;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}