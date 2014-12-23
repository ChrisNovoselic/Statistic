using System.Windows.Forms; //TableLayoutPanel

namespace StatisticTimeSync
{
    partial class FormStatisticTimeSync
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

        TableLayoutPanel m_panelMain;
        PanelSourceData [] m_arPanels;
        
        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            m_panelMain = new TableLayoutPanel ();
            m_arPanels = new PanelSourceData [] { new PanelSourceData () };

            this.SuspendLayout();

            m_panelMain.Dock = DockStyle.Fill;
            m_panelMain.ColumnCount = 3; m_panelMain.RowCount = 7;
            m_panelMain.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

            for (int i = 0; i < m_panelMain.ColumnCount; i++)
                m_panelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100 / m_panelMain.ColumnCount));
            for (int i = 0; i < m_panelMain.RowCount; i++)
                m_panelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100 / m_panelMain.RowCount));

            m_panelMain.Controls.Add(m_arPanels[0], 0, 0);
            //m_panelMain.SetColumnSpan (m_arPanels[0], 2);

            this.Controls.Add(m_panelMain);

            // 
            // FormStatisticTimeSync
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Name = "FormStatisticTimeSync";
            this.Text = "StatisticTimeSync";

            this.ResumeLayout(false);

            this.Load += new System.EventHandler(FormStatisticTimeSync_Load);
        }

        #endregion

    }
}

