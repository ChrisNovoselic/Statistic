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
        Label lblName;
        
        public PanelTecCurPower(TEC tec)
        {
            int i = -1, j = -1;
            
            InitializeComponent();

            this.Dock = DockStyle.Fill;
            this.RowCount = 6 + 1;
            //Свойства колонок
            this.ColumnCount = 3;
            this.ColumnStyles.Add (new ColumnStyle (SizeType.Percent, 30));
            this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            //Видимая граница для отладки
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            //Наименование ТЭЦ
            lblName = createLabel(tec.name, Color.Blue, Color.LightSteelBlue);

            this.Controls.Add(lblName, 0, 0);
            this.SetColumnSpan (lblName, 3);

            //Дата/время
            this.Controls.Add(lblName, 0, 0);
            this.SetColumnSpan(lblName, 3);

            i = 0; j = 0;
            foreach (TECComponent g in tec.list_TECComponents)
            {
                if ((g.m_id > 100) && (g.m_id < 500)) {
                    //Добавить наименование ГТП
                    Label lblTECComponent = createLabel(g.name_shr, Color.Green, Color.Yellow);
                    this.Controls.Add(lblTECComponent, 0, j + 2);
                    
                    foreach (TG tg in g.TG)
                    {
                        //Добавить наименование ТГ
                        this.Controls.Add(createLabel(tg.name_shr, Color.Red, Color.LightSteelBlue), 1, j + 2);
                        //Добавить наименование ТГ
                        this.Controls.Add(createLabel(tg.name_shr, Color.Black, Color.LightSteelBlue), 2, j + 2);

                        j ++;
                    }

                    this.SetRowSpan(lblTECComponent, g.TG.Count);

                    i ++;
                }
                else
                    ;
            }

            this.RowStyles.Add(new RowStyle(SizeType.Percent, 10));
            this.RowStyles.Add(new RowStyle(SizeType.Percent, 10));
            
            this.RowCount = j + 2;
            for (i = 0; i < this.RowCount - 2; i ++) {
                this.RowStyles.Add(new RowStyle(SizeType.Percent, 80 / (this.RowCount - 2)));
            }
            
        }

        public PanelTecCurPower(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        private Label createLabel (string name, Color cltText, Color clrGrnd) {
            Label lblRes = new Label();
            lblRes.Text = name;
            lblRes.Dock = DockStyle.Fill;
            lblRes.BorderStyle = BorderStyle.Fixed3D;
            lblRes.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            lblRes.ForeColor = cltText;
            lblRes.BackColor = clrGrnd;

            return lblRes;
        }
    }
}
