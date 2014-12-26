using System.Windows.Forms; //TableLayoutPanel

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

        PanelGetDate[] m_arPanels;

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            m_arPanels = new PanelGetDate[] { new PanelGetDate() };

            this.SuspendLayout();

            this.Dock = DockStyle.Fill;
            this.ColumnCount = 3; this.RowCount = 7;
            this.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

            for (int i = 0; i < this.ColumnCount; i++)
                this.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100 / this.ColumnCount));
            for (int i = 0; i < this.RowCount; i++)
                this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100 / this.RowCount));

            this.Controls.Add(m_arPanels[0], 0, 0);
            //m_panelMain.SetColumnSpan (m_arPanels[0], 2);

            this.ResumeLayout();
        }

        #endregion
    }
}
