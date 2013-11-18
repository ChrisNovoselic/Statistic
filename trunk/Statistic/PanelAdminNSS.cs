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

        protected override void getDataGridViewAdmin()
        {
            double value;
            bool valid;

            for (int i = 0; i < 24; i++)
            {
            }
        }

        private void addTextBoxColumn (DateTime date) {
            this.dgwAdminTable.Columns.Add("column" + (this.dgwAdminTable.Columns.Count), m_admin.GetNameTECComponent(((AdminNSS)m_admin).m_list_indxTECComponents[this.dgwAdminTable.Columns.Count - 1]));

            for (int i = 0; i < 24; i++)
            {
                if (this.dgwAdminTable.Columns.Count == 2)
                    this.dgwAdminTable.Rows[i].Cells[0].Value = date.AddHours(i + 1).ToString("yyyy-MM-dd HH");
                else
                    ;

                this.dgwAdminTable.Rows[i].Cells[this.dgwAdminTable.Columns.Count - 1].Value = ((AdminNSS)m_admin).m_listCurRDGValues[this.dgwAdminTable.Columns.Count - 2][i].plan.ToString("F2");
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
            int i = -1;

            while (this.dgwAdminTable.Columns.Count > 1)
            {
                this.dgwAdminTable.Columns.RemoveAt(this.dgwAdminTable.Columns.Count -1);
            }            
            
            for (i = 0; i < 24; i++)
            {
                this.dgwAdminTable.Rows[i].Cells[0].Value = string.Empty;
            }
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
