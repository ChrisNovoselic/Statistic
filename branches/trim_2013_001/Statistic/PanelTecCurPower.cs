using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

using StatisticCommon;

namespace Statistic
{
    public partial class PanelTecCurPower : TableLayoutPanel
    {
        enum INDEX_LABEL : int { NAME, DATETIME, VALUE_TOTAL, NAME_COMPONENT, NAME_TG, VALUE_TG, COUNT_INDEX_LABEL};
        const int COUNT_FIXED_ROWS = 3;
        
        Label [] m_arLabel;
        Dictionary<int, Label> m_dictLabelVal;
        HLabelStyles[] m_arLabelStyles;

        public TEC m_tec;

        private System.Threading.Timer m_timerCurrent;
        
        public PanelTecCurPower(TEC tec)
        {
            InitializeComponent();

            m_tec = tec;
            Initialize ();            
        }

        public PanelTecCurPower(IContainer container, TEC tec) : this (tec)
        {
            container.Add(this);
        }

        private void Initialize () {
            int i = -1;

            m_arLabelStyles = new HLabelStyles[(int)INDEX_LABEL.COUNT_INDEX_LABEL];
            m_arLabelStyles[(int)INDEX_LABEL.NAME] = new HLabelStyles(Color.White, Color.Blue, 22F, ContentAlignment.MiddleCenter);
            m_arLabelStyles[(int)INDEX_LABEL.DATETIME] = new HLabelStyles(Color.LimeGreen, Color.Gray, 24F, ContentAlignment.MiddleCenter);
            m_arLabelStyles[(int)INDEX_LABEL.VALUE_TOTAL] = new HLabelStyles(Color.LimeGreen, Color.Black, 24F, ContentAlignment.MiddleCenter);
            m_arLabelStyles[(int)INDEX_LABEL.NAME_COMPONENT] = new HLabelStyles(Color.Yellow, Color.Green, 14F, ContentAlignment.TopLeft);
            m_arLabelStyles[(int)INDEX_LABEL.NAME_TG] = new HLabelStyles(Color.LightSteelBlue, Color.Yellow, 14F, ContentAlignment.MiddleLeft);
            m_arLabelStyles[(int)INDEX_LABEL.VALUE_TG] = new HLabelStyles(Color.LimeGreen, Color.Black, 14F, ContentAlignment.MiddleCenter);

            m_dictLabelVal = new Dictionary<int, Label>();
            m_arLabel = new Label[(int)INDEX_LABEL.VALUE_TOTAL + 1];

            this.Dock = DockStyle.Fill;
            //Свойства колонок
            this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            //Видимая граница для отладки
            this.BorderStyle = BorderStyle.None; //BorderStyle.FixedSingle;

            //Наименование ТЭЦ, Дата/время, Значение для всех ГТП/ТГ
            for (i = (int)INDEX_LABEL.NAME; i < (int)INDEX_LABEL.NAME_COMPONENT; i++)
            {
                string cntnt = string.Empty;
                switch (i)
                {
                    case (int)INDEX_LABEL.NAME:
                        cntnt = m_tec.name;
                        break;
                    case (int)INDEX_LABEL.DATETIME:
                        cntnt = @"--:--:--";
                        break;
                    case (int)INDEX_LABEL.VALUE_TOTAL:
                        cntnt = 0.ToString("F2");
                        break;
                    default:
                        break;
                }
                m_arLabel[i] = HLabel.createLabel(cntnt, m_arLabelStyles[i]);
                this.Controls.Add(m_arLabel[i], 0, i);
                this.SetColumnSpan(m_arLabel[i], 3);
            }

            foreach (TECComponent g in m_tec.list_TECComponents)
            {
                if ((g.m_id > 100) && (g.m_id < 500))
                {
                    //Добавить наименование ГТП
                    Label lblTECComponent = HLabel.createLabel(g.name_shr, m_arLabelStyles[(int)INDEX_LABEL.NAME_COMPONENT]);
                    this.Controls.Add(lblTECComponent, 0, m_dictLabelVal.Count + COUNT_FIXED_ROWS);

                    foreach (TG tg in g.TG)
                    {
                        //Добавить наименование ТГ
                        this.Controls.Add(HLabel.createLabel(tg.name_shr, m_arLabelStyles[(int)INDEX_LABEL.NAME_TG]), 1, m_dictLabelVal.Count + COUNT_FIXED_ROWS);
                        //Добавить значение ТГ
                        m_dictLabelVal.Add(/*tg.id_tm*/m_dictLabelVal.Count, HLabel.createLabel(0.ToString("F2"), m_arLabelStyles[(int)INDEX_LABEL.VALUE_TG]));
                        this.Controls.Add(m_dictLabelVal[m_dictLabelVal.Count - 1], 2, m_dictLabelVal.Count - 1 + COUNT_FIXED_ROWS);
                    }

                    this.SetRowSpan(lblTECComponent, g.TG.Count);
                }
                else
                    ;
            }

            //Свойства зафиксированных строк
            for (i = 0; i < COUNT_FIXED_ROWS; i++)
                this.RowStyles.Add(new RowStyle(SizeType.Percent, 10));

            this.RowCount = m_dictLabelVal.Count + COUNT_FIXED_ROWS;
            for (i = 0; i < this.RowCount - COUNT_FIXED_ROWS; i++)
            {
                this.RowStyles.Add(new RowStyle(SizeType.Percent, (float)Math.Round((double)(100 - (10 * COUNT_FIXED_ROWS)) / (this.RowCount - COUNT_FIXED_ROWS), 1)));
            }
        }
    }
}
