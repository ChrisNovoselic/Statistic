namespace StatisticDiagnostic1
{
    partial class FormDiagnostic
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

        #region Код, автоматически созданный конструктором форм Windows

        PanelStatisticDiagnostic1 panelMain;

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panelMain = new StatisticDiagnostic1.PanelStatisticDiagnostic1(this.components);
            this.SuspendLayout();
            // 
            // panelMain
            //
            this.panelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Name = "panelMain";
            this.panelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.panelMain.Size = new System.Drawing.Size(1041, 600);
            this.panelMain.TabIndex = 0;
            
            // 
            // FormDiagnostic
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1041, 600);
            this.Controls.Add(panelMain);
            this.Name = "FormDiagnostic";
            this.Text = "FormStatisticDiagnostic";
            this.Activated += new System.EventHandler(this.FormDiagnostic_Activate);
            this.Deactivate += new System.EventHandler(this.FormDiagnostic_Deactivate);
            this.Load += new System.EventHandler(this.FormDiagnostic_Load);            
            this.ResumeLayout(false);
        }

        #endregion      
    }
}

