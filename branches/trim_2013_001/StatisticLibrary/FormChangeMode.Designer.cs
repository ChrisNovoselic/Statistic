namespace StatisticCommon
{
    partial class FormChangeMode
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
            this.параметрыПриложения = new System.Windows.Forms.Button();
            this.lblChoose = new System.Windows.Forms.Label();
            this.clbMode = new System.Windows.Forms.CheckedListBox();
            this.btnSetAll = new System.Windows.Forms.Button();
            this.btnClearAll = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(5, 286);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(95, 23);
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "Выбрать";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // параметрыПриложения
            // 
            this.параметрыПриложения.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.параметрыПриложения.Location = new System.Drawing.Point(106, 286);
            this.параметрыПриложения.Name = "параметрыПриложения";
            this.параметрыПриложения.Size = new System.Drawing.Size(95, 23);
            this.параметрыПриложения.TabIndex = 2;
            this.параметрыПриложения.Text = "Отменить";
            this.параметрыПриложения.UseVisualStyleBackColor = true;
            this.параметрыПриложения.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lblChoose
            // 
            this.lblChoose.AutoSize = true;
            this.lblChoose.Location = new System.Drawing.Point(1, 2);
            this.lblChoose.Name = "lblChoose";
            this.lblChoose.Size = new System.Drawing.Size(134, 13);
            this.lblChoose.TabIndex = 3;
            this.lblChoose.Text = "Выберите режим работы";
            // 
            // clbMode
            // 
            this.clbMode.CheckOnClick = true;
            this.clbMode.FormattingEnabled = true;
            this.clbMode.Location = new System.Drawing.Point(4, 57);
            this.clbMode.Name = "clbMode";
            this.clbMode.Size = new System.Drawing.Size(197, 214);
            this.clbMode.TabIndex = 4;
            // 
            // btnSetAll
            // 
            this.btnSetAll.Location = new System.Drawing.Point(5, 19);
            this.btnSetAll.Name = "btnSetAll";
            this.btnSetAll.Size = new System.Drawing.Size(95, 23);
            this.btnSetAll.TabIndex = 5;
            this.btnSetAll.Text = "Выбрать все";
            this.btnSetAll.UseVisualStyleBackColor = true;
            this.btnSetAll.Click += new System.EventHandler(this.btnSetAll_Click);
            // 
            // btnClearAll
            // 
            this.btnClearAll.Location = new System.Drawing.Point(106, 19);
            this.btnClearAll.Name = "btnClearAll";
            this.btnClearAll.Size = new System.Drawing.Size(95, 23);
            this.btnClearAll.TabIndex = 6;
            this.btnClearAll.Text = "Снять все";
            this.btnClearAll.UseVisualStyleBackColor = true;
            this.btnClearAll.Click += new System.EventHandler(this.btnClearAll_Click);
            // 
            // FormChangeMode
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.параметрыПриложения;
            this.ClientSize = new System.Drawing.Size(206, 320);
            this.Controls.Add(this.btnClearAll);
            this.Controls.Add(this.btnSetAll);
            this.Controls.Add(this.clbMode);
            this.Controls.Add(this.lblChoose);
            this.Controls.Add(this.параметрыПриложения);
            this.Controls.Add(this.btnOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FormChangeMode";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ChangeMode_FormClosing);
            this.Shown += new System.EventHandler(this.ChangeMode_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button параметрыПриложения;
        private System.Windows.Forms.Label lblChoose;
        private System.Windows.Forms.CheckedListBox clbMode;
        private System.Windows.Forms.Button btnSetAll;
        private System.Windows.Forms.Button btnClearAll;
    }
}