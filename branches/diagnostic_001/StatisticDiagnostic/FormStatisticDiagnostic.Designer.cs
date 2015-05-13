namespace StatisticDiagnostic
{
    partial class FormStatisticDiagnostic
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

        PanelStatisticDiagnostic panelMain;
        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            panelMain = new PanelStatisticDiagnostic();

            this.SuspendLayout();

            this.Controls.Add(panelMain);

            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Name = "FormStatisticDiagnostic";
            this.Text = "StatisticDiagnostic";

            this.ResumeLayout(false);

            this.Load += new System.EventHandler(FormStatisticDiagnostic_Load);
            this.Activated += new System.EventHandler(FormStatisticDiagnostic_Activate);
            this.Deactivate += new System.EventHandler(FormStatisticDiagnostic_Deactivate);
        }
    }
        #endregion
}
