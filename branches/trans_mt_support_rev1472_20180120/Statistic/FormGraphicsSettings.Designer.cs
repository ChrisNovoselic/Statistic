using System;
using System.Drawing;
using System.Windows.Forms;
/// <summary>
/// ������������ ���� Statistic 
/// </summary>
namespace Statistic {
    /// <summary>
    /// ��������� ����� FormGraphicsSettings (��������� ��������) 
    /// </summary>
    partial class FormGraphicsSettings {

        /// <summary>
        /// ��������� LABEL_COLOR
        /// </summary>
        struct LABEL_COLOR {

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
            public LABEL_COLOR (Color col, string name, string text, System.Drawing.Point pt)
            {
                this.color = col;
                this.name = name;
                this.text = text;
                this.pos = pt;
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
        protected override void Dispose (bool disposing)
        {
            if (disposing && (components != null)) {
                components.Dispose ();
            }
            base.Dispose (disposing);
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

        private void InitializeComponent ()
        {
            // �������� ������� �������  (����,���������� � �.�.)
            this.m_arlblColorValues = new System.Windows.Forms.Label [(int)INDEX_COLOR_VAUES.COUNT_INDEX_COLOR];
            this.gbxColorShema = new System.Windows.Forms.GroupBox ();
            //this.m_arRbtnColorShema = new System.Windows.Forms.RadioButton [] {
            //    new System.Windows.Forms.RadioButton()
            //    , new System.Windows.Forms.RadioButton()
            //};
            this.m_cbUseSystemColors = new System.Windows.Forms.CheckBox ();
            this.m_arlblColorShema = new System.Windows.Forms.Label [Enum.GetValues(typeof(INDEX_COLOR_SHEMA)).Length];
            // �������� GroupBox (����: ��� ��������)         
            this.gbxTypeGraph = new System.Windows.Forms.GroupBox ();
            // �������� CheckBox (������)
            this.cbxScale = new System.Windows.Forms.CheckBox ();
            // �������� RadioButton (�������������:�����������, ��������)      
            this.m_arRbtnTypeGraph = new System.Windows.Forms.RadioButton [] {
                new System.Windows.Forms.RadioButton()
                , new System.Windows.Forms.RadioButton()
            };
            // �������� GroupBox (����: ���� �������� ��������)      
            this.gbxSourceData = new System.Windows.Forms.GroupBox ();
            // �������� ������� �������������� "���� �������� ��������"
            this.m_arRbtnSourceData = new System.Windows.Forms.RadioButton [] {
                new System.Windows.Forms.RadioButton()
                , new System.Windows.Forms.RadioButton()
                , new System.Windows.Forms.RadioButton()
                , new System.Windows.Forms.RadioButton()
                , new System.Windows.Forms.RadioButton()
            };
            // ����� SuspendLayout() ���������������� ���������� � ������ "�������� �����", "��� ��������" � "���� �������� ��������"
            this.gbxColorShema.SuspendLayout ();
            this.gbxTypeGraph.SuspendLayout ();
            this.gbxSourceData.SuspendLayout ();
            // SuspendLayout ������������� ������ ��������� ������������ (layout logic)
            this.SuspendLayout ();

            #region ������� ��� ������ ����� ���������
            // ������ ��������, ��������� �� 10 ��������� (����,���������� � �.�.)
            LABEL_COLOR [] arLabelColor = new LABEL_COLOR [(int)INDEX_COLOR_VAUES.COUNT_INDEX_COLOR]
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
                , new LABEL_COLOR (Color.FromArgb(231, 231, 238), "lblBG_ASKUTE_color", "��� (��������)", new System.Drawing.Point(12, 236))
                , new LABEL_COLOR (Color.FromArgb(200, 200, 200), "lblGRIDcolor", "�����", new System.Drawing.Point(12, 261))
            };

            // ��� ������� ��������� (���,���������� � �.�.)
            for (int i = 0; i < (int)INDEX_COLOR_VAUES.COUNT_INDEX_COLOR; i++) {
                // C������ ������� 
                this.m_arlblColorValues [i] = new System.Windows.Forms.Label ();

                this.m_arlblColorValues [i].Tag = (INDEX_COLOR_VAUES)i;
                // ���� ������� ����� (�������)
                this.m_arlblColorValues [i].BackColor = arLabelColor [i].color;
                // ���� ��������� ����� (�������)
                this.m_arlblColorValues [i].ForeColor = getForeColor (arLabelColor [i].color);
                // ����� ����� 
                this.m_arlblColorValues [i].BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                // ���������
                this.m_arlblColorValues [i].Location = arLabelColor [i].pos;
                // ���
                this.m_arlblColorValues [i].Name = arLabelColor [i].name;
                // ������
                this.m_arlblColorValues [i].Size = new System.Drawing.Size (195, 26);
                // ���� �������
                this.m_arlblColorValues [i].Text = arLabelColor [i].text;
                // ������������ ������
                this.m_arlblColorValues [i].TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                // ��� CLick �� ����� ����������� ������� ���������� ������� ������� 
                this.m_arlblColorValues [i].Click += new System.EventHandler (this.lbl_color_Click);
            }
            #endregion

            int indx = -1
                , yPos = -1 //������� �� ��� �������
                , yMargin = 22; //���������� ����� ���������� ���������� �� ��� �������

            #region �������� �����            
            // ���������� ������������ �����
            this.gbxColorShema.Location = new System.Drawing.Point (222, 6);
            // ��� �����
            this.gbxColorShema.Name = "gbxColorShema";
            // ������ 
            this.gbxColorShema.Size = new System.Drawing.Size (173, 60);
            // ������������������ �������� ����� �������� ��� ������� �� ������ Tab
            this.gbxColorShema.TabIndex = 5;
            // gbxType �� �������� ����� � ������� ������� TAB. 
            this.gbxColorShema.TabStop = false;
            // ����� �������
            this.gbxColorShema.Text = "�������� �����";
            // 
            // cbUseSystemColors ������� "�������"
            // 
            indx = (int)ColorShemas.System;
            this.m_cbUseSystemColors.AutoSize = true;            
            this.m_cbUseSystemColors.Tag = (ColorShemas)indx;
            // �������� �������� �������
            this.m_cbUseSystemColors.Checked = true;
            this.m_cbUseSystemColors.Location = new System.Drawing.Point (6, yPos = 16);
            this.m_cbUseSystemColors.Name = "cbUseSystemColors";
            this.m_cbUseSystemColors.Size = new System.Drawing.Size (92, 17);
            this.m_cbUseSystemColors.TabIndex = 1;
            this.m_cbUseSystemColors.TabStop = true;
            this.m_cbUseSystemColors.Text = "�������";
            this.m_cbUseSystemColors.UseVisualStyleBackColor = true;
            this.m_cbUseSystemColors.CheckedChanged += new System.EventHandler (this.cbUseSystemColors_CheckedChanged);
            this.m_cbUseSystemColors.Enabled = _allowedChangeShema;
            // 
            // labelColorShema ������� "���"
            // 
            int wLabelColorShema = ((gbxColorShema.ClientSize.Width - (2 * 6)) / 2) - 1; // "1" - ���������� ����� ��������� �� �����������
            indx = (int)INDEX_COLOR_SHEMA.BACKGROUND;
            this.m_arlblColorShema [indx] = new System.Windows.Forms.Label ();
            this.m_arlblColorShema[indx].Tag = (INDEX_COLOR_SHEMA)indx;
            //this.m_arlblColorShema [indx].AutoSize = true;
            this.m_arlblColorShema [indx].Location = new System.Drawing.Point (6, yPos += (yMargin - 2));
            this.m_arlblColorShema [indx].Name = "labelColorShema";
            this.m_arlblColorShema [indx].Size = new System.Drawing.Size (wLabelColorShema, 17 + 2);
            this.m_arlblColorShema [indx].TabIndex = 0;
            this.m_arlblColorShema [indx].Text = "���";
            this.m_arlblColorShema [indx].TextAlign = ContentAlignment.MiddleLeft;
            this.m_arlblColorShema [indx].BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;            
            this.m_arlblColorShema [indx].BackColor = CustomColorTable.BackColor;
            this.m_arlblColorShema [indx].ForeColor = CustomColorTable.ForeColor;
            //this.m_arlblColorShema [indx].Enabled = false;
            // ��������� ������� "������� ������" - ��������� ����� "������" �����
            this.m_arlblColorShema [indx].Click += new System.EventHandler (lbl_color_Click);
            this.m_arlblColorShema [indx].BackColorChanged += new System.EventHandler (labelColorShema_ValueChanged);
            // 
            // labelColorFont ������� "�����"
            // 
            indx = (int)INDEX_COLOR_SHEMA.FONT;
            this.m_arlblColorShema [indx] = new System.Windows.Forms.Label ();
            this.m_arlblColorShema [indx].Tag = (INDEX_COLOR_SHEMA)indx;
            //this.m_arlblColorShema [indx].AutoSize = true;
            this.m_arlblColorShema [indx].Location = new System.Drawing.Point (6 + wLabelColorShema + 2 * 1, yPos); // "2 * 1" - ���������� ����� ��������� �� �����������
            this.m_arlblColorShema [indx].Name = "labelColorShema";
            this.m_arlblColorShema [indx].Size = new System.Drawing.Size (wLabelColorShema, 17 + 2);
            this.m_arlblColorShema [indx].TabIndex = 0;
            this.m_arlblColorShema [indx].Text = "�����";
            this.m_arlblColorShema [indx].TextAlign = ContentAlignment.MiddleLeft;
            this.m_arlblColorShema [indx].BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            //TODO:
            this.m_arlblColorShema [indx].BackColor = CustomColorTable.BackColor;
            this.m_arlblColorShema [indx].ForeColor = CustomColorTable.ForeColor;
            //this.m_arlblColorShema [indx].Enabled = false;
            // ��������� ������� "������� ������" - ��������� ����� "������" �����
            this.m_arlblColorShema [indx].Click += new System.EventHandler (lbl_color_Click);
            this.m_arlblColorShema [indx].ForeColorChanged += new System.EventHandler (labelColorShema_ValueChanged);
            #endregion

            #region ���������������, ���� ��������
            // 
            // gbxType ������� "��� ��������"
            // 
            this.gbxTypeGraph.Controls.Add (this.cbxScale);
            // ���������� � ����  (���������) ��������� "�����������" � "��������"
            this.gbxTypeGraph.Controls.AddRange (m_arRbtnTypeGraph);
            // ���������� ������������ �����
            this.gbxTypeGraph.Location = new System.Drawing.Point (222, 69);
            // ��� �����
            this.gbxTypeGraph.Name = "gbxTypeGraph";
            // ������ 
            this.gbxTypeGraph.Size = new System.Drawing.Size (173, 88);
            // ������������������ �������� ����� �������� ��� ������� �� ������ Tab
            this.gbxTypeGraph.TabIndex = 5;
            // gbxType �� �������� ����� � ������� ������� TAB. 
            this.gbxTypeGraph.TabStop = false;
            // ����� �������
            this.gbxTypeGraph.Text = "��� ��������";
            // 
            // cbxScale ������� "��������������� ��������"
            // 
            // ������ �������� ������������� ��������� � ������������ � �������� ��� �����������
            this.cbxScale.AutoSize = true;
            //���������� ������ �������� ���� �������� ������������ ������ �������� ���� ��� ���������� 
            this.cbxScale.Location = new System.Drawing.Point (6, yPos = 19);
            // ��� ��������
            this.cbxScale.Name = "cbxScale";
            // ������ ��������
            this.cbxScale.Size = new System.Drawing.Size (159, 17);
            // ������������������ �������� ����� �������� ��� ������� �� ������ Tab
            this.cbxScale.TabIndex = 0;
            // ������� ��������
            this.cbxScale.Text = "�������������� �������";
            // ��������� ���� � ������� ���������� ������
            this.cbxScale.UseVisualStyleBackColor = true;
            // ��������� ������� ������ ������������ �������
            this.cbxScale.CheckedChanged += new System.EventHandler (this.cbxScale_CheckedChanged);
            // 
            // rbtnBar ������� "�����������"
            // 
            indx = (int)GraphTypes.Bar;
            this.m_arRbtnTypeGraph [indx].AutoSize = true;
            // �������� �������� �������
            this.m_arRbtnTypeGraph [indx].Tag = (GraphTypes)indx;
            this.m_arRbtnTypeGraph [indx].Checked = true;
            this.m_arRbtnTypeGraph [indx].Location = new System.Drawing.Point (6, yPos += yMargin);
            this.m_arRbtnTypeGraph [indx].Name = "rbtnBar";
            this.m_arRbtnTypeGraph [indx].Size = new System.Drawing.Size (92, 17);
            this.m_arRbtnTypeGraph [indx].TabIndex = 1;
            this.m_arRbtnTypeGraph [indx].TabStop = true;
            this.m_arRbtnTypeGraph [indx].Text = "�����������";
            this.m_arRbtnTypeGraph [indx].UseVisualStyleBackColor = true;
            // 
            // rbtnLine ������� "��������"
            // 
            indx = (int)GraphTypes.Linear;
            this.m_arRbtnTypeGraph [indx].Tag = (GraphTypes)indx;
            this.m_arRbtnTypeGraph [indx].AutoSize = true;
            this.m_arRbtnTypeGraph [indx].Location = new System.Drawing.Point (6, yPos += yMargin);
            this.m_arRbtnTypeGraph [indx].Name = "rbtnLine";
            this.m_arRbtnTypeGraph [indx].Size = new System.Drawing.Size (75, 17);
            this.m_arRbtnTypeGraph [indx].TabIndex = 0;
            this.m_arRbtnTypeGraph [indx].Text = "��������";
            this.m_arRbtnTypeGraph [indx].UseVisualStyleBackColor = true;
            // ��������� ������� ������ ������������ �������
            this.m_arRbtnTypeGraph [indx].CheckedChanged += new System.EventHandler (this.rbtnTypeGraph_CheckedChanged);
            #endregion

            #region ���� �������� ��������
            // 
            // groupBoxSourceData ������� "���� �������� ��������"   
            // 
            // ���������� ������� ������ � ���� (���������)
            this.gbxSourceData.Controls.AddRange (m_arRbtnSourceData);
            this.gbxSourceData.Location = new System.Drawing.Point (222, 162);
            this.gbxSourceData.Name = "groupBoxSourceData";
            this.gbxSourceData.Size = new System.Drawing.Size (173, 126);
            this.gbxSourceData.TabIndex = 8;
            this.gbxSourceData.TabStop = false;
            this.gbxSourceData.Text = "���� �������� ��������";

            // 
            // rbtnSourceData_AISKUE_PLUS_SOTIASSO
            // 
            // ���������� indx �������� 0, yPos �������� 16;
            indx = (int)CONN_SETT_TYPE.AISKUE_PLUS_SOTIASSO;
            yPos = 16;
            // ������ ��������� ���������� RadioButton �� ����� ����������� ��� ����������������� ������,
            // � �������� Checked ������ ���� ��������� � ����
            this.m_arRbtnSourceData [(int)indx].Tag = indx;
            this.m_arRbtnSourceData [(int)indx].AutoCheck = false;
            this.m_arRbtnSourceData [(int)indx].AutoSize = true;
            this.m_arRbtnSourceData [(int)indx].Location = new System.Drawing.Point (6, yPos);
            this.m_arRbtnSourceData [(int)indx].Name = "rbtnSourceData_ASKUE_PLUS_SOTIASSO";
            this.m_arRbtnSourceData [(int)indx].Size = new System.Drawing.Size (134, 17);
            this.m_arRbtnSourceData [(int)indx].TabIndex = 3;
            this.m_arRbtnSourceData [(int)indx].Text = "������+��������";
            this.m_arRbtnSourceData [(int)indx].UseVisualStyleBackColor = true;
            this.m_arRbtnSourceData [(int)indx].Click += new System.EventHandler (this.rbtnSourceData_Click);
            // 
            // rbtnSourceData_ASKUE
            // ���������� indx ��������� 1, yPos ��������� 16+19=35;
            indx = (int)CONN_SETT_TYPE.AISKUE_3_MIN;
            yPos += yMargin;
            this.m_arRbtnSourceData [(int)indx].Tag = indx;
            this.m_arRbtnSourceData [(int)indx].AutoCheck = false;
            this.m_arRbtnSourceData [(int)indx].AutoSize = true;
            this.m_arRbtnSourceData [(int)indx].Checked = true;
            this.m_arRbtnSourceData [(int)indx].Location = new System.Drawing.Point (6, yPos);
            this.m_arRbtnSourceData [(int)indx].Name = "rbtnSourceData_ASKUE";
            this.m_arRbtnSourceData [(int)indx].Size = new System.Drawing.Size (69, 17);
            this.m_arRbtnSourceData [(int)indx].TabIndex = 1;
            this.m_arRbtnSourceData [(int)indx].Text = "������";
            this.m_arRbtnSourceData [(int)indx].UseVisualStyleBackColor = true;
            this.m_arRbtnSourceData [(int)indx].Click += new System.EventHandler (this.rbtnSourceData_Click);
            // 
            // rbtnSourceData_SOTIASSO_3_min
            // ���������� indx ��������� 2, yPos ��������� 35+19=54;
            indx = (int)CONN_SETT_TYPE.SOTIASSO_3_MIN;
            yPos += yMargin;
            this.m_arRbtnSourceData [(int)indx].Tag = indx;
            this.m_arRbtnSourceData [(int)indx].AutoCheck = false;
            this.m_arRbtnSourceData [(int)indx].AutoSize = true;
            this.m_arRbtnSourceData [(int)indx].Location = new System.Drawing.Point (6, yPos);
            this.m_arRbtnSourceData [(int)indx].Name = "rbtnSourceData_SOTIASSO_3_min";
            this.m_arRbtnSourceData [(int)indx].Size = new System.Drawing.Size (84, 17);
            this.m_arRbtnSourceData [(int)indx].TabIndex = 0;
            this.m_arRbtnSourceData [(int)indx].Text = "��������(3 ���)";
            this.m_arRbtnSourceData [(int)indx].UseVisualStyleBackColor = true;
            this.m_arRbtnSourceData [(int)indx].Click += new System.EventHandler (this.rbtnSourceData_Click);
            // 
            // rbtnSourceData_SOTIASSO_1_min
            // ���������� indx ��������� 3, yPos ��������� 54+19=73;
            indx = (int)CONN_SETT_TYPE.SOTIASSO_1_MIN;
            yPos += yMargin;
            this.m_arRbtnSourceData [(int)indx].Tag = indx;
            this.m_arRbtnSourceData [(int)indx].AutoCheck = false;
            this.m_arRbtnSourceData [(int)indx].AutoSize = true;
            this.m_arRbtnSourceData [(int)indx].Location = new System.Drawing.Point (6, yPos);
            this.m_arRbtnSourceData [(int)indx].Name = "rbtnSourceData_SOTIASSO_1_min";
            this.m_arRbtnSourceData [(int)indx].Size = new System.Drawing.Size (84, 17);
            this.m_arRbtnSourceData [(int)indx].TabIndex = 0;
            this.m_arRbtnSourceData [(int)indx].Text = "��������(1 ���)";
            this.m_arRbtnSourceData [(int)indx].UseVisualStyleBackColor = true;
            this.m_arRbtnSourceData [(int)indx].Click += new System.EventHandler (this.rbtnSourceData_Click);
            // 
            // rbtnSourceData_COSTUMIZE
            // ���������� indx ��������� 4, yPos ��������� 73+19=92;
            indx = (int)CONN_SETT_TYPE.COSTUMIZE;
            yPos += yMargin;
            this.m_arRbtnSourceData [(int)indx].Tag = indx;
            this.m_arRbtnSourceData [(int)indx].AutoCheck = false;
            this.m_arRbtnSourceData [(int)indx].AutoSize = true;
            this.m_arRbtnSourceData [(int)indx].Location = new System.Drawing.Point (6, yPos);
            this.m_arRbtnSourceData [(int)indx].Name = "rbtnSourceData_COSTUMIZE";
            this.m_arRbtnSourceData [(int)indx].Size = new System.Drawing.Size (80, 17);
            this.m_arRbtnSourceData [(int)indx].TabIndex = 2;
            this.m_arRbtnSourceData [(int)indx].TabStop = true;
            this.m_arRbtnSourceData [(int)indx].Text = "���������";
            this.m_arRbtnSourceData [(int)indx].UseVisualStyleBackColor = true;
            this.m_arRbtnSourceData [(int)indx].Click += new System.EventHandler (this.rbtnSourceData_Click);
            #endregion

            // 
            // FormGraphicsSettings
            // 
            // �������������� �  96 DPI (���� ���������� �����������)
            this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
            // ������������������� ������
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            // ������ ���� 407*300
            this.ClientSize = new System.Drawing.Size (407, 300);
            // ���������� ��������� ���������� � ����
            for (int i = 0; i < (int)INDEX_COLOR_VAUES.COUNT_INDEX_COLOR; i++)
                this.Controls.Add (this.m_arlblColorValues [i]);
            // ���������� � ����  (���������) ��������� "���" � "�����"
            this.gbxColorShema.Controls.AddRange (
                new System.Windows.Forms.Control [] { m_cbUseSystemColors, m_arlblColorShema [(int)INDEX_COLOR_SHEMA.BACKGROUND], m_arlblColorShema [(int)INDEX_COLOR_SHEMA.FONT] }
            );
            this.Controls.Add (this.gbxColorShema);
            this.Controls.Add (this.gbxTypeGraph);
            this.Controls.Add (this.gbxSourceData);
            //this.Controls.Add(this.cbxScale);
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
            this.Text = "��������� ������������ ����������";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler (this.GraphicsSettings_FormClosing);
            // ����� ResumeLayout ������������ ������ ��������� ������������ 
            this.gbxColorShema.ResumeLayout (false);
            this.gbxColorShema.PerformLayout ();
            this.gbxTypeGraph.ResumeLayout (false);
            this.gbxTypeGraph.PerformLayout ();
            this.gbxSourceData.ResumeLayout (false);
            this.gbxSourceData.PerformLayout ();
            this.ResumeLayout (false);
            //��������� ����������
            this.PerformLayout ();

        }

        #endregion

        private System.Windows.Forms.Label [] m_arlblColorValues;
        private System.Windows.Forms.GroupBox gbxColorShema;
        //private System.Windows.Forms.RadioButton [] m_arRbtnColorShema;
        private System.Windows.Forms.CheckBox m_cbUseSystemColors;
        private System.Windows.Forms.Label [] m_arlblColorShema;
        private System.Windows.Forms.GroupBox gbxTypeGraph;
        private System.Windows.Forms.CheckBox cbxScale;
        private System.Windows.Forms.RadioButton [] m_arRbtnTypeGraph;
        private System.Windows.Forms.GroupBox gbxSourceData;
        private System.Windows.Forms.RadioButton [] m_arRbtnSourceData;
    }
}