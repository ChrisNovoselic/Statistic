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
            this.������������������� = new System.Windows.Forms.Button();
            this.lblChoose = new System.Windows.Forms.Label();
            this.clbMode = new System.Windows.Forms.CheckedListBox();
            this.btnSetAll = new System.Windows.Forms.Button();
            this.btnClearAll = new System.Windows.Forms.Button();
            this.checkBoxTEC = new System.Windows.Forms.CheckBox();
            this.checkBoxTG = new System.Windows.Forms.CheckBox();
            this.checkBoxGTP = new System.Windows.Forms.CheckBox();
            this.checkBoxPC = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(5, 336);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(95, 23);
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "�������";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // �������������������
            // 
            this.�������������������.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.�������������������.Location = new System.Drawing.Point(106, 336);
            this.�������������������.Name = "�������������������";
            this.�������������������.Size = new System.Drawing.Size(95, 23);
            this.�������������������.TabIndex = 2;
            this.�������������������.Text = "��������";
            this.�������������������.UseVisualStyleBackColor = true;
            this.�������������������.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lblChoose
            // 
            this.lblChoose.AutoSize = true;
            this.lblChoose.Location = new System.Drawing.Point(1, 2);
            this.lblChoose.Name = "lblChoose";
            this.lblChoose.Size = new System.Drawing.Size(134, 13);
            this.lblChoose.TabIndex = 3;
            this.lblChoose.Text = "�������� ����� ������";
            // 
            // clbMode
            // 
            this.clbMode.CheckOnClick = true;
            this.clbMode.FormattingEnabled = true;
            this.clbMode.Location = new System.Drawing.Point(5, 114);
            this.clbMode.Name = "clbMode";
            this.clbMode.Size = new System.Drawing.Size(197, 214);
            this.clbMode.TabIndex = 4;
            this.clbMode.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(clbMode_ItemCheck); 
            // 
            // btnSetAll
            // 
            this.btnSetAll.Location = new System.Drawing.Point(5, 19);
            this.btnSetAll.Name = "btnSetAll";
            this.btnSetAll.Size = new System.Drawing.Size(95, 23);
            this.btnSetAll.TabIndex = 5;
            this.btnSetAll.Text = "������� ���";
            this.btnSetAll.UseVisualStyleBackColor = true;
            this.btnSetAll.Click += new System.EventHandler(this.btnSetAll_Click);
            // 
            // btnClearAll
            // 
            this.btnClearAll.Location = new System.Drawing.Point(106, 19);
            this.btnClearAll.Name = "btnClearAll";
            this.btnClearAll.Size = new System.Drawing.Size(95, 23);
            this.btnClearAll.TabIndex = 6;
            this.btnClearAll.Text = "����� ���";
            this.btnClearAll.UseVisualStyleBackColor = true;
            this.btnClearAll.Click += new System.EventHandler(this.btnClearAll_Click);
            // 
            // checkBoxTEC
            // 
            this.checkBoxTEC.AutoSize = true;
            this.checkBoxTEC.Location = new System.Drawing.Point(5, 56);
            this.checkBoxTEC.Name = "checkBoxTEC";
            this.checkBoxTEC.Size = new System.Drawing.Size(48, 17);
            this.checkBoxTEC.TabIndex = 7;
            this.checkBoxTEC.Text = "���";
            this.checkBoxTEC.UseVisualStyleBackColor = true;
            this.checkBoxTEC.CheckedChanged += new System.EventHandler(this.checkBox_CheckedChanged);
            // 
            // checkBoxTG
            // 
            this.checkBoxTG.AutoSize = true;
            this.checkBoxTG.Location = new System.Drawing.Point(4, 85);
            this.checkBoxTG.Name = "checkBoxTG";
            this.checkBoxTG.Size = new System.Drawing.Size(39, 17);
            this.checkBoxTG.TabIndex = 8;
            this.checkBoxTG.Text = "��";
            this.checkBoxTG.UseVisualStyleBackColor = true;
            this.checkBoxTG.CheckedChanged += new System.EventHandler(this.checkBox_CheckedChanged);
            // 
            // checkBoxGTP
            // 
            this.checkBoxGTP.AutoSize = true;
            this.checkBoxGTP.Location = new System.Drawing.Point(106, 56);
            this.checkBoxGTP.Name = "checkBoxGTP";
            this.checkBoxGTP.Size = new System.Drawing.Size(47, 17);
            this.checkBoxGTP.TabIndex = 9;
            this.checkBoxGTP.Text = "���";
            this.checkBoxGTP.UseVisualStyleBackColor = true;
            this.checkBoxGTP.CheckedChanged += new System.EventHandler(this.checkBox_CheckedChanged);
            // 
            // checkBoxPC
            // 
            this.checkBoxPC.AutoSize = true;
            this.checkBoxPC.Location = new System.Drawing.Point(106, 85);
            this.checkBoxPC.Name = "checkBoxPC";
            this.checkBoxPC.Size = new System.Drawing.Size(44, 17);
            this.checkBoxPC.TabIndex = 10;
            this.checkBoxPC.Text = "��";
            this.checkBoxPC.UseVisualStyleBackColor = true;
            this.checkBoxPC.CheckedChanged += new System.EventHandler(this.checkBox_CheckedChanged);
            // 
            // FormChangeMode
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.�������������������;
            this.ClientSize = new System.Drawing.Size(206, 365);
            this.Controls.Add(this.checkBoxPC);
            this.Controls.Add(this.checkBoxGTP);
            this.Controls.Add(this.checkBoxTG);
            this.Controls.Add(this.checkBoxTEC);
            this.Controls.Add(this.btnClearAll);
            this.Controls.Add(this.btnSetAll);
            this.Controls.Add(this.clbMode);
            this.Controls.Add(this.lblChoose);
            this.Controls.Add(this.�������������������);
            this.Controls.Add(this.btnOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FormChangeMode";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.HandleCreated += new System.EventHandler(ChangeMode_HandleCreated);
            this.Load += new System.EventHandler(ChangeMode_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ChangeMode_FormClosing);
            this.Shown += new System.EventHandler(this.ChangeMode_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button �������������������;
        private System.Windows.Forms.Label lblChoose;
        private System.Windows.Forms.CheckedListBox clbMode;
        private System.Windows.Forms.Button btnSetAll;
        private System.Windows.Forms.Button btnClearAll;
        private System.Windows.Forms.CheckBox checkBoxTEC;
        private System.Windows.Forms.CheckBox checkBoxTG;
        private System.Windows.Forms.CheckBox checkBoxGTP;
        private System.Windows.Forms.CheckBox checkBoxPC;
    }
}