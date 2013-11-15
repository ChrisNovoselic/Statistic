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

        public override void setDataGridViewAdmin(DateTime date)
        {
            for (int i = 0; i < 24; i++)
            {
                this.dgwAdminTable.Rows[i].Cells[0].Value = date.AddHours(i + 1).ToString("yyyy-MM-dd HH");
            }

            m_admin.CopyCurToPrevRDGValues();
        }

        public override void ClearTables()
        {
            for (int i = 0; i < 24; i++)
            {
                this.dgwAdminTable.Rows[i].Cells[0].Value = string.Empty;
            }
        }
    }
}
