using System.Drawing;

namespace Statistic
{
    partial class FormGraphicsSettings
    {
        struct LABEL_COLOR
        {
            public Color color;
            public string name, text;
            public System.Drawing.Point pos;

            public LABEL_COLOR(Color col, string name, string text, System.Drawing.Point pt)
            {
                this.color = col;  this.name = name; this.text = text; this.pos = pt;
            }
        }

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
            this.cbxScale = new System.Windows.Forms.CheckBox();
            this.m_arlblColor = new System.Windows.Forms.Label [(int)INDEX_COLOR.COUNT_INDEX_COLOR];            
            this.gbxType = new System.Windows.Forms.GroupBox();
            this.rbtnBar = new System.Windows.Forms.RadioButton();
            this.rbtnLine = new System.Windows.Forms.RadioButton();
            this.groupBoxSourceData = new System.Windows.Forms.GroupBox();
            this.rbtnSourceData_ASKUE_PLUS_SOTIASSO = new System.Windows.Forms.RadioButton();
            this.rbtnSourceData_COSTUMIZE = new System.Windows.Forms.RadioButton();
            this.rbtnSourceData_ASKUE = new System.Windows.Forms.RadioButton();
            this.rbtnSourceData_SOTIASSO = new System.Windows.Forms.RadioButton();            
            this.gbxType.SuspendLayout();
            this.groupBoxSourceData.SuspendLayout();
            this.SuspendLayout();
            // 
            // cbxScale
            // 
            this.cbxScale.AutoSize = true;
            this.cbxScale.Location = new System.Drawing.Point(167, 13);
            this.cbxScale.Name = "cbxScale";
            this.cbxScale.Size = new System.Drawing.Size(159, 17);
            this.cbxScale.TabIndex = 0;
            this.cbxScale.Text = "Масштабировать графики";
            this.cbxScale.UseVisualStyleBackColor = true;
            this.cbxScale.CheckedChanged += new System.EventHandler(this.cbxScale_CheckedChanged);

            LABEL_COLOR [] arLabelColor = new LABEL_COLOR [(int)INDEX_COLOR.COUNT_INDEX_COLOR] {
                new LABEL_COLOR (Color.FromArgb(0, 0, 0), "lblUDGcolor", "УДГэ", new System.Drawing.Point(12, 11))
                , new LABEL_COLOR (Color.FromArgb(255, 0, 0), "lblDIVcolor", "Отклонение", new System.Drawing.Point(12, 36))
                , new LABEL_COLOR (Color.FromArgb(0, 128, 0), "lblP_ASKUEcolor", "Мощность (АИСКУЭ)", new System.Drawing.Point(12, 61))
                , new LABEL_COLOR (Color.FromArgb(0, 128, 192), "lblP_SOTIASSOcolor", "Мощность (СОТИАССО)", new System.Drawing.Point(12, 86))
                , new LABEL_COLOR (Color.FromArgb(255, 255, 0), "lblRECcolor", "Рекомендация", new System.Drawing.Point(12, 111))
                , new LABEL_COLOR (Color.FromArgb(231, 231, 238 /*230, 230, 230*/), "lblBG_ASKUE_color", "Фон (АИСКУЭ)", new System.Drawing.Point(12, 136))
                , new LABEL_COLOR (Color.FromArgb(231, 238, 231), "lblBG_SOTIASSO_color", "Фон (СОТИАССО)", new System.Drawing.Point(12, 161))                
                , new LABEL_COLOR (Color.FromArgb(200, 200, 200), "lblGRIDcolor", "Сетка", new System.Drawing.Point(12, 186))
            };

            for (int i = 0; i < (int)INDEX_COLOR.COUNT_INDEX_COLOR; i++)
            {
                this.m_arlblColor[i] = new System.Windows.Forms.Label();
                this.m_arlblColor[i].BackColor = arLabelColor[i].color;
                this.m_arlblColor[i].ForeColor = getForeColor(arLabelColor[i].color);
                this.m_arlblColor[i].BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                this.m_arlblColor[i].Location = arLabelColor[i].pos;
                this.m_arlblColor[i].Name = arLabelColor[i].name;
                this.m_arlblColor[i].Size = new System.Drawing.Size(140, 26);
                this.m_arlblColor[i].Text = arLabelColor [i].text;
                this.m_arlblColor[i].TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                this.m_arlblColor[i].Click += new System.EventHandler(this.lbl_color_Click);
            }
            // 
            // gbxType
            // 
            this.gbxType.Controls.Add(this.rbtnBar);
            this.gbxType.Controls.Add(this.rbtnLine);
            this.gbxType.Location = new System.Drawing.Point(167, 32);
            this.gbxType.Name = "gbxType";
            this.gbxType.Size = new System.Drawing.Size(173, 58);
            this.gbxType.TabIndex = 5;
            this.gbxType.TabStop = false;
            this.gbxType.Text = "Тип графиков";
            // 
            // rbtnBar
            // 
            this.rbtnBar.AutoSize = true;
            this.rbtnBar.Checked = true;
            this.rbtnBar.Location = new System.Drawing.Point(6, 16);
            this.rbtnBar.Name = "rbtnBar";
            this.rbtnBar.Size = new System.Drawing.Size(92, 17);
            this.rbtnBar.TabIndex = 1;
            this.rbtnBar.TabStop = true;
            this.rbtnBar.Text = "гистограмма";
            this.rbtnBar.UseVisualStyleBackColor = true;
            // 
            // rbtnLine
            // 
            this.rbtnLine.AutoSize = true;
            this.rbtnLine.Location = new System.Drawing.Point(6, 37);
            this.rbtnLine.Name = "rbtnLine";
            this.rbtnLine.Size = new System.Drawing.Size(75, 17);
            this.rbtnLine.TabIndex = 0;
            this.rbtnLine.Text = "линейный";
            this.rbtnLine.UseVisualStyleBackColor = true;
            this.rbtnLine.CheckedChanged += new System.EventHandler(this.rbtnLine_CheckedChanged);
            // 
            // groupBoxSourceData
            // 
            this.groupBoxSourceData.Controls.Add(this.rbtnSourceData_ASKUE_PLUS_SOTIASSO);
            this.groupBoxSourceData.Controls.Add(this.rbtnSourceData_COSTUMIZE);
            this.groupBoxSourceData.Controls.Add(this.rbtnSourceData_ASKUE);
            this.groupBoxSourceData.Controls.Add(this.rbtnSourceData_SOTIASSO);
            this.groupBoxSourceData.Location = new System.Drawing.Point(167, 96);
            this.groupBoxSourceData.Name = "groupBoxSourceData";
            this.groupBoxSourceData.Size = new System.Drawing.Size(173, 114);
            this.groupBoxSourceData.TabIndex = 8;
            this.groupBoxSourceData.TabStop = false;
            this.groupBoxSourceData.Text = "Источники данных графиков";
            // 
            // rbtnSourceData_ASKUE_PLUS_SOTIASSO
            // 
            this.rbtnSourceData_ASKUE_PLUS_SOTIASSO.AutoCheck = false;
            this.rbtnSourceData_ASKUE_PLUS_SOTIASSO.AutoSize = true;
            this.rbtnSourceData_ASKUE_PLUS_SOTIASSO.Location = new System.Drawing.Point(6, 17);
            this.rbtnSourceData_ASKUE_PLUS_SOTIASSO.Name = "rbtnSourceData_ASKUE_PLUS_SOTIASSO";
            this.rbtnSourceData_ASKUE_PLUS_SOTIASSO.Size = new System.Drawing.Size(134, 17);
            this.rbtnSourceData_ASKUE_PLUS_SOTIASSO.TabIndex = 3;
            this.rbtnSourceData_ASKUE_PLUS_SOTIASSO.Text = "АИСКУЭ+СОТИАССО";
            this.rbtnSourceData_ASKUE_PLUS_SOTIASSO.UseVisualStyleBackColor = true;
            this.rbtnSourceData_ASKUE_PLUS_SOTIASSO.Click += new System.EventHandler(this.rbtnSourceData_ASKUEPLUSSOTIASSO_Click);
            // 
            // rbtnSourceData_COSTUMIZE
            // 
            this.rbtnSourceData_COSTUMIZE.AutoCheck = false;
            this.rbtnSourceData_COSTUMIZE.AutoSize = true;
            this.rbtnSourceData_COSTUMIZE.Location = new System.Drawing.Point(6, 87);
            this.rbtnSourceData_COSTUMIZE.Name = "rbtnSourceData_COSTUMIZE";
            this.rbtnSourceData_COSTUMIZE.Size = new System.Drawing.Size(80, 17);
            this.rbtnSourceData_COSTUMIZE.TabIndex = 2;
            this.rbtnSourceData_COSTUMIZE.TabStop = true;
            this.rbtnSourceData_COSTUMIZE.Text = "выборочно";
            this.rbtnSourceData_COSTUMIZE.UseVisualStyleBackColor = true;
            this.rbtnSourceData_COSTUMIZE.Click += new System.EventHandler(this.rbtnSourceData_COSTUMIZE_Click);
            // 
            // rbtnSourceData_ASKUE
            // 
            this.rbtnSourceData_ASKUE.AutoCheck = false;
            this.rbtnSourceData_ASKUE.AutoSize = true;
            this.rbtnSourceData_ASKUE.Checked = true;
            this.rbtnSourceData_ASKUE.Location = new System.Drawing.Point(6, 40);
            this.rbtnSourceData_ASKUE.Name = "rbtnSourceData_ASKUE";
            this.rbtnSourceData_ASKUE.Size = new System.Drawing.Size(69, 17);
            this.rbtnSourceData_ASKUE.TabIndex = 1;
            this.rbtnSourceData_ASKUE.Text = "АИСКУЭ";
            this.rbtnSourceData_ASKUE.UseVisualStyleBackColor = true;
            this.rbtnSourceData_ASKUE.Click += new System.EventHandler(this.rbtnSourceData_ASKUE_Click);
            // 
            // rbtnSourceData_SOTIASSO
            // 
            this.rbtnSourceData_SOTIASSO.AutoCheck = false;
            this.rbtnSourceData_SOTIASSO.AutoSize = true;
            this.rbtnSourceData_SOTIASSO.Location = new System.Drawing.Point(6, 64);
            this.rbtnSourceData_SOTIASSO.Name = "rbtnSourceData_SOTIASSO";
            this.rbtnSourceData_SOTIASSO.Size = new System.Drawing.Size(84, 17);
            this.rbtnSourceData_SOTIASSO.TabIndex = 0;
            this.rbtnSourceData_SOTIASSO.Text = "СОТИАССО";
            this.rbtnSourceData_SOTIASSO.UseVisualStyleBackColor = true;
            this.rbtnSourceData_SOTIASSO.Click += new System.EventHandler(this.rbtnSourceData_SOTIASSO_Click);
            // 
            // FormGraphicsSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(352, 217);
            for (int i = 0; i < (int)INDEX_COLOR.COUNT_INDEX_COLOR; i++)
                this.Controls.Add(this.m_arlblColor [i]);
            this.Controls.Add(this.groupBoxSourceData);            
            this.Controls.Add(this.gbxType);
            this.Controls.Add(this.cbxScale);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(170, 25);
            this.Name = "FormGraphicsSettings";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Настройки графиков";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GraphicsSettings_FormClosing);
            this.gbxType.ResumeLayout(false);
            this.gbxType.PerformLayout();
            this.groupBoxSourceData.ResumeLayout(false);
            this.groupBoxSourceData.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label [] m_arlblColor;
        private System.Windows.Forms.CheckBox cbxScale;
        private System.Windows.Forms.GroupBox gbxType;
        private System.Windows.Forms.RadioButton rbtnBar;
        private System.Windows.Forms.RadioButton rbtnLine;        
        private System.Windows.Forms.GroupBox groupBoxSourceData;
        private System.Windows.Forms.RadioButton rbtnSourceData_COSTUMIZE;
        private System.Windows.Forms.RadioButton rbtnSourceData_ASKUE;
        private System.Windows.Forms.RadioButton rbtnSourceData_SOTIASSO;        
        private System.Windows.Forms.RadioButton rbtnSourceData_ASKUE_PLUS_SOTIASSO;
        
    }
}