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
        HLabel[] m_arLabelProperties;

        private class HLabel
        {
            public Color m_foreColor,
                        m_backColor;
            public ContentAlignment m_align;
            public Single m_szFont;

            public HLabel(Color foreColor, Color backColor, Single szFont, ContentAlignment align)
            {
                m_foreColor = foreColor;
                m_backColor = backColor;
                m_szFont = szFont;
                m_align = align;
            }
        };
        
        public PanelTecCurPower(TEC tec)
        {
            int i = -1;

            m_arLabelProperties = new HLabel[(int)INDEX_LABEL.COUNT_INDEX_LABEL];
            m_arLabelProperties[(int)INDEX_LABEL.NAME] = new HLabel(Color.White, Color.Blue, 22F, ContentAlignment.MiddleCenter);
            m_arLabelProperties[(int)INDEX_LABEL.DATETIME] = new HLabel(Color.LimeGreen, Color.Gray, 24F, ContentAlignment.MiddleCenter);
            m_arLabelProperties[(int)INDEX_LABEL.VALUE_TOTAL] = new HLabel(Color.LimeGreen, Color.Black, 24F, ContentAlignment.MiddleCenter);
            m_arLabelProperties[(int)INDEX_LABEL.NAME_COMPONENT] = new HLabel(Color.Yellow, Color.Green, 14F, ContentAlignment.TopLeft);
            m_arLabelProperties[(int)INDEX_LABEL.NAME_TG] = new HLabel(Color.LightSteelBlue, Color.Yellow, 14F, ContentAlignment.MiddleLeft);
            m_arLabelProperties[(int)INDEX_LABEL.VALUE_TG] = new HLabel(Color.LimeGreen, Color.Black, 14F, ContentAlignment.MiddleCenter);

            m_dictLabelVal = new Dictionary<int,Label> ();
            m_arLabel = new Label[(int)INDEX_LABEL.VALUE_TOTAL + 1];
            
            InitializeComponent();

            this.Dock = DockStyle.Fill;
            //Свойства колонок
            this.ColumnStyles.Add (new ColumnStyle (SizeType.Percent, 30));
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
                        cntnt = tec.name;
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
                m_arLabel[i] = createLabel(cntnt, m_arLabelProperties [i]);
                this.Controls.Add(m_arLabel [i], 0, i);
                this.SetColumnSpan(m_arLabel [i], 3);
            }

            foreach (TECComponent g in tec.list_TECComponents)
            {
                if ((g.m_id > 100) && (g.m_id < 500)) {
                    //Добавить наименование ГТП
                    Label lblTECComponent = createLabel(g.name_shr, m_arLabelProperties [(int)INDEX_LABEL.NAME_COMPONENT]);
                    this.Controls.Add(lblTECComponent, 0, m_dictLabelVal.Count + COUNT_FIXED_ROWS);
                    
                    foreach (TG tg in g.TG)
                    {
                        //Добавить наименование ТГ
                        this.Controls.Add(createLabel(tg.name_shr, m_arLabelProperties[(int)INDEX_LABEL.NAME_TG]), 1, m_dictLabelVal.Count + COUNT_FIXED_ROWS);
                        //Добавить значение ТГ
                        m_dictLabelVal.Add(/*tg.id_tm*/m_dictLabelVal.Count, createLabel(0.ToString("F2"), m_arLabelProperties[(int)INDEX_LABEL.VALUE_TG]));
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
                this.RowStyles.Add(new RowStyle(SizeType.Percent, ((float)Math.Round(((double)(100 - (10 * COUNT_FIXED_ROWS)) / (this.RowCount - COUNT_FIXED_ROWS)), 1))));
            }
            
        }

        public PanelTecCurPower(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        private Label createLabel (string name, HLabel prop) {
            Label lblRes = new Label();
            lblRes.Text = name;
            lblRes.Dock = DockStyle.Fill;
            lblRes.BorderStyle = BorderStyle.Fixed3D;
            lblRes.Font = new System.Drawing.Font("Microsoft Sans Serif", prop.m_szFont, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            lblRes.TextAlign = prop.m_align;
            lblRes.ForeColor = prop.m_foreColor;
            lblRes.BackColor = prop.m_backColor;

            return lblRes;
        }
    }
}
