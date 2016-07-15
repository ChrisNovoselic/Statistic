using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;

using HClassLibrary;

namespace StatisticCommon
{
    /// <summary>
    /// ����� ��� �������� ��������� ���������� ���
    /// </summary>
    public class TECComponentBase
    {
        /// <summary>
        /// ������������ - ���� ����������� � ��������� ��� �������������� � �������-, ��� �������� ����� ������������ ���
        /// </summary>
        public enum TYPE : short { UNKNOWN = -1, TEPLO, ELECTRO, COUNT }
        /// <summary>
        /// ��� ���������� � ��������� ��� �������������� � �������-, ��� �������� ����� ������������ ���
        /// </summary>
        TYPE _type;
        /// <summary>
        /// �������������� ��� ����� ���������� ���
        /// </summary>
        public enum ID : int { LK = 10, GTP = 100, GTP_LK = 200, PC = 500, VYVOD = 900, TG = 1000, PARAM_VYVOD = 2000, MAX = 10000 }
        /// <summary>
        /// ������� ������������ ����������
        /// </summary>
        public string name_shr;
        /// <summary>
        /// ����������� ��� ������������� � ������� (��� ����������)
        /// </summary>
        public string name_future;
        /// <summary>
        /// ������������� ���������� (�� �� ������������)
        /// </summary>
        public int m_id;
        /// <summary>
        /// ������ ������� � ����� Excel �� ���������� ��� ��� ���������� (� ��������� ����� ������ ��� ����-5)
        /// </summary>
        public int m_indx_col_rdg_excel;
        /// <summary>
        /// ����������� ��� ������ ��������� ��������� ������������
        /// </summary>
        public decimal m_dcKoeffAlarmPcur;

        //����� �� 'class PanelStatisticView : PanelStatistic' - �� 'PanelStatisticView' ������ ��������� ��������???
        public volatile string[] m_SensorsStrings_ASKUE = { string.Empty, string.Empty }; //������ ��� ��������� ��� (�����) - 3-�, 30-�� ��� ��������������
        public volatile string m_SensorsString_SOTIASSO = string.Empty;
        public volatile string m_SensorsString_VZLET = string.Empty;
        /// <summary>
        /// ���������� - �������� (��� ����������)
        /// </summary>
        public TECComponentBase(TYPE type = TYPE.ELECTRO)
        {
            _type = type;

            m_dcKoeffAlarmPcur = -1;
        }
        /// <summary>
        /// ������� �������������� ���������� � ������ ���
        /// </summary>
        public bool IsGTP { get { return (m_id > (int)ID.GTP) && (m_id < (int)ID.PC); } }
        /// <summary>
        /// ���������� ��� (�����) ���������� �� ���������� ��������������
        /// </summary>
        /// <param name="id">������������� ����������</param>
        /// <returns>��� (�����) ����������</returns>
        public static FormChangeMode.MODE_TECCOMPONENT Mode(int id)
        {
            return (id < (int)ID.GTP) == true ? FormChangeMode.MODE_TECCOMPONENT.TEC :
                ((id > (int)ID.GTP) && (id < (int)ID.PC)) == true ? FormChangeMode.MODE_TECCOMPONENT.GTP :
                ((id > (int)ID.PC) && (id < (int)ID.TG)) == true ? FormChangeMode.MODE_TECCOMPONENT.PC :
                    ((id > (int)ID.TG) && (id < (int)ID.MAX)) == true ? FormChangeMode.MODE_TECCOMPONENT.TG :
                        FormChangeMode.MODE_TECCOMPONENT.ANY;
        }
        /// <summary>
        /// ������� �������������� ���������� � ������ ���� ����������
        ///  (�������, ���������)
        /// </summary>
        public bool IsPC { get { return (m_id > (int)ID.PC) && (m_id < (int)ID.TG); } }
        /// <summary>
        /// ������� �������������� ���������� � ������ ��
        /// </summary>
        public bool IsTG { get { return (m_id > (int)ID.TG) && (m_id < (int)ID.MAX); } }
    }
    /// <summary>
    /// ����� ��� �������� ���������� ��� - ��
    /// </summary>
    public class TG : TECComponentBase
    {
        /// <summary>
        /// ������������ - ������� ��������� ���������� ��� ����������� �������� ��
        /// </summary>
        public enum INDEX_VALUE : int { FACT //����.
                                        , TM //������������
                                        , LABEL_DESC //�������� (������� ������������) ��
                                        , COUNT_INDEX_VALUE }; //���������� ��������
        /// <summary>
        /// ������������ - ��������� ��������� ��
        /// </summary>
        public enum INDEX_TURNOnOff : int { OFF = -1, UNKNOWN, ON };
        /// <summary>
        /// ������ ��������������� �� � ���� ��� (����������� �� 'ID_TIME')
        ///  ��� ��������� ��� (�����) ����������� 3-� � 30-�� ��� ��������������
        ///  ��� ��������� - ���������
        /// </summary>
        public int[] m_arIds_fact;
        /// <summary>
        /// ��������� ������������� � ��������
        /// </summary>
        public string m_strKKS_NAME_TM;
        /// <summary>
        /// �������������� "����������" ��� �� (���, �(��)��)
        /// </summary>
        public int m_id_owner_gtp,
                    m_id_owner_pc;
        /// <summary>
        /// ������� ��������� ��
        /// </summary>
        public INDEX_TURNOnOff m_TurnOnOff; //��������� -1 - ����., 0 - ����������, 1 - ���. (������ ��� AdminAlarm)
        /// <summary>
        /// ����������� - �������� (��� ����������)
        /// </summary>
        public TG()
        {
            m_arIds_fact = new int[(int)HDateTime.INTERVAL.COUNT_ID_TIME];

            m_id_owner_gtp =
            m_id_owner_pc =
                //����������� ��������
                -1;
            m_TurnOnOff = INDEX_TURNOnOff.UNKNOWN; //����������� ���������
        }
        /// <summary>
        /// ����������� - �������� (��� ����������)
        /// </summary>
        public TG(DataRow row_tg, DataRow row_param_tg) : this ()
        {
            initTG(row_tg, row_param_tg);
        }

        //public void InitTG(DataRow row_tg, DataRow row_param_tg, out int err)
        //{
        //    err = -1;

        //    name_shr = row_tg["NAME_SHR"].ToString();
        //    m_id = Convert.ToInt32(row_tg["ID"]);
        //    m_id_owner_gtp = Convert.ToInt32(row_tg["ID_GTP"]);

        //    //DataRow[] rows_tg = allParamTG.Select(@"ID_TG=" + dest.m_id);
        //    //dest.m_strKKS_NAME_TM = rows_tg[0][@"KKS_NAME"].ToString();
        //    //dest.m_arIds_fact[(int)HDateTime.INTERVAL.MINUTES] = Int32.Parse(rows_tg[0][@"ID_IN_ASKUE_3"].ToString());
        //    //dest.m_arIds_fact[(int)HDateTime.INTERVAL.HOURS] = Int32.Parse(rows_tg[0][@"ID_IN_ASKUE_30"].ToString());
        //}

        private void initTG(DataRow row_tg, DataRow row_param_tg)
        {
            name_shr = row_tg["NAME_SHR"].ToString();
            if (DbTSQLInterface.IsNameField(row_tg, "NAME_FUTURE") == true) name_future = row_tg["NAME_FUTURE"].ToString(); else ;
            m_id = Convert.ToInt32(row_tg["ID"]);
            if (!(row_tg["INDX_COL_RDG_EXCEL"] is System.DBNull))
                m_indx_col_rdg_excel = Convert.ToInt32(row_tg["INDX_COL_RDG_EXCEL"]);
            else
                ;

            m_strKKS_NAME_TM = row_param_tg[@"KKS_NAME"].ToString();
            m_arIds_fact[(int)HDateTime.INTERVAL.MINUTES] = Int32.Parse(row_param_tg[@"ID_IN_ASKUE_3"].ToString());
            m_arIds_fact[(int)HDateTime.INTERVAL.HOURS] = Int32.Parse(row_param_tg[@"ID_IN_ASKUE_30"].ToString());
        }
    }
    /// <summary>
    /// ����� ��� �������� ���������� ��� (���, �(��)��)
    /// </summary>
    public class TECComponent : TECComponentBase
    {
        /// <summary>
        /// ������ ��������������� � �����-�����
        /// </summary>
        public List<int> m_listMCentreId;
        /// <summary>
        /// ������ ��������������� � �����-���������
        /// </summary>
        public List<int> m_listMTermId;
        /// <summary>
        /// ������ ��
        /// </summary>
        public List<TG> m_listTG;
        /// <summary>
        /// ������ ��� - "��������" ����������
        /// </summary>
        public TEC tec;
        /// <summary>
        /// ����������� - ��������������
        /// </summary>
        public TECComponent(TEC tec, DataRow rComp) : this (tec)
        {
            name_shr = rComp["NAME_SHR"].ToString(); //rComp["NAME_GNOVOS"]
            if (DbTSQLInterface.IsNameField(rComp, "NAME_FUTURE") == true) this.name_future = rComp["NAME_FUTURE"].ToString(); else ;
            m_id = Convert.ToInt32(rComp["ID"]);
            m_listMCentreId = getMCentreId(rComp);
            m_listMTermId = getMTermId(rComp);
            if ((DbTSQLInterface.IsNameField (rComp, "INDX_COL_RDG_EXCEL") == true) && (!(rComp["INDX_COL_RDG_EXCEL"] is System.DBNull)))
                m_indx_col_rdg_excel = Convert.ToInt32(rComp["INDX_COL_RDG_EXCEL"]);
            else
                ;
            if ((DbTSQLInterface.IsNameField (rComp, "KoeffAlarmPcur") == true) && (!(rComp["KoeffAlarmPcur"] is System.DBNull)))
                m_dcKoeffAlarmPcur = Convert.ToInt32(rComp["KoeffAlarmPcur"]);
            else
                ;
        }
        /// <summary>
        /// ����������� - ��������������
        /// </summary>
        private TECComponent(TEC tec)
        {
            this.tec = tec;

            m_listTG = new List<TG>();
            m_listMCentreId =
            m_listMTermId = null;
        }

        protected List<int> getModesId(DataRow r, string col)
        {
            int i = -1
                , id = -1;
            List<int> listRes = new List<int>();

            if ((DbTSQLInterface.IsNameField (r, col) == true) && (!(r[col] is DBNull)))
            {
                string[] ids = r[col].ToString().Split(',');
                for (i = 0; i < ids.Length; i++)
                    if (ids[i].Length > 0)
                        if (Int32.TryParse(ids[i], out id) == true)
                            listRes.Add(id);
                        else
                            listRes.Add(-1);
                    else
                        listRes.Add(-1);
            }
            else
                ;

            return listRes;
        }

        protected List<int> getMCentreId(DataRow r)
        {
            return getModesId(r, @"ID_MC");
        }

        protected List<int> getMTermId(DataRow r)
        {
            return getModesId(r, @"ID_MT");
        }
    }

    public class Vyvod : TECComponentBase
    {
        public class ParamVyvod : TECComponentBase
        {
            public string m_Symbol;
            public int m_typeAgregate;

            public ParamVyvod(DataRow r)
            {
                m_id = Convert.ToInt32(r[@"ID"]);

                Initialize(r);
            }

            public void Initialize(DataRow r)
            {
                m_SensorsString_VZLET = ((string)r[@"KKS_NAME"]).Trim();

                name_shr = ((string)r[@"NAME_SHR"]).Trim();
                m_Symbol = ((string)r[@"SYMBOL"]).Trim();
                m_typeAgregate = Convert.ToByte(r[@"TYPE_AGREGATE"]);
            }
        }
        /// <summary>
        /// ������ ��
        /// </summary>
        public List<ParamVyvod> m_listParam;
        /// <summary>
        /// ������ ��� - "��������" ����������
        /// </summary>
        public TEC tec;

        private bool m_bKomUchet;

        public Vyvod(TEC tec, DataRow row_param)
            : this(tec, new DataRow[] { row_param })
        {
        }
        /// <summary>
        /// ����������� - �������� (c �����������)
        /// </summary>
        /// <param name="tec">������-��� - ������������ �� ��������� � ������������ �������</param>
        /// <param name="r">������ �� ���������� ������� ������������ �������-������</param>
        public Vyvod(TEC tec, DataRow []rows_param)
        {
            this.tec = tec;

            m_listParam = new List<ParamVyvod>();

            Initialize(rows_param);
        }
        /// <summary>
        /// ���������������� ����� ���������� ������� ���� ���������� (�� �������� � ��� �������� ��� ��)
        /// </summary>
        /// <param name="r">������ ������� �� ��������� ������� ���� ����������</param>
        public void Initialize (DataRow []rows_param)
        {
            if (rows_param.Length > 0)
            {
                m_id = Convert.ToInt32(rows_param[0][@"ID_VYVOD"]);
                name_shr = ((string)rows_param[0][@"VYVOD_NAME_SHR"]).Trim ();
                m_bKomUchet = Convert.ToByte(rows_param[0][@"KOM_UCHET"]) == 1 ? true : false;

                foreach (DataRow r in rows_param)
                    InitParam(r);
            }
            else
                Logging.Logg().Error(@"Vyvod::Initialize () - ��� �� ����� ������ �� ��������� ������� ���������� ������...", Logging.INDEX_MESSAGE.NOT_SET);
                ; //??? ����������
        }
        /// <summary>
        /// ���������������� �������� �������� ���������
        /// </summary>
        /// <param name="r">������ �������-������� �� ��������� ������� ���������</param>
        public void InitParam(DataRow r)
        {
            ParamVyvod pv = null;
            int iIdParam = -1;

            iIdParam = Convert.ToInt32(r[@"ID"]);
            pv = m_listParam.Find(p => { return p.m_id == iIdParam; });

            if (pv == null)
            {
                pv = new ParamVyvod(r);
                m_listParam.Add(pv);
            }
            else
                // ����� �������� ��� ����������
                // ������������� ������ ��������� ������� (����� ID)
                pv.Initialize(r);
        }
    }
}
