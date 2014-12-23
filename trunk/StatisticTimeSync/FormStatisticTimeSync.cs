using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Data.Common;

using HClassLibrary;

namespace StatisticTimeSync
{
    public partial class FormStatisticTimeSync : Form
    {
        public FormStatisticTimeSync()
        {
            InitializeComponent();
        }

        private void FormStatisticTimeSync_Load (object obj, EventArgs ev) {
            ConnectionSettings connSett = new ConnectionSettings () {
                id = -1
                , name = @"DB_CONFIG"
                , server = @"10.100.104.18"
                , port = 1433
                , dbName = @"techsite_cfg-2.X.X"
                , userName = @"client"
                , password = @"client"
                , ignore = false
            };

            int iListenerId = DbSources.Sources().Register(connSett, false, connSett.name)
                , err = -1;

            DbConnection dbConn = null;
            DataTable tableSourceData = null;

            dbConn = DbSources.Sources().GetConnection(iListenerId, out err);

            if ((err == 0) && (!(dbConn == null))) {
                tableSourceData = DbTSQLInterface.Select(ref dbConn, @"SELECT * FROM source", null, null, out err);

                if (err == 0) {
                    if (tableSourceData.Rows.Count > 0)
                    {
                        int i = -1
                            , j = -1;
                        for (i = 0; i < m_arPanels.Length; i++) {
                            m_arPanels[i].EvtQueryConnSett += new DelegateFunc(onEvtQueryConnSett);
                            for (j = 0; j < tableSourceData.Rows.Count; j++)
                            {
                                m_arPanels[i].AddSourceData(tableSourceData.Rows[j][@"NAME_SHR"].ToString());
                            }
                        }
                    } else {
                    }
                } else {
                }
            }
            else
                throw new Exception (@"Нет соединения с БД");

            DbSources.Sources().UnRegister ();
        }

        private void onEvtQueryConnSett()
        {
        }
    }
}
