namespace Statistic
{
    partial class FormPasswordBase
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
            this.btnOk = new System.Windows.Forms.Button();
            this.������������������� = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            //this.btnOk.Location = new System.Drawing.Point(68, 49);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "��";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.bntOk_Click);
            // 
            // �������������������
            //
            this.�������������������.DialogResult = System.Windows.Forms.DialogResult.Cancel; 
            //this.�������������������.Location = new System.Drawing.Point(149, 49);
            this.�������������������.Name = "�������������������";
            this.�������������������.Size = new System.Drawing.Size(75, 23);
            this.�������������������.TabIndex = 2;
            this.�������������������.Text = "������";
            this.�������������������.UseVisualStyleBackColor = true;
            this.�������������������.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // FormPassword
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            //this.ClientSize = new System.Drawing.Size(292, 77);
            this.Controls.Add(this.�������������������);
            this.Controls.Add(this.btnOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FormPasswordBase";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormPasswordBase_FormClosing);
            this.Shown += new System.EventHandler(this.FormPasswordBase_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        protected System.Windows.Forms.Button btnOk;
        protected System.Windows.Forms.Button �������������������;
    }
}