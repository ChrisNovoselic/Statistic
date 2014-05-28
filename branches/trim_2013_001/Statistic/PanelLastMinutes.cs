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

        enum INDEX_LABEL : int { NAME_TEC, NAME_COMPONENT, VALUE_COMPONENT, DATETIME, COUNT_INDEX_LABEL };
        static HLabelStyles[] s_arLabelStyles = {new HLabelStyles(Color.Black, Color.Gray, 14F, ContentAlignment.MiddleCenter),
                                                new HLabelStyles(Color.Black, Color.Gray, 12F, ContentAlignment.MiddleCenter),
                                                new HLabelStyles(Color.LimeGreen, Color.Black, 12F, ContentAlignment.MiddleCenter),
                                                new HLabelStyles(Color.Black, Color.LightGoldenrodYellow, 12F, ContentAlignment.MiddleCenter)};
        static int COUNT_FIXED_ROWS = (int)INDEX_LABEL.NAME_COMPONENT + 1;
        static RowStyle fRowStyle () { return new RowStyle(SizeType.Percent, (float)Math.Round((double)100 / (24 + COUNT_FIXED_ROWS), 6)); }

        public PanelLastMinutes(List<TEC> listTec)
        {
            int i = -1;
            
            InitializeComponent();

            this.Dock = DockStyle.Fill;

            this.BorderStyle = BorderStyle.None; //BorderStyle.FixedSingle

            this.RowCount = 1;

            //Создание панели с дата/время
            TableLayoutPanel panelDateTime = new TableLayoutPanel();
            panelDateTime.Dock = DockStyle.Fill;
            panelDateTime.BorderStyle = BorderStyle.None; //BorderStyle.FixedSingle
            panelDateTime.ColumnCount = 1;
            panelDateTime.RowCount = 24 + COUNT_FIXED_ROWS; //Наименования: ТЭЦ + компонент ТЭЦ

            //Пустая ячейка (дата)
            panelDateTime.Controls.Add(new HLabel(s_arLabelStyles[(int)INDEX_LABEL.NAME_TEC]), 0, 0);
            panelDateTime.SetRowSpan(panelDateTime.Controls[0], COUNT_FIXED_ROWS);

            m_listLabelDateTime = new List<Label> ();

            DateTime datetimeNow = DateTime.Now.Date;
            datetimeNow = datetimeNow.AddMinutes(59);
            for (i = 0; i < 24; i ++) {
                m_listLabelDateTime.Add(HLabel.createLabel(datetimeNow.ToString(@"HH:mm"), s_arLabelStyles[(int)INDEX_LABEL.DATETIME]));

                panelDateTime.Controls.Add(m_listLabelDateTime[m_listLabelDateTime.Count - 1], 0, i + COUNT_FIXED_ROWS);

                datetimeNow = datetimeNow.AddHours(1);
            }

            for (i = 0; i < (24 + COUNT_FIXED_ROWS - 1); i++)
            {
                panelDateTime.RowStyles.Add(fRowStyle());
            }

            int iPercentColDatetime = 10;
            this.Controls.Add(panelDateTime, 0, 0);
            this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, iPercentColDatetime));

            int iCountSubColumns = 0;
            this.ColumnCount = listTec.Count + 1;
            for (i = 0; i < listTec.Count; i++)
            {
                this.Controls.Add(new PanelTecLastMinutes(listTec[i]), i + 1, 0);
                iCountSubColumns += ((PanelTecLastMinutes)this.Controls [i + 1]).m_iCountTECComponent; //Слева столбец дата/время
            }

            //Размеры столбцов после создания столбцов, т.к.
            //кол-во "подстолбцов" в столбцах до их создания неизвестно
            for (i = 0; i < listTec.Count; i++)
            {
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, (100 - iPercentColDatetime) / iCountSubColumns * ((PanelTecLastMinutes)this.Controls[i + 1]).m_iCountTECComponent));
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
            private TEC m_tec;
            public int m_iCountTECComponent;

            private Dictionary<int, Label[]> m_dictLabelVal;

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
                int i = -1;
                m_dictLabelVal = new Dictionary<int, Label[]>();

                this.Dock = DockStyle.Fill;
                this.BorderStyle = BorderStyle.None; //BorderStyle.FixedSingle
                this.RowCount = 24 + COUNT_FIXED_ROWS;

                //Добавить наименование станции
                Label lblNameTEC = HLabel.createLabel(m_tec.name, PanelLastMinutes.s_arLabelStyles[(int)INDEX_LABEL.NAME_TEC]);
                this.Controls.Add(lblNameTEC, 0, 0);
                
                foreach (TECComponent g in m_tec.list_TECComponents)
                {
                    if ((g.m_id > 100) && (g.m_id < 500))
                    {
                        //Добавить наименование ГТП
                        this.Controls.Add(HLabel.createLabel(g.name_shr.Split (' ')[1], PanelLastMinutes.s_arLabelStyles[(int)INDEX_LABEL.NAME_COMPONENT]), m_iCountTECComponent, COUNT_FIXED_ROWS - 1);

                        //Память под ячейки со значениями
                        m_dictLabelVal.Add(g.m_id, new Label[24]);
                        
                        for (i = 0; i < 24; i ++)
                        {
                            m_dictLabelVal[g.m_id][i] = HLabel.createLabel (0.ToString (@"F2"), PanelLastMinutes.s_arLabelStyles[(int)INDEX_LABEL.VALUE_COMPONENT]);
                            this.Controls.Add(m_dictLabelVal[g.m_id][i], m_iCountTECComponent, i + COUNT_FIXED_ROWS);
                        }

                        m_iCountTECComponent++;
                    }
                    else
                        ;
                }

                for (i = 0; i < (24 + COUNT_FIXED_ROWS - 1); i++)
                {
                    this.RowStyles.Add(PanelLastMinutes.fRowStyle());
                }

                for (i = 0; i < m_iCountTECComponent; i++)
                {
                    this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100 / m_iCountTECComponent));
                }

                this.SetColumnSpan(lblNameTEC, m_iCountTECComponent);
            }
        }
    }
}
