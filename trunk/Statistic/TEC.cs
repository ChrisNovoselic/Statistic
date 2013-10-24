using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;     
using System.Threading;

namespace Statistic
{
    public class TEC
    {
        public enum INDEX_NAME_FIELD { ADMIN_DATETIME, REC, IS_PER, DIVIAT,
                                        PBR_DATETIME, PBR, PBR_NUMBER, 
                                        COUNT_INDEX_NAME_FIELD};
        public enum TEC_TYPE { COMMON, BIYSK };

        public string name,
                    prefix_admin, prefix_pbr,
                    m_strUsedAdminValues, m_strUsedPPBRvsPBR;
        public List <string> m_strNamesField;

        public int m_timezone_offset_msc { get; set; }
        public string m_path_rdg_excel { get; set;}

        public List<TECComponent> list_TECComponents;

        public TEC_TYPE type() { if (name.IndexOf("Бийск") > -1) return TEC_TYPE.BIYSK; else return TEC_TYPE.COMMON; }

        public ConnectionSettings [] connSetts;
        public int[] m_arListenerIds; //Идентификаторы номеров клиентов подключенных к 'DbInterface' в 'tec.cs' для 'Data' и 'Admin.cs' для 'AdminValues', 'PBR'
        public int[] m_arIndxDbInterfaces; //Индексы 'DbInterface' в 'Admin.cs' для 'AdminValues', 'PBR' (1-ый элемент ВСЕГДА = -1, т.е. не используется, т.к. для 'Data' есть 'm_dbInterface')

        private bool is_connection_error;
        private bool is_data_error;
        
        private int used;

        private DbInterface m_dbInterface; //Для данных (SQL сервер)

        public ParametersTG parametersTGForm;

        public TEC (string name, string table_name_admin, string table_name_pbr, string prefix_admin, string prefix_pbr) {
            list_TECComponents = new List<TECComponent>();

            this.name = name;
            this.m_strUsedAdminValues = table_name_admin; this.m_strUsedPPBRvsPBR = table_name_pbr;
            this.prefix_admin = prefix_admin; this.prefix_pbr = prefix_pbr;

            connSetts = new ConnectionSettings[(int) CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];

            m_arListenerIds = new int[(int)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];
            m_arIndxDbInterfaces = new int[(int)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];

            for (int i = (int)CONN_SETT_TYPE.DATA; i < (int)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i ++) {
                m_arListenerIds [i] =
                m_arIndxDbInterfaces [i] =
                -1;
            }

            m_strNamesField = new List<string> ();

            if (type () == TEC_TYPE.BIYSK)
                parametersTGForm = new ParametersTG ();
            else
                ;

            is_data_error = is_connection_error = false;

            used = 0;
        }

        public void SetNamesField (string admin_datetime, string admin_rec, string admin_is_per, string admin_diviat,
                                    string pbr_datetime, string ppbr_vs_pbr, string pbr_number) {
            m_strNamesField.Add(admin_datetime); //INDEX_NAME_FIELD.ADMIN_DATETIME
            m_strNamesField.Add(admin_rec); //INDEX_NAME_FIELD.REC
            m_strNamesField.Add(admin_is_per); //INDEX_NAME_FIELD.IS_PER
            m_strNamesField.Add(admin_diviat); //INDEX_NAME_FIELD.DIVIAT

            m_strNamesField.Add(pbr_datetime); //INDEX_NAME_FIELD.PBR_DATETIME
            m_strNamesField.Add(ppbr_vs_pbr); //INDEX_NAME_FIELD.PBR

            m_strNamesField.Add(pbr_number); //INDEX_NAME_FIELD.PBR_NUMBER
        }

        public void Request(string request)
        {
            m_dbInterface.Request(m_arListenerIds[(int)CONN_SETT_TYPE.DATA], request);
        }

        public bool GetResponse(out bool error, out DataTable table)
        {
            return m_dbInterface.GetResponse(m_arListenerIds[(int)CONN_SETT_TYPE.DATA], out error, out table);
        }

        public void StartDbInterface()
        {
            if (used == 0)
            {
                m_dbInterface = new DbInterface(DbInterface.DbInterfaceType.MSSQL, "Интерфейс MSSQL-БД: " + name);
                m_arListenerIds[(int)CONN_SETT_TYPE.DATA] = m_dbInterface.ListenerRegister();
                m_dbInterface.Start();
                m_dbInterface.SetConnectionSettings(connSetts [(int) CONN_SETT_TYPE.DATA]);
            }
            used++;
            if (used > list_TECComponents.Count)
                used = list_TECComponents.Count;
            else
                ;
        }

        public void StopDbInterface()
        {
            used--;
            if (used == 0)
            {
                m_dbInterface.Stop();
                m_dbInterface.ListenerUnregister(m_arListenerIds[(int)CONN_SETT_TYPE.DATA]);
            }
            else
                ;

            if (used < 0)
                used = 0;
            else
                ;
        }

        public void StopDbInterfaceForce()
        {
            if (used > 0)
            {
                m_dbInterface.Stop();
                m_dbInterface.ListenerUnregister(m_arListenerIds[(int)CONN_SETT_TYPE.DATA]);
            }
            else
                ;
        }

        public void RefreshConnectionSettings()
        {
            if (used > 0)
            {
                m_dbInterface.SetConnectionSettings(connSetts [(int) CONN_SETT_TYPE.DATA]);
            }
            else
                ;
        }

        public int connSettings (DataTable source, int type) {
            int iRes = 0;

            connSetts[type] = new ConnectionSettings();
            connSetts[type].server = source.Rows[0]["IP"].ToString();
            connSetts[type].port = Int32.Parse(source.Rows[0]["PORT"].ToString());
            connSetts[type].dbName = source.Rows[0]["DB_NAME"].ToString();
            connSetts[type].userName = source.Rows[0]["UID"].ToString();
            connSetts[type].password = source.Rows[0]["PASSWORD"].ToString();
            connSetts[type].ignore = Boolean.Parse(source.Rows[0]["IGNORE"].ToString()); //== "1";

            return iRes;
        }

        private string selectPBRValueQuery(TECComponent g)
        {
            string strRes = string.Empty;

            if (g.prefix_admin.Length > 0)
            {
                //selectAdmin += strUsedAdminValues + @"." + nameAdmin + @"_" + g.prefix_admin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.REC] + ", " +
                //            strUsedAdminValues + @"." + nameAdmin + @"_" + g.prefix_admin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.IS_PER] + ", " +
                //            strUsedAdminValues + @"." + nameAdmin + @"_" + g.prefix_admin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.DIVIAT];
                strRes += m_strUsedPPBRvsPBR + @"." + prefix_pbr + @"_" + g.prefix_admin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.PBR];
            }
            else
            {
                //selectAdmin += strUsedAdminValues + "." + nameAdmin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.REC] + ", " +
                //            strUsedAdminValues + "." + nameAdmin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.IS_PER] + ", " +
                //            strUsedAdminValues + "." + nameAdmin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.DIVIAT];
                strRes += m_strUsedPPBRvsPBR + "." + prefix_pbr + "_" + m_strNamesField[(int)INDEX_NAME_FIELD.PBR];
            }

            return strRes;
        }

        private string pbrValueQuery(string selectPBR, DateTime dt, ChangeMode.MODE_TECCOMPONENT mode)
        {
            string strRes = string.Empty;

            switch (mode)
            {
                case ChangeMode.MODE_TECCOMPONENT.GTP:
                    strRes = @"SELECT " +
                        //strUsedAdminValues + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " AS DATE_ADMIN, " +
                        m_strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " AS DATE_PBR" +
                        //        selectAdmin +
                        @", " + selectPBR + " AS PBR";
                    if (m_strNamesField[(int)INDEX_NAME_FIELD.PBR_NUMBER].Length > 0)
                        strRes += @", " + m_strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_NUMBER];
                    else
                        ;
                    strRes += @" " + @"FROM " +
                        /*strUsedAdminValues*/ m_strUsedPPBRvsPBR +
                        @" WHERE " + m_strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " >= '" + dt.ToString("yyyy-MM-dd HH:mm:ss") + @"'" +
                        @" AND " + m_strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " <= '" + dt.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") + @"'" +
                        @" AND MINUTE(" + m_strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + ") = 0" +
                        //@" AND " + strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " IS NULL" +
                        @" ORDER BY DATE_PBR" +
                        @" ASC";
                    break;
                case ChangeMode.MODE_TECCOMPONENT.PC:
                    string strUsedPPBRvsPBR = "PPBRvsPBROfID";

                    strRes = @"SELECT " +
                        strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " AS DATE_PBR" +
                        @", " + selectPBR.Split (';')[0] + " AS PBR";

                    strRes += @", " + strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_NUMBER];

                    //Такого столбца для ГТП нет
                    strRes += @", " + "ID_COMPONENT";

                    strRes += @" " + @"FROM " +
                        strUsedPPBRvsPBR +
                        @" WHERE " + strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " >= '" + dt.ToString("yyyy-MM-dd HH:mm:ss") + @"'" +
                        @" AND " + strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " <= '" + dt.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") + @"'" +

                        @" AND ID_COMPONENT IN (" + selectPBR.Split (';')[1] + ")" +

                        @" AND MINUTE(" + strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + ") = 0";
                    /*
                    if (selectPBR.Split(';')[1].Split (',').Length > 1)
                        strRes += @" GROUP BY DATE_PBR";
                    else
                        ;
                    */
                    strRes += @" ORDER BY DATE_PBR" +
                        @" ASC";
                    break;
                default:
                    break;
            }

            return strRes;
        }

        public string GetPBRValueQuery (TECComponent comp, DateTime dt, ChangeMode.MODE_TECCOMPONENT mode) {
            string strRes = string.Empty,
                    selectPBR = string.Empty;

            switch (mode)
            {
                case ChangeMode.MODE_TECCOMPONENT.GTP:
                    selectPBR = selectPBRValueQuery(comp);
                    break;
                case ChangeMode.MODE_TECCOMPONENT.PC:
                    selectPBR = m_strNamesField[(int)INDEX_NAME_FIELD.PBR];

                    selectPBR += ";";

                    selectPBR += comp.m_id.ToString ();
                    break;
                default:
                    break;
            }

            strRes = pbrValueQuery(selectPBR, dt, mode);

            return strRes;
        }

        private string selectAdminValueQuery(TECComponent g)
        {
            string strRes = String.Empty;

            if (g.prefix_admin.Length > 0)
            {
                strRes += m_strUsedAdminValues + @"." + g.tec.prefix_admin + @"_" + g.prefix_admin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.REC] + ", " +
                            m_strUsedAdminValues + @"." + prefix_admin + @"_" + g.prefix_admin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.IS_PER] + ", " +
                            m_strUsedAdminValues + @"." + prefix_admin + @"_" + g.prefix_admin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.DIVIAT];
                //selectPBR += strUsedPPBRvsPBR + @"." + namePBR + @"_" + g.prefix_admin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.PBR];
            }
            else {
                strRes += m_strUsedAdminValues + @"." + prefix_admin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.REC] + ", " +
                            m_strUsedAdminValues + @"." + prefix_admin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.IS_PER] + ", " +
                            m_strUsedAdminValues + @"." + prefix_admin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.DIVIAT];
                //selectPBR += strUsedPPBRvsPBR + "." + namePBR + "_" + m_strNamesField[(int)INDEX_NAME_FIELD.PBR];
            }

            return strRes;
        }

        private string adminValueQuery(string selectAdmin, DateTime dt, ChangeMode.MODE_TECCOMPONENT mode)
        {
            string strRes = string.Empty;
            
            switch (mode) {
                case ChangeMode.MODE_TECCOMPONENT.GTP:
                    strRes = @"SELECT " + m_strUsedAdminValues + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " AS DATE_ADMIN, " +
                        //strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " AS DATE_PBR, " +
                                selectAdmin +
                        //@", " + selectPBR +
                        //@", " + strUsedPPBRvsPBR + ".PBR_NUMBER " +
                                @" " + @"FROM " + m_strUsedAdminValues +

                                @" " + @"WHERE " + m_strUsedAdminValues + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " >= '" + dt.ToString("yyyy-MM-dd HH:mm:ss") + @"'" +
                                @" " + @"AND " + m_strUsedAdminValues + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " <= '" + dt.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") + @"'" +

                                @" " + @"UNION " +
                                @"SELECT " + m_strUsedAdminValues + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " AS DATE_ADMIN, " +

                                //strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " AS DATE_PBR, " +
                                selectAdmin +
                        //@", " + selectPBR +
                        //@", " + strUsedPPBRvsPBR + ".PBR_NUMBER " +

                                @" " + @"FROM " + m_strUsedAdminValues +

                                //" RIGHT JOIN " + strUsedPPBRvsPBR +
                        //" ON " + m_strUsedAdminValues + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " = " + strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " " +

                                @" " + @"WHERE " +

                                //strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " >= '" + dt.ToString("yyyy-MM-dd HH:mm:ss") +
                        //@"' AND " + strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + " <= '" + dt.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") +
                        //@"' AND MINUTE(" + strUsedPPBRvsPBR + "." + m_strNamesField[(int)INDEX_NAME_FIELD.PBR_DATETIME] + ") = 0" +

                                //@" AND " +
                                m_strUsedAdminValues + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " IS NULL" +

                                @" " + @"ORDER BY DATE_ADMIN" +
                        //@", DATE_PBR" +
                                @" " + @"ASC";
                    break;
                case ChangeMode.MODE_TECCOMPONENT.PC:
                    string strUsedAdminValues = @"AdminValuesOfID";

                    strRes = @"SELECT " + strUsedAdminValues + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " AS DATE_ADMIN, " +
                        selectAdmin.Split (';') [0] +

                        //Такого столбца для ГТП нет
                        @", " + "ID_COMPONENT" +

                        @" " + @"FROM " + strUsedAdminValues +

                        @" " + @"WHERE" +
                        @" " + @"ID_COMPONENT IN (" + selectAdmin.Split(';')[1] + ")" +

                        @" " + @"AND " +
                        strUsedAdminValues + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " >= '" + dt.ToString("yyyy-MM-dd HH:mm:ss") + @"'" +
                        @" " + @"AND " +
                        strUsedAdminValues + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " <= '" + dt.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") + @"'";
                    /*
                    strRes += @" " + @"UNION " +
                        @"SELECT " + strUsedAdminValues + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " AS DATE_ADMIN, " +

                        selectAdmin.Split (';') [0] +

                        @" " + @"FROM " + strUsedAdminValues +

                        @" " + @"WHERE" +
                        @" " + @"ID_COMPONENT IN (" + selectAdmin.Split(';')[1] + ")" +

                        @" " + @"AND " +
                        strUsedAdminValues + "." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " IS NULL";
                    */
                    /*
                    if (selectAdmin.Split(';')[1].Split(',').Length > 1)
                        strRes += @" GROUP BY DATE_ADMIN";
                    else
                        ;
                    */
                    strRes += @" " + @"ORDER BY DATE_ADMIN" +
                        @" " + @"ASC";
                    break;
                default:
                    break;
            }

            return strRes;
        }

        public string GetAdminValueQuery(TECComponent comp, DateTime dt, ChangeMode.MODE_TECCOMPONENT mode)
        {
            string strRes = string.Empty,
                    selectAdmin = string.Empty;

            switch (mode)
            {
                case ChangeMode.MODE_TECCOMPONENT.GTP:
                    selectAdmin = selectAdminValueQuery(comp);
                    break;
                case ChangeMode.MODE_TECCOMPONENT.PC:
                    selectAdmin = m_strNamesField[(int)INDEX_NAME_FIELD.REC] + ", " + m_strNamesField[(int)INDEX_NAME_FIELD.IS_PER] + ", " + m_strNamesField[(int)INDEX_NAME_FIELD.DIVIAT];

                    selectAdmin += ";";

                    selectAdmin += comp.m_id.ToString();
                    break;
                default:
                    break;
            }

            strRes = adminValueQuery(selectAdmin, dt, mode);

            return strRes;
        }

        public string GetPBRValueQuery(int num_comp, DateTime dt, ChangeMode.MODE_TECCOMPONENT mode)
        {
            string strRes = string.Empty,
                    selectPBR = string.Empty;

            switch (mode)
            {
                case ChangeMode.MODE_TECCOMPONENT.GTP:
                    if (num_comp < 0)
                    {
                        foreach (TECComponent g in list_TECComponents)
                        {
                            selectPBR += ", ";

                            selectPBR += selectPBRValueQuery(g);
                        }
                        selectPBR = selectPBR.Substring(2);
                    }
                    else
                    {
                        selectPBR = selectPBRValueQuery(list_TECComponents[num_comp]);
                    }
                    break;
                case ChangeMode.MODE_TECCOMPONENT.PC:
                    if (num_comp < 0)
                    {
                        foreach (TECComponent g in list_TECComponents)
                        {
                            selectPBR += ", ";

                            selectPBR += (g.m_id).ToString ();
                        }
                        selectPBR = selectPBR.Substring(2);
                    }
                    else
                    {
                        selectPBR = (list_TECComponents[num_comp].m_id).ToString ();
                    }

                    selectPBR = m_strNamesField[(int)INDEX_NAME_FIELD.PBR] + ";" + selectPBR;
                    break;
                default:
                    break;
            }

            strRes = pbrValueQuery(selectPBR, dt, mode);

            return strRes;
        }

        public string GetAdminValueQuery(int num_comp, DateTime dt, ChangeMode.MODE_TECCOMPONENT mode)
        {
            string strRes = string.Empty,
                selectAdmin = string.Empty;

            switch (mode)
            {
                case ChangeMode.MODE_TECCOMPONENT.GTP:
                    if (num_comp < 0)
                    {
                        foreach (TECComponent g in list_TECComponents)
                        {
                            selectAdmin += ", ";
                            //selectPBR += ", ";

                            selectAdmin += selectAdminValueQuery(g);
                        }
                        selectAdmin = selectAdmin.Substring(2);
                        //selectPBR = selectPBR.Substring(2);
                    }
                    else
                    {
                        selectAdmin = selectAdminValueQuery(list_TECComponents[num_comp]);
                    }
                    break;
                case ChangeMode.MODE_TECCOMPONENT.PC:
                    if (num_comp < 0)
                    {
                        foreach (TECComponent g in list_TECComponents)
                        {
                            selectAdmin += ", ";

                            selectAdmin += (g.m_id).ToString();
                        }
                        selectAdmin = selectAdmin.Substring(2);
                    }
                    else
                    {
                        selectAdmin += (list_TECComponents[num_comp].m_id).ToString();
                    }

                    selectAdmin = m_strNamesField[(int)INDEX_NAME_FIELD.REC] + ", " + m_strNamesField[(int)INDEX_NAME_FIELD.IS_PER] + ", " + m_strNamesField[(int)INDEX_NAME_FIELD.DIVIAT] + ";" + selectAdmin;
                    break;
                default:
                    break;
            }

            strRes = adminValueQuery(selectAdmin, dt, mode);

            //switch (type())
            //{
            //    case TEC.TEC_TYPE.COMMON:

            //        break;
            //    case TEC.TEC_TYPE.BIYSK:
            //        if (num_comp < 0)
            //        {
            //            foreach (TECComponent g in list_TECComponents)
            //            {
            //                selectAdmin += ", ";
            //                //selectPBR += ", ";

            //                if (g.prefix_admin.Length > 0)
            //                {
            //                    selectAdmin += strUsedAdminValues + @"." + nameAdmin + @"_" + g.prefix_admin + "_" + m_strNamesField [(int)INDEX_NAME_FIELD.REC] + ", " +
            //                                strUsedAdminValues + "." + nameAdmin + @"_" + g.prefix_admin + "_" + m_strNamesField [(int)INDEX_NAME_FIELD.IS_PER] + ", " +
            //                                strUsedAdminValues + "." + nameAdmin + @"_" + g.prefix_admin + "_" + m_strNamesField[(int)INDEX_NAME_FIELD.DIVIAT];
            //                    //selectPBR += strUsedPPBRvsPBR + @"." + namePBR + g.prefix_pbr;
            //                }
            //                else {
            //                    selectAdmin += strUsedAdminValues + @"." + nameAdmin + @"_" + m_strNamesField [(int)INDEX_NAME_FIELD.REC] + ", " +
            //                                    strUsedAdminValues + "." + nameAdmin + @"_" + m_strNamesField [(int)INDEX_NAME_FIELD.IS_PER] + ", " +
            //                                    strUsedAdminValues + "." + nameAdmin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.DIVIAT];
            //                    //selectPBR += strUsedPPBRvsPBR + @"." + namePBR;
            //                }
            //            }
            //            selectAdmin = selectAdmin.Substring(2);
            //            //selectPBR = selectPBR.Substring(2);
            //        }
            //        else
            //        {
            //            TECComponent g = list_TECComponents[num_comp];
            //            if (g.prefix_admin.Length > 0)
            //            {
            //                selectAdmin += strUsedAdminValues + @"." + nameAdmin + @"_" + list_TECComponents[num_comp].prefix_admin + @"_" + m_strNamesField [(int)INDEX_NAME_FIELD.REC] + ", " +
            //                            strUsedAdminValues + "." + nameAdmin + @"_" + list_TECComponents[num_comp].prefix_admin + @"_" + m_strNamesField [(int)INDEX_NAME_FIELD.IS_PER] + ", " +
            //                            strUsedAdminValues + @"." + nameAdmin + @"_" + list_TECComponents[num_comp].prefix_admin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.DIVIAT];
            //                //selectPBR += strUsedPPBRvsPBR + @"." + namePBR + list_TECComponents[num_comp].prefix_pbr;
            //            }
            //            else {
            //                selectAdmin += strUsedAdminValues + @"." + nameAdmin + @"_" + m_strNamesField [(int)INDEX_NAME_FIELD.REC] + ", " +
            //                                strUsedAdminValues + "." + nameAdmin + @"_" + m_strNamesField [(int)INDEX_NAME_FIELD.IS_PER] + ", " +
            //                                strUsedAdminValues + "." + nameAdmin + @"_" + m_strNamesField[(int)INDEX_NAME_FIELD.DIVIAT];
            //                //selectPBR += strUsedPPBRvsPBR + @"." + namePBR;
            //            }
            //        }

            //        strRes = @"SELECT " + strUsedAdminValues + @"." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + ", " + selectAdmin +
            //                 @" FROM " + strUsedAdminValues + " " +
            //                 @"WHERE " + strUsedAdminValues + @"." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " >= '" + dt.ToString("yyyy-MM-dd HH:mm:ss") +
            //                 @"' AND " + strUsedAdminValues + @"." + m_strNamesField[(int)INDEX_NAME_FIELD.ADMIN_DATETIME] + " <= '" + dt.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") +
            //                 @"' ORDER BY DATE";
            //        break;
            //    default:
            //        break;
            //}

            return strRes;
        }

        public string GetAdminDatesQuery(DateTime dt, ChangeMode.MODE_TECCOMPONENT mode, TECComponent comp)
        {
            string strUsedAdminValues = "AdminValuesOfID",
                    strRes = string.Empty;

            switch (mode)
            {
                case ChangeMode.MODE_TECCOMPONENT.GTP:
                    strRes = @"SELECT DATE FROM " + m_strUsedAdminValues + " WHERE " +
                          @"DATE > '" + dt.ToString("yyyy-MM-dd HH:mm:ss") +
                          @"' AND DATE <= '" + dt.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") +
                          @"' ORDER BY DATE ASC";
                    break;
                case ChangeMode.MODE_TECCOMPONENT.PC:
                    strRes = @"SELECT DATE FROM " + strUsedAdminValues + " WHERE" +
                            @" ID_COMPONENT = " + comp.m_id +
                          @" AND DATE > '" + dt.ToString("yyyy-MM-dd HH:mm:ss") +
                          @"' AND DATE <= '" + dt.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") +
                          @"' ORDER BY DATE ASC";
                    break;
                default:
                    break;
            }

            return strRes;
        }

        public string GetPBRDatesQuery(DateTime dt, ChangeMode.MODE_TECCOMPONENT mode, TECComponent comp)
        {
            string strUsedPPBRvsPBR = "PPBRvsPBROfID",
                    strRes = string.Empty;

            switch (mode)
            {
                case ChangeMode.MODE_TECCOMPONENT.GTP:
                    strRes = @"SELECT DATE_TIME FROM " + m_strUsedPPBRvsPBR +
                            @" WHERE " +
                            @"DATE_TIME > '" + dt.ToString("yyyy-MM-dd HH:mm:ss") +
                            @"' AND DATE_TIME <= '" + dt.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") +
                            @"' ORDER BY DATE_TIME ASC";
                    break;
                case ChangeMode.MODE_TECCOMPONENT.PC:
                    strRes = @"SELECT DATE_TIME FROM " + strUsedPPBRvsPBR +
                            @" WHERE" +
                            @" ID_COMPONENT = " + comp.m_id + "" + 
                            @" AND DATE_TIME > '" + dt.ToString("yyyy-MM-dd HH:mm:ss") +
                            @"' AND DATE_TIME <= '" + dt.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") +
                            @"' ORDER BY DATE_TIME ASC";
                    break;
                default:
                    break;
            }

            return strRes;
        }

        public string NameFieldOfAdminRequest(TECComponent comp)
        {
            string strRes = @"" + this.prefix_admin;

            if (comp.prefix_admin.Length > 0)
            {
                strRes += "_" + comp.prefix_admin;
            }
            else
                ;

            return strRes;
        }

        public string NameFieldOfPBRRequest(TECComponent comp)
        {
            string strRes = @"" + this.prefix_pbr;

            if (comp.prefix_pbr.Length > 0)
            {
                strRes += "_" + comp.prefix_pbr;
            }
            else
                ;

            return strRes;
        }
    }
}
