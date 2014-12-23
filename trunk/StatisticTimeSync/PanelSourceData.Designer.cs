namespace StatisticTimeSync
{
    partial class PanelSourceData
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.Dock = System.Windows.Forms.DockStyle.Fill;

            this.m_checkBoxTurnOn = new System.Windows.Forms.CheckBox();
            this.m_comboBoxSourceData = new System.Windows.Forms.ComboBox();
            this.m_labelTime = new System.Windows.Forms.Label();
            this.m_labelDiff = new System.Windows.Forms.Label();

            this.ColumnCount = 2; this.RowCount = 2;
            this.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;

            for (int i = 0; i < this.ColumnCount; i ++)
                this.ColumnStyles.Add (new System.Windows.Forms.ColumnStyle ( System.Windows.Forms.SizeType.Percent, 50F));
            for (int i = 0; i < this.RowCount; i++)
                this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));

            this.Controls.Add(m_checkBoxTurnOn, 0, 1);
            this.Controls.Add(m_comboBoxSourceData, 0, 0);
            this.Controls.Add(m_labelTime, 1, 1);
            this.Controls.Add(m_labelDiff, 1, 0);

            this.SuspendLayout();
            // 
            // m_checkBoxTurnOn
            // 
            this.m_checkBoxTurnOn.AutoSize = true;
            //this.m_checkBoxTurnOn.Location = new System.Drawing.Point(0, 0);
            this.m_checkBoxTurnOn.Name = "m_checkBoxTurnOn";
            //this.m_checkBoxTurnOn.Size = new System.Drawing.Size(104, 24);
            this.m_checkBoxTurnOn.TabIndex = 0;
            this.m_checkBoxTurnOn.Text = "Включено";
            this.m_checkBoxTurnOn.UseVisualStyleBackColor = true;
            this.m_checkBoxTurnOn.CheckedChanged += new System.EventHandler(checkBoxTurnOn_CheckedChanged);
            this.m_checkBoxTurnOn.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            this.m_checkBoxTurnOn.Enabled = false;
            // 
            // m_comboBoxSourceData
            // 
            this.m_comboBoxSourceData.FormattingEnabled = true;
            //this.m_comboBoxSourceData.Location = new System.Drawing.Point(0, 0);
            this.m_comboBoxSourceData.Name = "m_comboBoxSourceData";
            this.m_comboBoxSourceData.Size = new System.Drawing.Size(121, 21);
            this.m_comboBoxSourceData.TabIndex = 0;
            this.m_comboBoxSourceData.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_comboBoxSourceData.Enabled = ! this.m_checkBoxTurnOn.Checked;
            this.m_comboBoxSourceData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_comboBoxSourceData.SelectedIndexChanged += new System.EventHandler(comboBoxSourceData_SelectedIndexChanged);
            this.m_comboBoxSourceData.Items.Add (@"[Нет]");
            this.m_comboBoxSourceData.SelectedIndex = 0;
            // 
            // m_labelTime
            // 
            this.m_labelTime.AutoSize = true;
            //this.m_labelTime.Location = new System.Drawing.Point(0, 0);
            //this.m_labelTime.Size = new System.Drawing.Size(100, 23);
            this.m_labelTime.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_labelTime.Name = "m_labelTime";            
            this.m_labelTime.TabIndex = 0;
            this.m_labelTime.Text = "HH:mm:ss.ccc";
            this.m_labelTime.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.m_labelTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // m_labelDiff
            // 
            this.m_labelDiff.AutoSize = true;
            //this.m_labelDiff.Location = new System.Drawing.Point(0, 0);
            //this.m_labelDiff.Size = new System.Drawing.Size(100, 23);
            this.m_labelDiff.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_labelDiff.Name = "m_labelDiff";            
            this.m_labelDiff.TabIndex = 0;
            this.m_labelDiff.Text = "HH:mm:ss.ccc";
            this.m_labelDiff.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.m_labelDiff.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.CheckBox m_checkBoxTurnOn;
        private System.Windows.Forms.ComboBox m_comboBoxSourceData;
        private System.Windows.Forms.Label m_labelTime;
        private System.Windows.Forms.Label m_labelDiff;
    }
}
