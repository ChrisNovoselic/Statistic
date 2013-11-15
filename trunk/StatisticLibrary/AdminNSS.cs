using System;
using System.Data;

namespace StatisticCommon
{
    public class AdminNSS : Admin
    {
        public AdminNSS () {
        }

        protected override bool GetAdminValuesResponse(DataTable tableAdminValuesResponse, DateTime date)
        {
            DataTable table = null;
            DataTable[] arTable = { m_tablePPBRValuesResponse, tableAdminValuesResponse };
            int [] arIndexTables = {0, 1};

            int i = -1, j = -1, k = -1,
                hour = -1;

            int offsetPBR_NUMBER = m_tablePPBRValuesResponse.Columns.IndexOf ("PBR_NUMBER");
            if (offsetPBR_NUMBER > 0) offsetPBR_NUMBER = 0; else ;

            int offsetPBR = m_tablePPBRValuesResponse.Columns.IndexOf("PBR");
            if (offsetPBR > 0) offsetPBR = 0; else ;

            //׃האכוםטו סעמכבצמג 'ID_COMPONENT'
            for (i = 0; i < arTable.Length; i++) {
                /*
                for (j = 0; j < arTable[i].Columns.Count; j++)
                {
                    if (arTable[i].Columns [j].ColumnName == "ID_COMPONENT") {
                        arTable[i].Columns.RemoveAt (j);
                        break;
                    }
                    else
                        ;
                }
                */
                if (!(arTable[i].Columns.IndexOf("ID_COMPONENT") < 0))
                    try { arTable[i].Columns.Remove("ID_COMPONENT"); }
                    catch (Exception e) {
                        Logging.Logg().LogExceptionToFile(e, "AdminNSS - GetAdminValuesResponse () - ...Columns.Remove(\"ID_COMPONENT\")"); 
                    }
                else
                    ;
            }

            if (arTable[0].Rows.Count < arTable[1].Rows.Count) {
                arIndexTables[0] = 1;
                arIndexTables[1] = 0;
            }
            else {
            }

            table = arTable[arIndexTables [0]].Copy();
            table.Merge(arTable[arIndexTables[1]].Clone (), false);

            for (i = 0; i < arTable[arIndexTables[0]].Rows.Count; i++)
            {
                for (j = 0; j < arTable[arIndexTables[1]].Rows.Count; j++)
                {
                    if (arTable[arIndexTables[0]].Rows[i][0].Equals (arTable[arIndexTables[1]].Rows[j][0])) {
                        for (k = 0; k < arTable[arIndexTables[1]].Columns.Count; k++)
                        {
                            table.Rows [i] [arTable[arIndexTables[1]].Columns [k].ColumnName] = arTable[arIndexTables[1]].Rows[j][k];
                        }
                    }
                    else
                        ;
                }
            }

            return true;
        }
    }
}
