using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Data;
using System.Threading;
using System.Data.Common;
using System.Data.OleDb;

using Modes;
using ModesApiExternal;
using ASUTP;
using ASUTP.Core;
using System.Collections.ObjectModel;

namespace trans_mc
{
    public class DbMCInterface : ASUTP.Database.DbInterface
    {
        private IApiExternal m_MCApi;
        private Modes.BusinessLogic.IModesTimeSlice m_MCTimeSlice;
        private IList <PlanFactorItem> m_listPFI;

        public enum ID_EVENT : short { Unknown = -1, GENOBJECT_MODIFIED, NEW_PLAN_VALUES, RELOAD_PLAN_VALUES }

        public enum Operation : short { Unknown = -1, InitIGO, MaketEquipment, PPBR }
        /// <summary>
        /// ������� ��� ������������ ������� �����-�����
        /// </summary>
        private Action<object> delegateMCApiHandler;

        private List <Modes.BusinessLogic.IGenObject> m_listIGO;

        protected override int Timeout { get; set; }

        /// <summary>
        /// ���������������� �����������
        /// </summary>
        /// <param name="name">���</param>
        public DbMCInterface(string name, Action<object>mcApiHandler)
            //����� ������������ �� �������� ������ DbInterface
            : base(name)
        {
            m_listIGO = new List<Modes.BusinessLogic.IGenObject> ();

            delegateMCApiHandler = mcApiHandler;
        }

        public override bool EqualeConnectionSettings(object cs)
        {
            return string.Equals((string)m_connectionSettings, (string)cs);
        }

        public override bool IsEmptyConnectionSettings
        {
            get
            {
                return string.IsNullOrWhiteSpace((string)m_connectionSettings);
            }
        }

        /// <summary>
        /// ���������� ������������ ������ ("������ ��������� �����������") �� �������� ������
        /// </summary>
        /// <param name="mcHostName">����� ����� ��� �����</param>
        /// <param name="bStarted">������</param>
        public override void SetConnectionSettings(object mcHostName, bool bStarted)
        {
            lock (lockConnectionSettings) // ��������� �������� ����������� � ����������� ����� ��� ��������������� - ��������� ��������
            {
                //!!! ������������ �����
                setConnectionSettings(mcHostName);
                //���� "��������� ����������" ��������� ��� �����
                m_connectionSettings = mcHostName;
            }
            //!!! ������������ �����
            setConnectionSettings();
        }

        /// <summary>
        /// ���������� ���������� � �����-������� � ����������� ������ ���������� � ��������
        /// </summary>
        /// <returns>��������� ������������ ���������� � �������������</returns>
        protected override bool Connect()
        {
            string msgLog = string.Empty;

            if (m_connectionSettings == null)
                return false;
            else
                ;

            if (m_connectionSettings.GetType().Equals(typeof(string)) == false)
                return false;
            else
                ;

            if (!(((string)m_connectionSettings).Length > 0))
                return false;
            else
                ;

            bool result = false, bRes = false;

            try {
                if (bRes == true)
                    return bRes;
                else
                    bRes = true;
            } catch (Exception e) {
                Logging.Logg().Exception(e, "DbMCInterface::Connect ()", Logging.INDEX_MESSAGE.NOT_SET);
            }

            lock (lockConnectionSettings)
            {
                if (IsNeedReconnectNew == true) // ���� ����� �������� � ������ ����� �������� ���� �������� ���������, �� ����������� �� ������� ����������� �� ������
                    return false;
                else
                    ;
            }

            msgLog = string.Format("���������� � Modes-Centre ({0})", (string)m_connectionSettings);

            try {
                ModesApiFactory.Initialize((string)m_connectionSettings);

                Logging.Logg().Debug(string.Format(@"{0} - ...", msgLog), Logging.INDEX_MESSAGE.NOT_SET);
            } catch (Exception e) {
                Logging.Logg().Exception(e, string.Format(@"{0} - ...", msgLog), Logging.INDEX_MESSAGE.NOT_SET);
            }

            bRes = 
            result =
                ModesApiFactory.IsInitilized;
            
            if (bRes == true) {
                // �� ������ ������������ ������� �����-�����
                try {
                    m_MCApi = ModesApiFactory.GetModesApi();
                    m_MCTimeSlice = m_MCApi.GetModesTimeSlice(DateTime.Now.Date.LocalHqToSystemEx(), SyncZone.First, TreeContent.PGObjects, true);
                    m_listPFI = m_MCApi.GetPlanFactors();

                    m_MCApi.OnData53500Modified += mcApi_OnEventHandler;
                    m_MCApi.OnPlanDataChanged += mcApi_OnEventHandler;
                    m_MCApi.OnMaket53500Changed += mcApi_OnEventHandler;

                    Logging.Logg().Debug(string.Format(@"{0} - {1}...", msgLog, @"�����"), Logging.INDEX_MESSAGE.NOT_SET);
                } catch (Exception e) {
                    Logging.Logg().Exception(e, string.Format(@"{0} - ...", msgLog), Logging.INDEX_MESSAGE.NOT_SET);

                    result = false;
                }
            } else
                Logging.Logg().Debug(string.Format(@"{0} - {1}...", msgLog, @"������"), Logging.INDEX_MESSAGE.NOT_SET);

            delegateMCApiHandler?.Invoke (bRes);

            return result;
        }

        private void mcApi_OnEventHandler(object obj, EventArgs e)
        {
            object[] sendToTrans;

            if (e.GetType().Equals(typeof(Modes.NetAccess.EventRefreshData53500)) == true) {
                Modes.NetAccess.EventRefreshData53500 ev = e as Modes.NetAccess.EventRefreshData53500;

                sendToTrans = new object[] {
                    ID_EVENT.GENOBJECT_MODIFIED
                    , ev
                };
            } else if (e.GetType().Equals(typeof(Modes.NetAccess.EventRefreshJournalMaket53500)) == true) {
                Modes.NetAccess.EventRefreshJournalMaket53500 ev = e as Modes.NetAccess.EventRefreshJournalMaket53500;

                sendToTrans = new object[] {
                    ID_EVENT.RELOAD_PLAN_VALUES
                    , ev
                };
            } else if (e.GetType().Equals(typeof(Modes.NetAccess.EventPlanDataChanged)) == true) {
                Modes.NetAccess.EventPlanDataChanged ev = e as Modes.NetAccess.EventPlanDataChanged;

                sendToTrans = new object[] {
                    ID_EVENT.NEW_PLAN_VALUES
                    , ev
                };
            } else
                sendToTrans = new object[] { ID_EVENT.Unknown };

            // ������� ������������
            delegateMCApiHandler(sendToTrans);
        }

        protected override bool Disconnect()
        {
            bool result = true
                , bRes = false;

            return result;
        }

        public override void Disconnect(out int err)
        {
            err = 0;
        }

        private Modes.BusinessLogic.IGenObject addIGO (ReadOnlyCollection<Modes.BusinessLogic.IGenObject>tree, int idInnner)
        {
            foreach (Modes.BusinessLogic.IGenObject igo in tree)
            {
                //Console.WriteLine(igo.Description + " [" + igo.GenObjType.Description + "]");
                //ProcessParams(IGO);
                ProcessChilds(igo, 1, idInnner);
            }

            if ((m_listIGO.Count > 0) && (m_listIGO[m_listIGO.Count - 1].IdInner == idInnner))
                return m_listIGO[m_listIGO.Count - 1];
            else
                return null;
        }

        private Modes.BusinessLogic.IGenObject findIGO(int idInnner)
        {
            foreach (Modes.BusinessLogic.IGenObject igo in m_listIGO)
            {
                if (igo.IdInner == idInnner)
                    return igo;
                else
                    ;
            }

            return null;
        }

        //public class PPBR_Record
        //{
        //    public DateTime date_time,
        //                    wr_date_time;
        //    public string PBR_number;

        //    public double? pbr;
        //    public double? pmin;
        //    public double? pmax;

        //    //public int? idInner; //??? ��� TEC5_TG36 2 ������� (TG34 + TG56), �.�. 2 IGO

        //    public PPBR_Record()
        //    {
        //    }
        //}
        ///// <summary>
        ///// ������� - �������� ��������� �� �����-����� � ������� ������ �����
        ///// </summary>
        //public event EventHandler NewPlanValues;
        ///// <summary>
        ///// ������� - �������� ��������� �� �����-����� �� ��������� �������� ��������(�����������) �����
        ///// </summary>
        //public event EventHandler ReloadPlanValues;
        ///// <summary>
        ///// ������� - �������� ��������� �� �����-����� �� ��������� ������� ������������
        ///// </summary>
        //public event EventHandler GenObjectModified;

        protected override bool GetData(DataTable table, object query)
        {
            bool result = false;

            int i = -1;
            Modes.BusinessLogic.IGenObject igo;
            Operation oper = Operation.Unknown;

            ////������� �1
            //PPBR_Record ppbr = null;
            //SortedList<DateTime, PPBR_Record> srtListPPBR = new SortedList<DateTime,PPBR_Record> ();
            //������� �2
            DataRow []ppbr_rows = null;

            string pbr_number = string.Empty
                , pbr_indx;
            DateTime date = DateTime.MinValue;
            string[] args = null
                , idsInner = null;
            bool valid = false;
            int idInner = -1;
            IList<PlanValueItem> listPVI = null;
            IList<Modes.BusinessLogic.IMaketEquipment> listEquip;
            DateTime dateCurrent;

            table.Reset();
            table.Locale = System.Globalization.CultureInfo.CurrentCulture;

            args = ((string)query).Split (';');
            Enum.TryParse<Operation> (args [0], out oper);

            //Logging.Logg().Debug("DbMCInterface::GetData () - " + query + "...", Logging.INDEX_MESSAGE.NOT_SET);

            switch (oper)
            {
                case Operation.InitIGO:
                    //InitIGO (arg);
                    break;
                case Operation.MaketEquipment:
                    table.Columns.Add ("ID", typeof (int));
                    table.Columns.Add ("ID_INNER", typeof (int));
                    table.Columns.Add ("WR_DATE_TIME", typeof (DateTime));

                    date = DateTime.FromOADate (Double.Parse (args [2]));
                    idsInner = args [1].Split (',');

                    listEquip = m_MCApi.GetMaket53500Equipment (Array.ConvertAll<string, Guid>(idsInner, Guid.Parse));

                    listEquip.ToList ().ForEach (equip => {
                        equip.GenTree.ToList ().ForEach (item => {
                            igo = findIGO (item.IdInner);

                            Logging.Logg ().Debug ($@"DbMCInterface::GetData () - equip=[{equip.Mrid}], item.IdIner={item.IdInner}, item.GenObjType.Id={item.GenObjType.Id}...", Logging.INDEX_MESSAGE.NOT_SET);

                            if (Equals (igo, null) == false)
                                if (item.GenObjType.Id == 3)
                                    m_listIGO.Add (item);
                                else
                                    ;
                            else
                                ;

                            if (Equals (igo, null) == false)
                                table.Rows.Add (new object [] { igo.Id, igo.IdInner, HDateTime.ToMoscowTimeZone () });
                            else
                                ;
                        });
                    });
                    break;
                case Operation.PPBR:
                    table.Columns.Add("DATE_PBR", typeof (DateTime));
                    table.Columns.Add("WR_DATE_TIME", typeof(DateTime));
                    table.Columns.Add("PBR", typeof(double));
                    table.Columns.Add("Pmin", typeof(double));
                    table.Columns.Add("Pmax", typeof(double));
                    table.Columns.Add("PBR_NUMBER", typeof(string));
                    //table.Columns.Add("ID_COMPONENT", typeof(Int32));

                    date = DateTime.FromOADate(Double.Parse(args[2]));
                    idsInner = args[1].Split(',');

                    for (i = 0; i < idsInner.Length; i ++)
                    {
                        valid = Int32.TryParse(idsInner[i], out idInner);
                        
                        if (valid == false)
                            continue;
                        else
                            ;

                        igo = findIGO(idInner);

                        if (igo == null)
                            igo = addIGO (m_MCTimeSlice.GenTree, idInner);
                        else
                            ;

                        if (!(igo == null)) {
                            try {
                                listPVI = m_MCApi.GetPlanValuesActual(date.LocalHqToSystemEx(), date.AddDays(1).LocalHqToSystemEx(), igo);

                                result = true;
                            } catch (Exception e) {
                                Logging.Logg().Exception(e, string.Format(@"DbMCInterface::GetData () - GetPlanValuesActual () - ��������� �������� ��� '{0}', [IdInner={1}]..."
                                        , igo.Description, igo.IdInner)
                                    , Logging.INDEX_MESSAGE.NOT_SET);

                                getData_OnFillError (m_MCApi, new FillErrorEventArgs (table, new object [] { }));

                                result = false;
                            }

                            if (result == true)
                                if (listPVI.Count == 0)
                                    Logging.Logg ().Warning(string.Format("DbMCInterface::GetData () - GetPlanValuesActual () - ��� ���������� ��������� ��� '{0}', [ID={1}]..."
                                            , igo.Description, igo.IdInner)
                                        , Logging.INDEX_MESSAGE.NOT_SET);
                                else
                                    ;
                            else
                                //������ ��������� ��������!
                                break;

                            foreach (PlanValueItem pvi in listPVI.OrderBy(RRR => RRR.DT))
                            {
                                //Console.WriteLine("    " + pvi.DT.SystemToLocalHqEx().ToString() + " " +
                                //                            pvi.Type.ToString() + " [" + m_listPFI[pvi.ObjFactor].Description + "] " +
                                //                            /*it.ObjName ��� id ������������� �������*/
                                //                            m_listPFI[pvi.ObjFactor].Name + " =" + pvi.Value.ToString());

                                dateCurrent = pvi.DT.SystemToLocalHqEx();
                                pbr_number = pvi.Type.ToString().IndexOf(StatisticCommon.HAdmin.PBR_PREFIX) < 0 ? StatisticCommon.HAdmin.PBR_PREFIX + pvi.Type.ToString() : pvi.Type.ToString();

                                //��������� ������ � ������� ����������� �� ��� �����
                                ////������� �1
                                //if (srtListPPBR.ContainsKey(dateCurrent))
                                //    ppbr = srtListPPBR.First(item => item.Key == dateCurrent).Value;
                                //else
                                //    ;

                                //������� �2
                                if (table.Rows.Count > 0)
                                    ppbr_rows = table.Select("DATE_PBR='" + dateCurrent.ToString() + "'");
                                else
                                    ;

                                //��������� ��������� ������
                                ////������� �1
                                //if (ppbr == null)
                                //{
                                //    ppbr = new PPBR_Record();
                                //    ppbr.date_time = dateCurrent;
                                //    ppbr.wr_date_time = DateTime.Now;
                                //    ppbr.PBR_number = pvi.Type.ToString();
                                //    //ppbr.idInner = igo.IdInner; //??? ��� TEC5_TG36 2 ������� (TG34 + TG56), �.�. 2 IGO

                                //    //����� ���������� ����� ���������� �������������� ��������� ������ - � ��������� �� �� ����� ������ ��������.
                                //    srtListPPBR.Add(dateCurrent, ppbr);
                                //}
                                //else
                                //    ;

                                //������� �2, 3
                                if ((ppbr_rows == null)
                                    || (ppbr_rows.Length == 0))
                                {
                                    table.Rows.Add(new object[] { dateCurrent, DateTime.Now, 0F, 0F, 0F, pbr_number/*, igo.IdInner*/});
                                    ppbr_rows = table.Select($"DATE_PBR='{dateCurrent.ToString()}'");
                                }
                                else
                                    ;

                                ppbr_rows[0]["PBR_NUMBER"] = pbr_number;

                                switch (m_listPFI[pvi.ObjFactor].Id)
                                {
                                    case 0:
                                        //else.ppbr.pbr = pvi.Value; //������� �1
                                        //ppbr_rows [0]["PBR"] = (double)ppbr_rows [0]["PBR"] + pvi.Value; //������� �2
                                        pbr_indx = "PBR"; //������� �3
                                        break;
                                    case 1:
                                        //ppbr.pmin = pvi.Value; //������� �1
                                        //ppbr_rows [0]["PMIN"] = (double)ppbr_rows [0]["PMIN"] + pvi.Value; //������� �2
                                        pbr_indx = "PMIN"; //������� �3
                                        break;
                                    case 2:
                                        //ppbr.pmax = pvi.Value; //������� �1
                                        //ppbr_rows [0]["PMAX"] = (double)ppbr_rows [0]["PMAX"] + pvi.Value; //������� �2
                                        pbr_indx = "PMAX"; //������� �3
                                        break;
                                    default:
                                        pbr_indx = string.Empty;
                                        break;
                                }

                                if (string.IsNullOrWhiteSpace (pbr_indx) == false)
                                    try {
                                        ppbr_rows [0] [pbr_indx] = (double)ppbr_rows [0] [pbr_indx] + pvi.Value; //������� �3
                                    } catch (Exception e) {
                                        Logging.Logg ().Exception (e, $"DbMCInterface::GetData() - ���{ppbr_rows [0] [pbr_indx].GetType().Name} �������� �� �������={m_listPFI [pvi.ObjFactor].Id}...", Logging.INDEX_MESSAGE.NOT_SET);
                                    }
                                else
                                    Logging.Logg ().Error (string.Format ("DbMCInterface::GetData() - �� ������ ������={0} �������� ���...", m_listPFI [pvi.ObjFactor].Id)
                                        , Logging.INDEX_MESSAGE.NOT_SET);
                            }
                        }
                        else
                            result = false; //igo == null
                    }
                    break;
                default:
                    break;
            }

            return result;
        }

        void ProcessChilds(Modes.BusinessLogic.IGenObject IGO, int Level, int idInner)
        {
            foreach (Modes.BusinessLogic.IGenObject IGOch in IGO.Children)
            {
                if (!(IGOch.GenObjType.Id == 15))      //������������ ���� ��� ��������� - �� ��� ��� �� ����������, �� �������� ���������
                {
                    //Console.WriteLine(new System.String('-', Level) + IGOch.Description + " [" + IGOch.GenObjType.Description + "]  P:" + IGOch.VarParams.Count.ToString() + " Id:" + IGOch.Id.ToString() + " IdInner:" + IGOch.IdInner.ToString());
                    //ProcessParams(IGOch);
                    if ((IGOch.GenObjType.Id == 3)
                        && (IGOch.IdInner == idInner))
                    {
                        //� ������������ ���� �������������� (id=1) ��� ���������� - ������ �������� ��������
                        m_listIGO.Add (IGOch);
                    }
                    else
                        ;

                    ProcessChilds(IGOch, Level + 1, idInner);
                }
                else
                    ;
            }
        }

        private bool InitIGO (string ids)
        {
            bool bRes = false;

            string [] arId = ids.Split (',');

            foreach (string id in arId)
            {
                foreach (Modes.BusinessLogic.IGenObject IGO in m_MCTimeSlice.GenTree)
                {
                    Console.WriteLine(IGO.Description + " [" + IGO.GenObjType.Description + "]");
                    //ProcessParams(IGO);
                    ProcessChilds(IGO, 1, Convert.ToInt32 (id));
                }
            }

            return bRes;
        }

        protected override void GetDataCancel ()
        {
            //TODO: ��� ���������� ��������� ������������� ����� 
            // m_MCApi.GetPlanValuesActual
        }
    }
}
