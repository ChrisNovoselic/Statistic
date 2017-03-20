using System.Drawing;
/// <summary>
/// ������������ ���� Statistic 
/// </summary>
namespace Statistic
{
    /// <summary>
    /// ��������� ����� FormGraphicsSettings (��������� ��������) 
    /// </summary>
    partial class FormGraphicsSettings
    {

        /// <summary>
        /// ��������� LABEL_COLOR
        /// </summary>
        struct LABEL_COLOR 
        {
     
            /// <summary>
            ///  ���� ���������
            /// </summary>
            public Color color;
            public string name, text;
            public System.Drawing.Point pos;

            /// <summary>
            ///  ����������������� ����������� LABEL_COLOR �������������� ���� ���������
            /// </summary>
            /// <param name="col">����</param>
            /// <param name="name">���</param>
            /// <param name="text">�������</param>
            /// <param name="pt">�������</param>
            public LABEL_COLOR(Color col, string name, string text, System.Drawing.Point pt)
            {
                this.color = col;  this.name = name; this.text = text; this.pos = pt;
            }
        }

        /// <summary>                                                        //������������� ������������ ��� ��� �������� ����� Designer.cs. 
        /// Required designer variable.                                      //components ����������� ��� �������� ����������, ���������� � �����.   
        /// </summary>                                                       //��� � ����� ������������ �����������, ��� ��� ���������� ����� �������, ����� ����� ����� �������. 
        private System.ComponentModel.IContainer components = null;          //���� �� �� �������� ����� ���������� � ����� �� ����� ����������, ���������� ����� ��������.         
                                                                                      
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
        /// 
        /// ����� InitializeComponent() �������������� ��� ����������,������������� �� �����.
        /// InitializeComponent() �������� ����� LoadComponent().
        /// LoadComponent() ��������� ���������������� XAML �� ������ � ���������� ��� ��� ���������� ����������������� ����������.
        /// </summary>

        private void InitializeComponent()                                                           
        {
            // �������� CheckBox (������)
            this.cbxScale = new System.Windows.Forms.CheckBox();
            // �������� ������� �������  (����,���������� � �.�.)
            this.m_arlblColor = new System.Windows.Forms.Label [(int)INDEX_COLOR.COUNT_INDEX_COLOR];
            // �������� GroupBox (����: ��� ��������)         
            this.gbxType = new System.Windows.Forms.GroupBox();
            // �������� RadioButton (�������������:�����������)      
            this.rbtnBar = new System.Windows.Forms.RadioButton();
            // �������� RadioButton (�������������:��������)
            this.rbtnLine = new System.Windows.Forms.RadioButton();
            // �������� GroupBox (����: ���� �������� ��������)      
            this.groupBoxSourceData = new System.Windows.Forms.GroupBox();
            // �������� ������� �������������� "���� �������� ��������"
            this.m_arRadioButtonSourceData = new System.Windows.Forms.RadioButton []
            {    
                new System.Windows.Forms.RadioButton()
                , new System.Windows.Forms.RadioButton()
                , new System.Windows.Forms.RadioButton()
                , new System.Windows.Forms.RadioButton()
                , new System.Windows.Forms.RadioButton()
            };
            // ����� SuspendLayout() ���������������� ���������� � ������ "��� ��������" � "���� �������� ��������"
            this.gbxType.SuspendLayout();
            this.groupBoxSourceData.SuspendLayout();
            // SuspendLayout ������������� ������ ��������� ������������ (layout logic)
            this.SuspendLayout();

            // 
            // cbxScale ������� "��������������� ��������"
            // 
            // ������ �������� ������������� ��������� � ������������ � �������� ��� �����������
            this.cbxScale.AutoSize = true;
            //���������� ������ �������� ���� �������� ������������ ������ �������� ���� ��� ���������� 
            this.cbxScale.Location = new System.Drawing.Point(222, 13);
            // ��� ��������
            this.cbxScale.Name = "cbxScale";
            // ������ ��������
            this.cbxScale.Size = new System.Drawing.Size(159, 17);
            // ������������������ �������� ����� �������� ��� ������� �� ������ Tab
            this.cbxScale.TabIndex = 0;
            // ������� ��������
            this.cbxScale.Text = "�������������� �������";
            // ��������� ���� � ������� ���������� ������
            this.cbxScale.UseVisualStyleBackColor = true;
            // ��������� ������� ������ ������������ �������
            this.cbxScale.CheckedChanged += new System.EventHandler(this.cbxScale_CheckedChanged);

            // ������ �������, ��������� �� 10 ��������� (����,���������� � �.�.)
            LABEL_COLOR[] arLabelColor = new LABEL_COLOR [(int)INDEX_COLOR.COUNT_INDEX_COLOR] 
            {
                // LABEL1: ���� ������,��� "lblUDGcolor",������� "����, ����",���������� ��������� (12, 11)
                  new LABEL_COLOR (Color.FromArgb(0, 0, 0), "lblUDGcolor", "����, ����", new System.Drawing.Point(12, 11))
                , new LABEL_COLOR (Color.FromArgb(255, 0, 0), "lblDIVcolor", "����������", new System.Drawing.Point(12, 36))
                , new LABEL_COLOR (Color.FromArgb(0, 128, 0), "lblP_ASKUEcolor", "�������� (��������)", new System.Drawing.Point(12, 61))
                , new LABEL_COLOR (Color.FromArgb(255, 255, 255), "lblP_ASKUE_normHourscolor", "�������� (��������, �����.�.)", new System.Drawing.Point(12, 86))
                , new LABEL_COLOR (Color.FromArgb(0, 128, 192), "lblP_SOTIASSOcolor", "�������� (��������)", new System.Drawing.Point(12, 111))
                , new LABEL_COLOR (Color.FromArgb(255, 255, 0), "lblRECcolor", "������������", new System.Drawing.Point(12, 136))
                , new LABEL_COLOR (Color.FromArgb(128, 000, 128), "lblT_ASKUTEcolor", "����������� (��������)", new System.Drawing.Point(12, 161))
                , new LABEL_COLOR (Color.FromArgb(231, 231, 238 /*230, 230, 230*/), "lblBG_ASKUE_color", "��� (��������)", new System.Drawing.Point(12, 186))
                , new LABEL_COLOR (Color.FromArgb(231, 238, 231), "lblBG_SOTIASSO_color", "��� (��������)", new System.Drawing.Point(12, 211))                
                , new LABEL_COLOR (Color.FromArgb(200, 200, 200), "lblGRIDcolor", "�����", new System.Drawing.Point(12, 236))
            };

            // ��� ������� ��������� (���,���������� � �.�.)
            for (int i = 0; i < (int)INDEX_COLOR.COUNT_INDEX_COLOR; i++)
            {
                // C������ ����� 
                this.m_arlblColor[i] = new System.Windows.Forms.Label();
                // ���� ������� ����� (������)
                this.m_arlblColor[i].BackColor = arLabelColor[i].color;
                // ���� ��������� ����� (�������)
                this.m_arlblColor[i].ForeColor = getForeColor(arLabelColor[i].color);
                // ����� ����� 
                this.m_arlblColor[i].BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                // ���������
                this.m_arlblColor[i].Location = arLabelColor[i].pos;
                // ���
                this.m_arlblColor[i].Name = arLabelColor[i].name;
                // ������
                this.m_arlblColor[i].Size = new System.Drawing.Size(195, 26);
                // ���� �������
                this.m_arlblColor[i].Text = arLabelColor [i].text;
                // ������������ ������
                this.m_arlblColor[i].TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                // ��� CLick �� ����� ����������� ������� ���������� ������� ������� 
                this.m_arlblColor[i].Click += new System.EventHandler(this.lbl_color_Click);
            }
            // 
            // gbxType ������� "��� ��������"
            // 
            // ���������� � ����  (���������) ��������� "�����������" � "��������"
            this.gbxType.Controls.Add(this.rbtnBar);
            this.gbxType.Controls.Add(this.rbtnLine);
            // ���������� ������������ �����
            this.gbxType.Location = new System.Drawing.Point(222, 32);
            // ��� �����
            this.gbxType.Name = "gbxType";
            // ������ 
            this.gbxType.Size = new System.Drawing.Size(173, 58);
            // ������������������ �������� ����� �������� ��� ������� �� ������ Tab
            this.gbxType.TabIndex = 5;
            // gbxType �� �������� ����� � ������� ������� TAB. 
            this.gbxType.TabStop = false;
            // ����� �������
            this.gbxType.Text = "��� ��������";
            // 
            // rbtnBar ������� "�����������"
            // 
            this.rbtnBar.AutoSize = true;
            // �������� �������� �������
            this.rbtnBar.Checked = true;
            this.rbtnBar.Location = new System.Drawing.Point(6, 16);
            this.rbtnBar.Name = "rbtnBar";
            this.rbtnBar.Size = new System.Drawing.Size(92, 17);
            this.rbtnBar.TabIndex = 1;
            this.rbtnBar.TabStop = true;
            this.rbtnBar.Text = "�����������";
            this.rbtnBar.UseVisualStyleBackColor = true;
            // 
            // rbtnLine ������� "��������"
            // 
            this.rbtnLine.AutoSize = true;
            this.rbtnLine.Location = new System.Drawing.Point(6, 37);
            this.rbtnLine.Name = "rbtnLine";
            this.rbtnLine.Size = new System.Drawing.Size(75, 17);
            this.rbtnLine.TabIndex = 0;
            this.rbtnLine.Text = "��������";
            this.rbtnLine.UseVisualStyleBackColor = true;
            // ��������� ������� ������ ������������ �������
            this.rbtnLine.CheckedChanged += new System.EventHandler(this.rbtnLine_CheckedChanged);
            // 
            // groupBoxSourceData ������� "���� �������� ��������"   
            // 
            // ���������� ������� ������ � ���� (���������)
            this.groupBoxSourceData.Controls.AddRange(m_arRadioButtonSourceData);
            this.groupBoxSourceData.Location = new System.Drawing.Point(222, 96);
            this.groupBoxSourceData.Name = "groupBoxSourceData";
            this.groupBoxSourceData.Size = new System.Drawing.Size(173, 114);
            this.groupBoxSourceData.TabIndex = 8;
            this.groupBoxSourceData.TabStop = false;
            this.groupBoxSourceData.Text = "���� �������� ��������";
            
            int indx = -1  
                //������� �� ��� �������
                , yPos = -1
                //���������� ����� ���������� ����������
                , yMargin = 19;
            // 
            // rbtnSourceData_AISKUE_PLUS_SOTIASSO
            // 
            // ���������� indx �������� 0, yPos �������� 16;
            indx = (int)CONN_SETT_TYPE.AISKUE_PLUS_SOTIASSO; yPos = 16;
            // ������ ��������� ���������� RadioButton �� ����� ����������� ��� ����������������� ������,
            // � �������� Checked ������ ���� ��������� � ����
            this.m_arRadioButtonSourceData[(int)indx].AutoCheck = false;
            this.m_arRadioButtonSourceData[(int)indx].AutoSize = true;
            this.m_arRadioButtonSourceData[(int)indx].Location = new System.Drawing.Point(6, yPos);
            this.m_arRadioButtonSourceData[(int)indx].Name = "rbtnSourceData_ASKUE_PLUS_SOTIASSO";
            this.m_arRadioButtonSourceData[(int)indx].Size = new System.Drawing.Size(134, 17);
            this.m_arRadioButtonSourceData[(int)indx].TabIndex = 3;
            this.m_arRadioButtonSourceData[(int)indx].Text = "������+��������";
            this.m_arRadioButtonSourceData[(int)indx].UseVisualStyleBackColor = true;
            this.m_arRadioButtonSourceData[(int)indx].Click += new System.EventHandler(this.rbtnSourceData_ASKUEPLUSSOTIASSO_Click);
            // 
            // rbtnSourceData_ASKUE
            // ���������� indx ��������� 1, yPos ��������� 16+19=35;
            indx = (int)CONN_SETT_TYPE.AISKUE_3_MIN; yPos += yMargin;
            this.m_arRadioButtonSourceData[(int)indx].AutoCheck = false;
            this.m_arRadioButtonSourceData[(int)indx].AutoSize = true;
            this.m_arRadioButtonSourceData[(int)indx].Checked = true;
            this.m_arRadioButtonSourceData[(int)indx].Location = new System.Drawing.Point(6, yPos);
            this.m_arRadioButtonSourceData[(int)indx].Name = "rbtnSourceData_ASKUE";
            this.m_arRadioButtonSourceData[(int)indx].Size = new System.Drawing.Size(69, 17);
            this.m_arRadioButtonSourceData[(int)indx].TabIndex = 1;
            this.m_arRadioButtonSourceData[(int)indx].Text = "������";
            this.m_arRadioButtonSourceData[(int)indx].UseVisualStyleBackColor = true;
            this.m_arRadioButtonSourceData[(int)indx].Click += new System.EventHandler(this.rbtnSourceData_ASKUE_Click);
            // 
            // rbtnSourceData_SOTIASSO_3_min
            // ���������� indx ��������� 2, yPos ��������� 35+19=54;
            indx = (int)CONN_SETT_TYPE.SOTIASSO_3_MIN; yPos += yMargin;
            this.m_arRadioButtonSourceData[(int)indx].AutoCheck = false;
            this.m_arRadioButtonSourceData[(int)indx].AutoSize = true;
            this.m_arRadioButtonSourceData[(int)indx].Location = new System.Drawing.Point(6, yPos);
            this.m_arRadioButtonSourceData[(int)indx].Name = "rbtnSourceData_SOTIASSO_3_min";
            this.m_arRadioButtonSourceData[(int)indx].Size = new System.Drawing.Size(84, 17);
            this.m_arRadioButtonSourceData[(int)indx].TabIndex = 0;
            this.m_arRadioButtonSourceData[(int)indx].Text = "��������(3 ���)";
            this.m_arRadioButtonSourceData[(int)indx].UseVisualStyleBackColor = true;
            this.m_arRadioButtonSourceData[(int)indx].Click += new System.EventHandler(this.rbtnSourceData_SOTIASSO3min_Click);
            // 
            // rbtnSourceData_SOTIASSO_1_min
            // ���������� indx ��������� 3, yPos ��������� 54+19=73;
            indx = (int)CONN_SETT_TYPE.SOTIASSO_1_MIN; yPos += yMargin;
            this.m_arRadioButtonSourceData[(int)indx].AutoCheck = false;
            this.m_arRadioButtonSourceData[(int)indx].AutoSize = true;
            this.m_arRadioButtonSourceData[(int)indx].Location = new System.Drawing.Point(6, yPos);
            this.m_arRadioButtonSourceData[(int)indx].Name = "rbtnSourceData_SOTIASSO_1_min";
            this.m_arRadioButtonSourceData[(int)indx].Size = new System.Drawing.Size(84, 17);
            this.m_arRadioButtonSourceData[(int)indx].TabIndex = 0;
            this.m_arRadioButtonSourceData[(int)indx].Text = "��������(1 ���)";
            this.m_arRadioButtonSourceData[(int)indx].UseVisualStyleBackColor = true;
            this.m_arRadioButtonSourceData[(int)indx].Click += new System.EventHandler(this.rbtnSourceData_SOTIASSO1min_Click);
            // 
            // rbtnSourceData_COSTUMIZE
            // ���������� indx ��������� 4, yPos ��������� 73+19=92;
            indx = (int)CONN_SETT_TYPE.COSTUMIZE; yPos += yMargin;
            this.m_arRadioButtonSourceData[(int)indx].AutoCheck = false;
            this.m_arRadioButtonSourceData[(int)indx].AutoSize = true;
            this.m_arRadioButtonSourceData[(int)indx].Location = new System.Drawing.Point(6, yPos);
            this.m_arRadioButtonSourceData[(int)indx].Name = "rbtnSourceData_COSTUMIZE";
            this.m_arRadioButtonSourceData[(int)indx].Size = new System.Drawing.Size(80, 17);
            this.m_arRadioButtonSourceData[(int)indx].TabIndex = 2;
            this.m_arRadioButtonSourceData[(int)indx].TabStop = true;
            this.m_arRadioButtonSourceData[(int)indx].Text = "���������";
            this.m_arRadioButtonSourceData[(int)indx].UseVisualStyleBackColor = true;
            this.m_arRadioButtonSourceData[(int)indx].Click += new System.EventHandler(this.rbtnSourceData_COSTUMIZE_Click);
            // 
            // FormGraphicsSettings
            // 
            // �������������� �  96 DPI (���� ���������� �����������)
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            // ������������������� ������
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            // ������ ���� 407*300
            this.ClientSize = new System.Drawing.Size(407, 300);
            // ���������� ��������� ���������� � ����
            for (int i = 0; i < (int)INDEX_COLOR.COUNT_INDEX_COLOR; i++)
                this.Controls.Add(this.m_arlblColor [i]);
            this.Controls.Add(this.groupBoxSourceData);            
            this.Controls.Add(this.gbxType);
            this.Controls.Add(this.cbxScale);
            // ����� ����� 
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            // ���� �� ��������������� � �� �������������
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            // ����������� ������, � ������� ����� ���� ������� ������ �����
            //this.MinimumSize = new System.Drawing.Size(170, 25);
            this.Name = "FormGraphicsSettings";
            // �� ���������� ������ � ������ ��������� �����
            this.ShowIcon = false;
            // ����� ��  ������������ � ������ ����� Windows �� ����� ����������
            this.ShowInTaskbar = false;
            // ������������ ����� ��� �������
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "��������� ��������";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GraphicsSettings_FormClosing);
            // ����� ResumeLayout ������������ ������ ��������� ������������ 
            this.gbxType.ResumeLayout(false);
            this.gbxType.PerformLayout();
            this.groupBoxSourceData.ResumeLayout(false);
            this.groupBoxSourceData.PerformLayout();
            this.ResumeLayout(false);
            //��������� ����������
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label [] m_arlblColor;
        private System.Windows.Forms.CheckBox cbxScale;             
        private System.Windows.Forms.GroupBox gbxType;
        private System.Windows.Forms.RadioButton rbtnBar;
        private System.Windows.Forms.RadioButton rbtnLine;        
        private System.Windows.Forms.GroupBox groupBoxSourceData;
        private System.Windows.Forms.RadioButton [] m_arRadioButtonSourceData;        
    }
}