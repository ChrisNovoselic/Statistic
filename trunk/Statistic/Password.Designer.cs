namespace Statistic
{
    partial class Password
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
            this.tbxPassword = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.параметрыПриложения = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tbxPassword
            // 
            this.tbxPassword.Location = new System.Drawing.Point(3, 23);
            this.tbxPassword.MaxLength = 20;
            this.tbxPassword.Name = "tbxPassword";
            this.tbxPassword.PasswordChar = '#';
            this.tbxPassword.Size = new System.Drawing.Size(286, 20);
            this.tbxPassword.TabIndex = 0;
            this.tbxPassword.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbxPassword_KeyDown);
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(30, 3);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(232, 13);
            this.lblPassword.TabIndex = 3;
            this.lblPassword.Text = "Введите пароль коммерческого диспетчера";
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(68, 49);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "Ок";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.bntOk_Click);
            // 
            // параметрыПриложения
            // 
            this.параметрыПриложения.Location = new System.Drawing.Point(149, 49);
            this.параметрыПриложения.Name = "параметрыПриложения";
            this.параметрыПриложения.Size = new System.Drawing.Size(75, 23);
            this.параметрыПриложения.TabIndex = 2;
            this.параметрыПриложения.Text = "Отмена";
            this.параметрыПриложения.UseVisualStyleBackColor = true;
            this.параметрыПриложения.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // Password
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 77);
            this.Controls.Add(this.параметрыПриложения);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.tbxPassword);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Password";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Shown += new System.EventHandler(this.Password_Shown);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Password_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbxPassword;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button параметрыПриложения;
    }
}