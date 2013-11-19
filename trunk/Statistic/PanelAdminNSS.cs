using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using System.Security.Cryptography;
using System.IO;
using System.Threading;
using System.Globalization;

using StatisticCommon;

namespace Statistic
{
    public class PanelAdminNSS : PanelAdmin
    {
        protected override void InitializeComponents()
        {
            base.InitializeComponents();

            this.dgwAdminTable = new DataGridViewAdminNSS();
            this.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgwAdminTable)).BeginInit();
            this.Controls.Add(this.dgwAdminTable);
            // 
            // dgwAdminTable
            //
            this.dgwAdminTable.Location = new System.Drawing.Point(176, 9);
            this.dgwAdminTable.Size = new System.Drawing.Size(574, 591);
            this.dgwAdminTable.TabIndex = 1;
            //this.dgwAdminTable.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgwAdminTable_CellClick);
            //this.dgwAdminTable.CellValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgwAdminTable_CellValidated);
            ((System.ComponentModel.ISupportInitialize)(this.dgwAdminTable)).EndInit();
            this.ResumeLayout();
        }

        public PanelAdminNSS (Admin a) : base (a) {
        }

        private int GetIndexGTPOwner(int indx_tg)
        {
            int id_gtp_owner = ((DataGridViewAdminNSS)dgwAdminTable).GetIdGTPOwner(indx_tg);
            
            foreach (int indx in ((AdminNSS)m_admin).m_list_indxTECComponents)
            {
                if (m_admin.allTECComponents[indx].m_id == id_gtp_owner) {
                    return ((AdminNSS)m_admin).m_list_indxTECComponents.IndexOf (indx);
                }
                else
                    ;
            }

            return -1;
        }

        protected override void getDataGridViewAdmin()
        {
            double value;
            bool valid;

            foreach (int indx in ((AdminNSS)m_admin).m_list_indxTECComponents) 
            {
                if (m_admin.modeTECComponent(indx) == FormChangeMode.MODE_TECCOMPONENT.TG)
                {
                    int indx_tg = ((AdminNSS)m_admin).m_list_indxTECComponents.IndexOf(indx),
                        indx_gtp = GetIndexGTPOwner(indx_tg);

                    if ((!(indx_tg < 0)) && (!(indx_gtp < 0)))
                        for (int i = 0; i < 24; i++)
                        {
                            ((AdminNSS)m_admin).m_listCurRDGValues[indx_tg][i].plan = Convert.ToDouble (dgwAdminTable.Rows[i].Cells [indx_tg + 1].Value); // '+ 1' за счет DateTime

                            ((AdminNSS)m_admin).m_listCurRDGValues[indx_tg][i].recomendation = 0.0;
                            ((AdminNSS)m_admin).m_listCurRDGValues[indx_tg][i].deviationPercent = ((AdminNSS)m_admin).m_listCurRDGValues[indx_gtp][i].deviationPercent;
                            ((AdminNSS)m_admin).m_listCurRDGValues[indx_tg][i].deviation = ((AdminNSS)m_admin).m_listCurRDGValues[indx_gtp][i].deviation;
                        }
                    else
                        ;
                }
                else
                    ;
            }
        }

        private void addTextBoxColumn (DateTime date) {
            int indx = ((AdminNSS)m_admin).m_list_indxTECComponents[this.dgwAdminTable.Columns.Count - 2];
            ((DataGridViewAdminNSS)this.dgwAdminTable).addTextBoxColumn(m_admin.GetNameTECComponent(indx),
                                                                        m_admin.GetIdTECComponent (indx),
                                                                        m_admin.GetIdGTPOwnerTECComponent(indx),
                                                                        date);

            DataGridViewCellEventArgs ev;

            for (int i = 0; i < 24; i++)
            {
                if (this.dgwAdminTable.Columns.Count == 3) //Только при добавлении 1-го столбца
                    this.dgwAdminTable.Rows[i].Cells[0].Value = date.AddHours(i + 1).ToString("yyyy-MM-dd HH");
                else
                    ;

                this.dgwAdminTable.Rows[i].Cells[this.dgwAdminTable.Columns.Count - 2].Value = ((AdminNSS)m_admin).m_listCurRDGValues[this.dgwAdminTable.Columns.Count - 3][i].plan.ToString("F2");
                ev = new DataGridViewCellEventArgs(this.dgwAdminTable.Columns.Count - 2, i);
                ((DataGridViewAdminNSS)this.dgwAdminTable).DataGridViewAdminNSS_CellValueChanged(null, ev);
            }

            m_admin.CopyCurToPrevRDGValues();
        }

        private void updateTextBoxColumn()
        {
            for (int i = 0; i < 24; i++)
            {
            }

            //m_admin.CopyCurToPrevRDGValues();
        }

        public override void setDataGridViewAdmin(DateTime date)
        {
            //if (this.dgwAdminTable.Columns.Count < ((AdminNSS)m_admin).m_list_indxTECComponents.Count)
                this.BeginInvoke(new DelegateDateFunction(addTextBoxColumn), date);
            //else
            //    this.BeginInvoke(new DelegateFunc(updateTextBoxColumn));
        }

        public override void ClearTables()
        {
            ((DataGridViewAdminNSS)this.dgwAdminTable).ClearTables();
        }

        public override void InitializeComboBoxTecComponent(FormChangeMode.MODE_TECCOMPONENT mode)
        {
            base.InitializeComboBoxTecComponent (mode);

            if (m_listTECComponentIndex.Count > 0) {
                comboBoxTecComponent.Items.AddRange (((AdminNSS)m_admin).GetListNameTEC ());
            
                if (comboBoxTecComponent.Items.Count > 0)
                {
                    m_admin.indxTECComponents = m_listTECComponentIndex[0];
                    comboBoxTecComponent.SelectedIndex = 0;
                }
                else
                    ;
            }
            else
                ;
        }

        public override void Activate(bool active)
        {
            base.Activate (active);

            ClearTables ();
        }
    }
}
