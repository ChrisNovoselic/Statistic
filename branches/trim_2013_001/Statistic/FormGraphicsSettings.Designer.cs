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
            this.lblBGcolor = new System.Windows.Forms.Label();
            this.lblGRIDcolor = new System.Windows.Forms.Label();
            this.gbxType.SuspendLayout();
            this.SuspendLayout();
            // 
            // cbxScale
            // 
            this.cbxScale.AutoSize = true;
            this.cbxScale.Location = new System.Drawing.Point(4, 7);
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
            this.lblUDGcolor.Location = new System.Drawing.Point(12, 27);
            this.lblUDGcolor.Name = "lblUDGcolor";
            this.lblUDGcolor.Size = new System.Drawing.Size(140, 35);
            this.lblUDGcolor.TabIndex = 1;
            this.lblUDGcolor.Text = "Цвет УДГэ";
            this.lblUDGcolor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblUDGcolor.Click += new System.EventHandler(this.lblUDGcolor_Click);
            // 
            // lblDIVcolor
            // 
            this.lblDIVcolor.BackColor = System.Drawing.SystemColors.Control;
            this.lblDIVcolor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblDIVcolor.Location = new System.Drawing.Point(12, 62);
            this.lblDIVcolor.Name = "lblDIVcolor";
            this.lblDIVcolor.Size = new System.Drawing.Size(140, 35);
            this.lblDIVcolor.TabIndex = 2;
            this.lblDIVcolor.Text = "Цвет отклонения";
            this.lblDIVcolor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblDIVcolor.Click += new System.EventHandler(this.lblDIVcolor_Click);
            // 
            // lblPcolor
            // 
            this.lblPcolor.BackColor = System.Drawing.SystemColors.Control;
            this.lblPcolor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblPcolor.Location = new System.Drawing.Point(12, 97);
            this.lblPcolor.Name = "lblPcolor";
            this.lblPcolor.Size = new System.Drawing.Size(140, 35);
            this.lblPcolor.TabIndex = 3;
            this.lblPcolor.Text = "Цвет значения мощности";
            this.lblPcolor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblPcolor.Click += new System.EventHandler(this.lblPcolor_Click);
            // 
            // lblRECcolor
            // 
            this.lblRECcolor.BackColor = System.Drawing.SystemColors.Control;
            this.lblRECcolor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblRECcolor.Location = new System.Drawing.Point(12, 132);
            this.lblRECcolor.Name = "lblRECcolor";
            this.lblRECcolor.Size = new System.Drawing.Size(140, 35);
            this.lblRECcolor.TabIndex = 4;
            this.lblRECcolor.Text = "Цвет рекомендации";
            this.lblRECcolor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblRECcolor.Click += new System.EventHandler(this.lblRECcolor_Click);
            // 
            // gbxType
            // 
            this.gbxType.Controls.Add(this.rbtnBar);
            this.gbxType.Controls.Add(this.rbtnLine);
            this.gbxType.Location = new System.Drawing.Point(12, 275);
            this.gbxType.Name = "gbxType";
            this.gbxType.Size = new System.Drawing.Size(140, 70);
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
            // lblBGcolor
            // 
            this.lblBGcolor.BackColor = System.Drawing.SystemColors.Control;
            this.lblBGcolor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblBGcolor.Location = new System.Drawing.Point(12, 167);
            this.lblBGcolor.Name = "lblBGcolor";
            this.lblBGcolor.Size = new System.Drawing.Size(140, 35);
            this.lblBGcolor.TabIndex = 6;
            this.lblBGcolor.Text = "Цвет фона";
            this.lblBGcolor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblBGcolor.Click += new System.EventHandler(this.lblBGcolor_Click);
            // 
            // lblGRIDcolor
            // 
            this.lblGRIDcolor.BackColor = System.Drawing.SystemColors.Control;
            this.lblGRIDcolor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblGRIDcolor.Location = new System.Drawing.Point(12, 202);
            this.lblGRIDcolor.Name = "lblGRIDcolor";
            this.lblGRIDcolor.Size = new System.Drawing.Size(140, 35);
            this.lblGRIDcolor.TabIndex = 7;
            this.lblGRIDcolor.Text = "Цвет сетки";
            this.lblGRIDcolor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblGRIDcolor.Click += new System.EventHandler(this.lblGRIDcolor_Click);
            // 
            // FormGraphicsSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(164, 357);
            this.Controls.Add(this.lblGRIDcolor);
            this.Controls.Add(this.lblBGcolor);
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
        private System.Windows.Forms.Label lblBGcolor;
        private System.Windows.Forms.Label lblGRIDcolor;
    }
}