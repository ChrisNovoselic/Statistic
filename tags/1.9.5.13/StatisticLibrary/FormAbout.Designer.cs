namespace StatisticCommon
{
    partial class FormAbout
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormAbout));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.llblMailTo = new System.Windows.Forms.LinkLabel();
            this.btnClose = new System.Windows.Forms.Button();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.m_lblProductVersion = new System.Windows.Forms.Label();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.m_lblDomainMashineUserName = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(12, 9);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(48, 48);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(66, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(264, 69);
            this.label1.TabIndex = 2;
            this.label1.Text = "��������: ��� \"�����������������\", ����� IT.\r\n\r\n�����������: ������� ������, e-ma" +
    "il: \r\n�������������: ������� �.�., e-mail:\r\n�������������: ������ �.�., e-mail:";
            // 
            // llblMailTo
            // 
            this.llblMailTo.AutoSize = true;
            this.llblMailTo.Location = new System.Drawing.Point(277, 68);
            this.llblMailTo.Name = "llblMailTo";
            this.llblMailTo.Size = new System.Drawing.Size(106, 13);
            this.llblMailTo.TabIndex = 1;
            this.llblMailTo.TabStop = true;
            this.llblMailTo.Text = "small@groundzero.ru";
            this.llblMailTo.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llblMailTo_LinkClicked);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(162, 115);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "�������";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(277, 81);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(81, 13);
            this.linkLabel1.TabIndex = 3;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "real@tps-nsk.ru";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llblMailTo_LinkClicked);
            // 
            // m_lblProductVersion
            // 
            this.m_lblProductVersion.AutoSize = true;
            this.m_lblProductVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.m_lblProductVersion.Location = new System.Drawing.Point(66, 8);
            this.m_lblProductVersion.Name = "m_lblProductVersion";
            this.m_lblProductVersion.Size = new System.Drawing.Size(97, 13);
            this.m_lblProductVersion.TabIndex = 4;
            this.m_lblProductVersion.Text = "Product-Version";
            // 
            // linkLabel2
            // 
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.Location = new System.Drawing.Point(277, 95);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(98, 13);
            this.linkLabel2.TabIndex = 5;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "ChrjapinAN@itss.ru";
            // 
            // m_lblDomainMashineUserName
            // 
            this.m_lblDomainMashineUserName.AutoSize = true;
            this.m_lblDomainMashineUserName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.m_lblDomainMashineUserName.ForeColor = System.Drawing.Color.Red;
            this.m_lblDomainMashineUserName.Location = new System.Drawing.Point(66, 24);
            this.m_lblDomainMashineUserName.Name = "m_lblDomainMashineUserName";
            this.m_lblDomainMashineUserName.Size = new System.Drawing.Size(154, 13);
            this.m_lblDomainMashineUserName.TabIndex = 6;
            this.m_lblDomainMashineUserName.Text = "DomainMashineUserName";
            // 
            // FormAbout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(393, 142);
            this.Controls.Add(this.m_lblDomainMashineUserName);
            this.Controls.Add(this.linkLabel2);
            this.Controls.Add(this.m_lblProductVersion);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.llblMailTo);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FormAbout";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "� ���������";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel llblMailTo;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label m_lblProductVersion;
        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.Label m_lblDomainMashineUserName;
    }
}