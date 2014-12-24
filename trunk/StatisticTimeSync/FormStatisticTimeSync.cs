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
        private ConnectionSettings m_connSett;
        private DataTable m_tableSourceData;
        
        public FormStatisticTimeSync()
        {
            InitializeComponent();
        }

        private void FormStatisticTimeSync_Load (object obj, EventArgs ev) {
            m_connSett = new ConnectionSettings () {
                id = -1
                , name = @"DB_CONFIG"
                , server = @"10.100.104.18"
                , port = 1433
                , dbName = @"techsite_cfg-2.X.X"
                , userName = @"client"
                , password = @"client"
                , ignore = false
            };

            int iListenerId = DbSources.Sources().Register(m_connSett, false, m_connSett.name)
                , err = -1;

            DbConnection dbConn = null;
            m_tableSourceData = null;

            dbConn = DbSources.Sources().GetConnection(iListenerId, out err);

            if ((err == 0) && (!(dbConn == null))) {
                m_tableSourceData = DbTSQLInterface.Select(ref dbConn, @"SELECT * FROM source", null, null, out err);

                if (err == 0) {
                    if (m_tableSourceData.Rows.Count > 0)
                    {
                        int i = -1
                            , j = -1;
                        for (i = 0; i < m_arPanels.Length; i++) {
                            m_arPanels[i].EvtAskedData += new DelegateObjectFunc(onEvtQueryAskedData);
                            for (j = 0; j < m_tableSourceData.Rows.Count; j++)
                            {
                                m_arPanels[i].AddSourceData(m_tableSourceData.Rows[j][@"NAME_SHR"].ToString());
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

        private void onEvtQueryAskedData(object ev)
        {
            switch (((EventArgsDataHost)ev).id) {
                case (int)PanelSourceData.ID_ASKED_DATAHOST.CONN_SETT:
                    int iListenerId = DbSources.Sources().Register(m_connSett, false, m_connSett.name)
                        , id = Int32.Parse (m_tableSourceData.Select(@"NAME_SHR = '" + ((PanelSourceData)((EventArgsDataHost)ev).par).GetSelectedSourceData() + @"'")[0][@"ID"].ToString ())
                        , err = -1;
                    DataRow rowConnSett = ConnectionSettingsSource.GetConnectionSettings (TYPE_DATABASE_CFG.CFG_200, iListenerId, id, 501, out err).Rows[0];
                    ConnectionSettings connSett = new ConnectionSettings(rowConnSett, false);
                    ((PanelSourceData)((EventArgsDataHost)ev).par).OnEvtDataRecievedHost(new EventArgsDataHost(((EventArgsDataHost)ev).id, connSett));
                    DbSources.Sources().UnRegister();
                    break;
                default:
                    break;
            }
        }
    }
}
