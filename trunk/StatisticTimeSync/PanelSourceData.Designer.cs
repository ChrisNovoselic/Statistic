using System.Windows.Forms; //TableLayoutPanel
using System;

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

            int i = -1;

            m_arPanels = new PanelGetDate[INDEX_SOURCE_GETDATE.Length];
            for (i = 0; i < m_arPanels.Length; i++)
                m_arPanels [i] = new PanelGetDate ();

            //Только для панели с эталонным серверои БД
            m_arPanels[0].DelegateEtalonGetDate = new HClassLibrary.DelegateDateFunc(recievedEtalonDate);
            //Для панелей с любыми серверами БД
            for (i = 0; i < m_arPanels.Length; i++) {
                EvtGetDate += new HClassLibrary.DelegateObjectFunc(m_arPanels[i].OnEvtGetDate);
                EvtEtalonDate += new HClassLibrary.DelegateDateFunc(m_arPanels[i].OnEvtEtalonDate);
            }

            this.SuspendLayout();

            this.Dock = DockStyle.Fill;
            this.ColumnCount = 3; this.RowCount = 7;
            this.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

            for (i = 0; i < this.ColumnCount; i++)
                this.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100 / this.ColumnCount));
            for (i = 0; i < this.RowCount; i++)
                this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100 / this.RowCount));

            this.Controls.Add(m_arPanels[0], 0, 0);

            int indx = -1
                , col = -1
                , row = -1;
            for (i = 1; i < m_arPanels.Length; i++) {
                indx = i;
                //if (! (indx < this.RowCount))
                    indx += (int)(indx / this.RowCount);
                //else ;

                col = (int)(indx / this.RowCount);
                row = indx % (this.RowCount - 0);
                if (row == 0) row = 1; else ;
                this.Controls.Add(m_arPanels[i], col, row); 
            }

            this.ResumeLayout();
        }

        #endregion
    }
}
