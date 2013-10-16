namespace Statistic
{
    partial class FormTECComponent
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
            this.listBoxItem = new System.Windows.Forms.ListBox();
            this.dataGridViewProp = new System.Windows.Forms.DataGridView();
            this.ColumnTG = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnProp1 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.ColumnProp2 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.comboBoxMode = new System.Windows.Forms.ComboBox();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.buttonDel = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewProp)).BeginInit();
            this.SuspendLayout();
            // 
            // listBoxItem
            // 
            this.listBoxItem.FormattingEnabled = true;
            this.listBoxItem.Location = new System.Drawing.Point(12, 52);
            this.listBoxItem.Name = "listBoxItem";
            this.listBoxItem.Size = new System.Drawing.Size(136, 160);
            this.listBoxItem.TabIndex = 0;
            // 
            // dataGridViewProp
            // 
            this.dataGridViewProp.AllowUserToOrderColumns = true;
            this.dataGridViewProp.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewProp.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnTG,
            this.ColumnProp1,
            this.ColumnProp2});
            this.dataGridViewProp.Location = new System.Drawing.Point(163, 13);
            this.dataGridViewProp.Name = "dataGridViewProp";
            this.dataGridViewProp.Size = new System.Drawing.Size(264, 225);
            this.dataGridViewProp.TabIndex = 3;
            // 
            // ColumnTG
            // 
            this.ColumnTG.Frozen = true;
            this.ColumnTG.HeaderText = "Эн./блок";
            this.ColumnTG.Name = "ColumnTG";
            this.ColumnTG.ReadOnly = true;
            // 
            // ColumnProp1
            // 
            this.ColumnProp1.Frozen = true;
            this.ColumnProp1.HeaderText = "ГТП";
            this.ColumnProp1.Name = "ColumnProp1";
            this.ColumnProp1.ReadOnly = true;
            // 
            // ColumnProp2
            // 
            this.ColumnProp2.Frozen = true;
            this.ColumnProp2.HeaderText = "ЩУ";
            this.ColumnProp2.Name = "ColumnProp2";
            this.ColumnProp2.ReadOnly = true;
            // 
            // comboBoxMode
            // 
            this.comboBoxMode.FormattingEnabled = true;
            this.comboBoxMode.Location = new System.Drawing.Point(12, 13);
            this.comboBoxMode.Name = "comboBoxMode";
            this.comboBoxMode.Size = new System.Drawing.Size(136, 21);
            this.comboBoxMode.TabIndex = 4;
            // 
            // buttonAdd
            // 
            this.buttonAdd.Location = new System.Drawing.Point(12, 218);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(66, 23);
            this.buttonAdd.TabIndex = 5;
            this.buttonAdd.Text = "Добавить";
            this.buttonAdd.UseVisualStyleBackColor = true;
            // 
            // buttonDel
            // 
            this.buttonDel.Location = new System.Drawing.Point(84, 218);
            this.buttonDel.Name = "buttonDel";
            this.buttonDel.Size = new System.Drawing.Size(66, 23);
            this.buttonDel.TabIndex = 6;
            this.buttonDel.Text = "Удалить";
            this.buttonDel.UseVisualStyleBackColor = true;
            // 
            // buttonOk
            // 
            this.buttonOk.Location = new System.Drawing.Point(240, 251);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(89, 23);
            this.buttonOk.TabIndex = 7;
            this.buttonOk.Text = "Принять";
            this.buttonOk.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(338, 251);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(89, 23);
            this.buttonCancel.TabIndex = 8;
            this.buttonCancel.Text = "Отмена";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // FormTECComponent
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(434, 282);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.buttonDel);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.comboBoxMode);
            this.Controls.Add(this.dataGridViewProp);
            this.Controls.Add(this.listBoxItem);
            this.Name = "FormTECComponent";
            this.Text = "Настройка состава ТЭЦ, ГТП, ЩУ";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewProp)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxItem;
        private System.Windows.Forms.DataGridView dataGridViewProp;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnTG;
        private System.Windows.Forms.DataGridViewComboBoxColumn ColumnProp1;
        private System.Windows.Forms.DataGridViewComboBoxColumn ColumnProp2;
        private System.Windows.Forms.ComboBox comboBoxMode;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Button buttonDel;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
    }
}