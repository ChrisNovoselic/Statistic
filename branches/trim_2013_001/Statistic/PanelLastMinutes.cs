using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

using StatisticCommon;

namespace Statistic
{
    public partial class PanelLastMinutes : TableLayoutPanel
    {
        List <Label> m_listLabelDateTime;
        private List<PanelTecLastMinutes> m_listTECLastMinutes;

        public PanelLastMinutes(List<TEC> listTec)
        {
            InitializeComponent();

            this.Dock = DockStyle.Fill;

            this.BorderStyle = BorderStyle.None; //BorderStyle.FixedSingle

            this.RowCount = 24 + 1;

            //Пустая ячейка
            this.Controls.Add(new HLabel(new Point(-1, -1), new Size(-1, -1), Color.Black, Color.Gray, 12F, ContentAlignment.MiddleLeft), 0, 0);
            this.RowStyles.Add(new RowStyle(SizeType.Percent, (float)Math.Round((double)100 / this.RowCount, 1)));

            m_listLabelDateTime = new List<Label> ();

            DateTime datetimeNow = DateTime.Now.Date;
            datetimeNow = datetimeNow.AddMinutes(59);
            for (int i = 0; i < 24; i ++) {
                m_listLabelDateTime.Add (new HLabel (new Point (-1, -1), new Size (-1, -1), Color.Black, Color.LightGoldenrodYellow, 12F, ContentAlignment.MiddleLeft));
                this.RowStyles.Add(new RowStyle(SizeType.Percent, (float)Math.Round((double)100 / this.RowCount, 1)));
                //this.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                this.Controls.Add(m_listLabelDateTime[m_listLabelDateTime.Count - 1], 0, i + 1);

                m_listLabelDateTime[m_listLabelDateTime.Count - 1].Text = datetimeNow.ToString(@"dd.MM.yyyy HH:mm");
                datetimeNow = datetimeNow.AddHours(1);
            }

            this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10));

            m_listTECLastMinutes = new List<PanelTecLastMinutes>();
            PanelTecLastMinutes ptlm;

            this.ColumnCount = listTec.Count;
            for (int i = 0; i < listTec.Count; i++)
            {
                ptlm = new PanelTecLastMinutes(listTec[i]);
                //this.Controls.Add(ptlm, i, 0);

                m_listTECLastMinutes.Add(ptlm);

                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100 / listTec.Count));
            }
        }

        public PanelLastMinutes(IContainer container, List<TEC> listTec)
            : this(listTec)
        {
            container.Add(this);
        }

        partial class PanelTecLastMinutes
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
                components = new System.ComponentModel.Container();
            }

            #endregion
        }

        public partial class PanelTecLastMinutes : TableLayoutPanel
        {
            TEC m_tec;

            public PanelTecLastMinutes(TEC tec)
            {
                InitializeComponent();

                m_tec = tec;
                Initialize();
            }

            public PanelTecLastMinutes(IContainer container, TEC tec)
                : this(tec)
            {
                container.Add(this);
            }

            private void Initialize()
            {
            }
        }
    }
}
