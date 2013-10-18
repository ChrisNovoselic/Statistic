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
            this.components = new System.ComponentModel.Container();
            this.listBoxTEC = new System.Windows.Forms.ListBox();
            this.comboBoxMode = new System.Windows.Forms.ComboBox();
            this.buttonTECAdd = new System.Windows.Forms.Button();
            this.buttonTECDel = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.listBoxItem = new System.Windows.Forms.ListBox();
            this.buttonItemDel = new System.Windows.Forms.Button();
            this.buttonItemAdd = new System.Windows.Forms.Button();
            this.buttonTGDel = new System.Windows.Forms.Button();
            this.buttonTGAdd = new System.Windows.Forms.Button();
            this.listBoxTG = new System.Windows.Forms.ListBox();
            this.timerUIControl = new System.Windows.Forms.Timer(this.components);
            this.checkBoxTECInUse = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // listBoxTEC
            // 
            this.listBoxTEC.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBoxTEC.FormattingEnabled = true;
            this.listBoxTEC.Location = new System.Drawing.Point(12, 13);
            this.listBoxTEC.Name = "listBoxTEC";
            this.listBoxTEC.Size = new System.Drawing.Size(136, 173);
            this.listBoxTEC.TabIndex = 0;
            this.listBoxTEC.SelectedIndexChanged += new System.EventHandler(this.listBoxTEC_SelectedIndexChanged);
            // 
            // comboBoxMode
            // 
            this.comboBoxMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMode.FormattingEnabled = true;
            this.comboBoxMode.Location = new System.Drawing.Point(164, 12);
            this.comboBoxMode.Name = "comboBoxMode";
            this.comboBoxMode.Size = new System.Drawing.Size(136, 21);
            this.comboBoxMode.TabIndex = 4;
            this.comboBoxMode.SelectedIndexChanged += new System.EventHandler(this.comboBoxMode_SelectedIndexChanged);
            // 
            // buttonTECAdd
            // 
            this.buttonTECAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonTECAdd.Location = new System.Drawing.Point(12, 218);
            this.buttonTECAdd.Name = "buttonTECAdd";
            this.buttonTECAdd.Size = new System.Drawing.Size(66, 23);
            this.buttonTECAdd.TabIndex = 5;
            this.buttonTECAdd.Text = "Добавить";
            this.buttonTECAdd.UseVisualStyleBackColor = true;
            this.buttonTECAdd.Click += new System.EventHandler(this.buttonTECAdd_Click);
            // 
            // buttonTECDel
            // 
            this.buttonTECDel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonTECDel.Location = new System.Drawing.Point(84, 218);
            this.buttonTECDel.Name = "buttonTECDel";
            this.buttonTECDel.Size = new System.Drawing.Size(66, 23);
            this.buttonTECDel.TabIndex = 6;
            this.buttonTECDel.Text = "Удалить";
            this.buttonTECDel.UseVisualStyleBackColor = true;
            this.buttonTECDel.Click += new System.EventHandler(this.buttonTECDel_Click);
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.Location = new System.Drawing.Point(271, 251);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(89, 23);
            this.buttonOk.TabIndex = 7;
            this.buttonOk.Text = "Принять";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(369, 251);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(89, 23);
            this.buttonCancel.TabIndex = 8;
            this.buttonCancel.Text = "Отмена";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // listBoxItem
            // 
            this.listBoxItem.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBoxItem.FormattingEnabled = true;
            this.listBoxItem.Location = new System.Drawing.Point(164, 39);
            this.listBoxItem.Name = "listBoxItem";
            this.listBoxItem.Size = new System.Drawing.Size(136, 173);
            this.listBoxItem.TabIndex = 9;
            this.listBoxItem.SelectedIndexChanged += new System.EventHandler(this.listBoxItem_SelectedIndexChanged);
            // 
            // buttonItemDel
            // 
            this.buttonItemDel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonItemDel.Location = new System.Drawing.Point(236, 218);
            this.buttonItemDel.Name = "buttonItemDel";
            this.buttonItemDel.Size = new System.Drawing.Size(66, 23);
            this.buttonItemDel.TabIndex = 11;
            this.buttonItemDel.Text = "Удалить";
            this.buttonItemDel.UseVisualStyleBackColor = true;
            this.buttonItemDel.Click += new System.EventHandler(this.buttonItemDel_Click);
            // 
            // buttonItemAdd
            // 
            this.buttonItemAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonItemAdd.Location = new System.Drawing.Point(164, 218);
            this.buttonItemAdd.Name = "buttonItemAdd";
            this.buttonItemAdd.Size = new System.Drawing.Size(66, 23);
            this.buttonItemAdd.TabIndex = 10;
            this.buttonItemAdd.Text = "Добавить";
            this.buttonItemAdd.UseVisualStyleBackColor = true;
            this.buttonItemAdd.Click += new System.EventHandler(this.buttonItemAdd_Click);
            // 
            // buttonTGDel
            // 
            this.buttonTGDel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonTGDel.Location = new System.Drawing.Point(391, 217);
            this.buttonTGDel.Name = "buttonTGDel";
            this.buttonTGDel.Size = new System.Drawing.Size(66, 23);
            this.buttonTGDel.TabIndex = 14;
            this.buttonTGDel.Text = "Удалить";
            this.buttonTGDel.UseVisualStyleBackColor = true;
            this.buttonTGDel.Click += new System.EventHandler(this.buttonTGDel_Click);
            // 
            // buttonTGAdd
            // 
            this.buttonTGAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonTGAdd.Location = new System.Drawing.Point(319, 217);
            this.buttonTGAdd.Name = "buttonTGAdd";
            this.buttonTGAdd.Size = new System.Drawing.Size(66, 23);
            this.buttonTGAdd.TabIndex = 13;
            this.buttonTGAdd.Text = "Добавить";
            this.buttonTGAdd.UseVisualStyleBackColor = true;
            this.buttonTGAdd.Click += new System.EventHandler(this.buttonTGAdd_Click);
            // 
            // listBoxTG
            // 
            this.listBoxTG.AllowDrop = true;
            this.listBoxTG.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBoxTG.FormattingEnabled = true;
            this.listBoxTG.Location = new System.Drawing.Point(319, 12);
            this.listBoxTG.Name = "listBoxTG";
            this.listBoxTG.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxTG.Size = new System.Drawing.Size(136, 199);
            this.listBoxTG.TabIndex = 12;
            // 
            // timerUIControl
            // 
            this.timerUIControl.Tick += new System.EventHandler(this.timerUIControl_Tick);
            // 
            // checkBoxTECInUse
            // 
            this.checkBoxTECInUse.AutoSize = true;
            this.checkBoxTECInUse.Location = new System.Drawing.Point(12, 195);
            this.checkBoxTECInUse.Name = "checkBoxTECInUse";
            this.checkBoxTECInUse.Size = new System.Drawing.Size(99, 17);
            this.checkBoxTECInUse.TabIndex = 15;
            this.checkBoxTECInUse.Text = "Использовать";
            this.checkBoxTECInUse.UseVisualStyleBackColor = true;
            this.checkBoxTECInUse.CheckedChanged += new System.EventHandler(this.checkBoxTECInUse_CheckedChanged);
            // 
            // FormTECComponent
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(467, 282);
            this.ControlBox = false;
            this.Controls.Add(this.checkBoxTECInUse);
            this.Controls.Add(this.buttonTGDel);
            this.Controls.Add(this.buttonTGAdd);
            this.Controls.Add(this.listBoxTG);
            this.Controls.Add(this.buttonItemDel);
            this.Controls.Add(this.buttonItemAdd);
            this.Controls.Add(this.listBoxItem);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.buttonTECDel);
            this.Controls.Add(this.buttonTECAdd);
            this.Controls.Add(this.comboBoxMode);
            this.Controls.Add(this.listBoxTEC);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormTECComponent";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Настройка состава ТЭЦ, ГТП, ЩУ";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxTEC;
        private System.Windows.Forms.ComboBox comboBoxMode;
        private System.Windows.Forms.Button buttonTECAdd;
        private System.Windows.Forms.Button buttonTECDel;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.ListBox listBoxItem;
        private System.Windows.Forms.Button buttonItemDel;
        private System.Windows.Forms.Button buttonItemAdd;
        private System.Windows.Forms.Button buttonTGDel;
        private System.Windows.Forms.Button buttonTGAdd;
        private System.Windows.Forms.ListBox listBoxTG;
        private System.Windows.Forms.Timer timerUIControl;
        private System.Windows.Forms.CheckBox checkBoxTECInUse;
    }
}