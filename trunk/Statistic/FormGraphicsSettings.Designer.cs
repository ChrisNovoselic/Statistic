namespace Statistic
{
    partial class FormGraphicsSettings
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
            this.cbxScale = new System.Windows.Forms.CheckBox();
            this.lblUDGcolor = new System.Windows.Forms.Label();
            this.lblDIVcolor = new System.Windows.Forms.Label();
            this.lblPcolor = new System.Windows.Forms.Label();
            this.lblRECcolor = new System.Windows.Forms.Label();
            this.gbxType = new System.Windows.Forms.GroupBox();
            this.rbtnBar = new System.Windows.Forms.RadioButton();
            this.rbtnLine = new System.Windows.Forms.RadioButton();
            this.lblBG_ASKUE_color = new System.Windows.Forms.Label();
            this.lblGRIDcolor = new System.Windows.Forms.Label();
            this.groupBoxSourceData = new System.Windows.Forms.GroupBox();
            this.rbtnSourceData_COSTUMIZE = new System.Windows.Forms.RadioButton();
            this.rbtnSourceData_ASKUE = new System.Windows.Forms.RadioButton();
            this.rbtnSourceData_SOTIASSO = new System.Windows.Forms.RadioButton();
            this.lblBG_SOTIASSO_color = new System.Windows.Forms.Label();
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
            // 
            // lblUDGcolor
            // 
            this.lblUDGcolor.BackColor = System.Drawing.SystemColors.Control;
            this.lblUDGcolor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblUDGcolor.Location = new System.Drawing.Point(12, 13);
            this.lblUDGcolor.Name = "lblUDGcolor";
            this.lblUDGcolor.Size = new System.Drawing.Size(140, 29);
            this.lblUDGcolor.TabIndex = 1;
            this.lblUDGcolor.Text = "Цвет УДГэ";
            this.lblUDGcolor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblUDGcolor.Click += new System.EventHandler(this.lblUDGcolor_Click);
            // 
            // lblDIVcolor
            // 
            this.lblDIVcolor.BackColor = System.Drawing.SystemColors.Control;
            this.lblDIVcolor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblDIVcolor.Location = new System.Drawing.Point(12, 41);
            this.lblDIVcolor.Name = "lblDIVcolor";
            this.lblDIVcolor.Size = new System.Drawing.Size(140, 29);
            this.lblDIVcolor.TabIndex = 2;
            this.lblDIVcolor.Text = "Цвет отклонения";
            this.lblDIVcolor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblDIVcolor.Click += new System.EventHandler(this.lblDIVcolor_Click);
            // 
            // lblPcolor
            // 
            this.lblPcolor.BackColor = System.Drawing.SystemColors.Control;
            this.lblPcolor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblPcolor.Location = new System.Drawing.Point(12, 69);
            this.lblPcolor.Name = "lblPcolor";
            this.lblPcolor.Size = new System.Drawing.Size(140, 29);
            this.lblPcolor.TabIndex = 3;
            this.lblPcolor.Text = "Цвет значения мощности";
            this.lblPcolor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblPcolor.Click += new System.EventHandler(this.lblPcolor_Click);
            // 
            // lblRECcolor
            // 
            this.lblRECcolor.BackColor = System.Drawing.SystemColors.Control;
            this.lblRECcolor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblRECcolor.Location = new System.Drawing.Point(12, 97);
            this.lblRECcolor.Name = "lblRECcolor";
            this.lblRECcolor.Size = new System.Drawing.Size(140, 29);
            this.lblRECcolor.TabIndex = 4;
            this.lblRECcolor.Text = "Цвет рекомендации";
            this.lblRECcolor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblRECcolor.Click += new System.EventHandler(this.lblRECcolor_Click);
            // 
            // gbxType
            // 
            this.gbxType.Controls.Add(this.rbtnBar);
            this.gbxType.Controls.Add(this.rbtnLine);
            this.gbxType.Location = new System.Drawing.Point(167, 34);
            this.gbxType.Name = "gbxType";
            this.gbxType.Size = new System.Drawing.Size(173, 70);
            this.gbxType.TabIndex = 5;
            this.gbxType.TabStop = false;
            this.gbxType.Text = "Тип графиков";
            // 
            // rbtnBar
            // 
            this.rbtnBar.AutoSize = true;
            this.rbtnBar.Checked = true;
            this.rbtnBar.Location = new System.Drawing.Point(6, 19);
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
            this.rbtnLine.Location = new System.Drawing.Point(6, 42);
            this.rbtnLine.Name = "rbtnLine";
            this.rbtnLine.Size = new System.Drawing.Size(75, 17);
            this.rbtnLine.TabIndex = 0;
            this.rbtnLine.Text = "линейный";
            this.rbtnLine.UseVisualStyleBackColor = true;
            this.rbtnLine.CheckedChanged += new System.EventHandler(this.rbtnLine_CheckedChanged);
            // 
            // lblBG_ASKUE_color
            // 
            this.lblBG_ASKUE_color.BackColor = System.Drawing.SystemColors.Control;
            this.lblBG_ASKUE_color.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblBG_ASKUE_color.Location = new System.Drawing.Point(12, 125);
            this.lblBG_ASKUE_color.Name = "lblBG_ASKUE_color";
            this.lblBG_ASKUE_color.Size = new System.Drawing.Size(140, 29);
            this.lblBG_ASKUE_color.TabIndex = 6;
            this.lblBG_ASKUE_color.Text = "Цвет фона (АСКУЭ)";
            this.lblBG_ASKUE_color.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblBG_ASKUE_color.Click += new System.EventHandler(this.lblBG_ASKUE_color_Click);
            // 
            // lblBG_SOTIASSO_color
            // 
            this.lblBG_SOTIASSO_color.BackColor = System.Drawing.SystemColors.Control;
            this.lblBG_SOTIASSO_color.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblBG_SOTIASSO_color.Location = new System.Drawing.Point(12, 153);
            this.lblBG_SOTIASSO_color.Name = "lblBG_SOTIASSO_color";
            this.lblBG_SOTIASSO_color.Size = new System.Drawing.Size(140, 29);
            this.lblBG_SOTIASSO_color.TabIndex = 9;
            this.lblBG_SOTIASSO_color.Text = "Цвет фона (СОТИАССО)";
            this.lblBG_SOTIASSO_color.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblBG_SOTIASSO_color.Click += new System.EventHandler(this.lblBG_SOTIASSO_color_Click);
            // 
            // lblGRIDcolor
            // 
            this.lblGRIDcolor.BackColor = System.Drawing.SystemColors.Control;
            this.lblGRIDcolor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblGRIDcolor.Location = new System.Drawing.Point(12, 181);
            this.lblGRIDcolor.Name = "lblGRIDcolor";
            this.lblGRIDcolor.Size = new System.Drawing.Size(140, 29);
            this.lblGRIDcolor.TabIndex = 7;
            this.lblGRIDcolor.Text = "Цвет сетки";
            this.lblGRIDcolor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblGRIDcolor.Click += new System.EventHandler(this.lblGRIDcolor_Click);
            // 
            // groupBoxSourceData
            // 
            this.groupBoxSourceData.Controls.Add(this.rbtnSourceData_COSTUMIZE);
            this.groupBoxSourceData.Controls.Add(this.rbtnSourceData_ASKUE);
            this.groupBoxSourceData.Controls.Add(this.rbtnSourceData_SOTIASSO);
            this.groupBoxSourceData.Location = new System.Drawing.Point(167, 110);
            this.groupBoxSourceData.Name = "groupBoxSourceData";
            this.groupBoxSourceData.Size = new System.Drawing.Size(173, 100);
            this.groupBoxSourceData.TabIndex = 8;
            this.groupBoxSourceData.TabStop = false;
            this.groupBoxSourceData.Text = "Источники данных графиков";
            // 
            // rbtnSourceData_COSTUMIZE
            // 
            this.rbtnSourceData_COSTUMIZE.AutoCheck = false;
            this.rbtnSourceData_COSTUMIZE.AutoSize = true;
            //this.rbtnSourceData_COSTUMIZE.Checked = true;
            this.rbtnSourceData_COSTUMIZE.Location = new System.Drawing.Point(6, 74);
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
            this.rbtnSourceData_ASKUE.Location = new System.Drawing.Point(6, 19);
            this.rbtnSourceData_ASKUE.Name = "rbtnSourceData_ASKUE";
            this.rbtnSourceData_ASKUE.Size = new System.Drawing.Size(61, 17);
            this.rbtnSourceData_ASKUE.TabIndex = 1;
            this.rbtnSourceData_ASKUE.Text = "АИСКУЭ";
            this.rbtnSourceData_ASKUE.UseVisualStyleBackColor = true;
            this.rbtnSourceData_ASKUE.Click += new System.EventHandler(this.rbtnSourceData_ASKUE_Click);
            // 
            // rbtnSourceData_SOTIASSO
            // 
            this.rbtnSourceData_SOTIASSO.AutoCheck = false;
            this.rbtnSourceData_SOTIASSO.AutoSize = true;
            //this.rbtnSourceData_SOTIASSO.Checked = true;
            this.rbtnSourceData_SOTIASSO.Location = new System.Drawing.Point(6, 46);
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
            this.Controls.Add(this.lblBG_SOTIASSO_color);
            this.Controls.Add(this.groupBoxSourceData);
            this.Controls.Add(this.lblGRIDcolor);
            this.Controls.Add(this.lblBG_ASKUE_color);
            this.Controls.Add(this.gbxType);
            this.Controls.Add(this.lblRECcolor);
            this.Controls.Add(this.lblPcolor);
            this.Controls.Add(this.lblDIVcolor);
            this.Controls.Add(this.lblUDGcolor);
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

        private System.Windows.Forms.CheckBox cbxScale;
        private System.Windows.Forms.Label lblUDGcolor;
        private System.Windows.Forms.Label lblDIVcolor;
        private System.Windows.Forms.Label lblPcolor;
        private System.Windows.Forms.Label lblRECcolor;
        private System.Windows.Forms.GroupBox gbxType;
        private System.Windows.Forms.RadioButton rbtnBar;
        private System.Windows.Forms.RadioButton rbtnLine;
        private System.Windows.Forms.Label lblBG_ASKUE_color;
        private System.Windows.Forms.Label lblGRIDcolor;
        private System.Windows.Forms.GroupBox groupBoxSourceData;
        private System.Windows.Forms.RadioButton rbtnSourceData_COSTUMIZE;
        private System.Windows.Forms.RadioButton rbtnSourceData_ASKUE;
        private System.Windows.Forms.RadioButton rbtnSourceData_SOTIASSO;
        private System.Windows.Forms.Label lblBG_SOTIASSO_color;
    }
}