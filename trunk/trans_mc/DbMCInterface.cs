using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Data;
using System.Threading;
using System.Data.Common;
using System.Data.OleDb;

using StatisticCommon;

using Modes;
using ModesApiExternal;

namespace trans_mc
{
    public class DbMCInterface : DbInterface
    {
        IApiExternal m_MCApi;
        Modes.BusinessLogic.IModesTimeSlice m_MCTimeSlice;
        IList <PlanFactorItem> m_listPFI;

        List <Modes.BusinessLogic.IGenObject> m_listIGO;

        public DbMCInterface(string name)
            : base(name)
        {
            m_listIGO = new List<Modes.BusinessLogic.IGenObject> ();
        }

        public override void SetConnectionSettings(object mcHostName)
        {
            lock (lockConnectionSettings) // изменение настроек подключения и выставление флага для переподключения - атомарная операция
            {
                m_connectionSettings = mcHostName;

                needReconnect = true;
            }

            SetConnectionSettings();
        }


        protected override bool Connect()
        {
            if (!(((string)m_connectionSettings).Length > 0))
                return false;
            else
                ;

            bool result = false, bRes = false;

            try
            {
                if (bRes == true)
                    return bRes;
                else
                    bRes = true;
            }
            catch (Exception e)
            {
                Logging.Logg().LogLock();
                Logging.Logg().LogToFile("Исключение обращения к переменной", true, true, false);
                Logging.Logg().LogToFile("Исключение " + e.Message, false, false, false);
                Logging.Logg().LogToFile(e.ToString(), false, false, false);
                Logging.Logg().LogUnlock();
            }

            lock (lockConnectionSettings)
            {
                if (needReconnect) // если перед приходом в данную точку повторно были изменены настройки, то подключения со старыми настройками не делаем
                    return false;
                else
                    ;
            }

            try
            {
                ModesApiFactory.Initialize((string)m_connectionSettings);

                Logging.Logg().LogLock();
                Logging.Logg().LogToFile("Соединение с Modes-Centre (" + (string)m_connectionSettings + ")", true, true, false);
                Logging.Logg().LogUnlock();
            }
            catch (Exception e)
            {
                Logging.Logg().LogExceptionToFile(e, "Ошибка соединения с Modes-Centre (" + (string)m_connectionSettings + ")");
            }

            bRes = 
            result =
            ModesApiFactory.IsInitilized;

            if (bRes == true)
            {
                m_MCApi = ModesApiFactory.GetModesApi();
                m_MCTimeSlice = m_MCApi.GetModesTimeSlice(DateTime.Now.Date.LocalHqToSystemEx(), SyncZone.First, TreeContent.PGObjects, true);
                m_listPFI = m_MCApi.GetPlanFactors();
            }
            else
                ;

            return result;
        }

        protected override bool Disconnect()
        {
            bool result = true, bRes = false;

            return result;
        }

        Modes.BusinessLogic.IGenObject addIGO (int idInnner)
        {
            foreach (Modes.BusinessLogic.IGenObject igo in m_MCTimeSlice.GenTree)
            {
                Console.WriteLine(igo.Description + " [" + igo.GenObjType.Description + "]");
                //ProcessParams(IGO);
                ProcessChilds(igo, 1, idInnner);
            }

            if (m_listIGO[m_listIGO.Count - 1].IdInner == idInnner)
                return m_listIGO[m_listIGO.Count - 1];
            else
                return null;
        }

        Modes.BusinessLogic.IGenObject findIGO(int idInnner)
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

        public class PPBR_Record
        {
            public DateTime date_time,
                            wr_date_time;
            public string PBR_number;

            public double? pbr;
            public double? pmin;
            public double? pmax;
            
            public PPBR_Record()
            {
            }
        }

        protected override bool GetData(DataTable table, object query)
        {
            bool result = false;
            Modes.BusinessLogic.IGenObject igo;

            string [] args = ((string)query).Split (';');
            
            switch (args[0])
            {
                case "InitIGO":
                    //InitIGO (arg);
                    break;
                case "PPBR":
                    table.Columns.Add("DATE_PBR", typeof (DateTime));
                    table.Columns.Add("PBR", typeof(double));
                    table.Columns.Add("Pmin", typeof(double));
                    table.Columns.Add("Pmax", typeof(double));
                    table.Columns.Add("PBR_NUMBER", typeof(string));
                    table.Columns.Add("ID_COMPONENT", typeof(Int32));

                    DateTime date = DateTime.FromOADate (Double.Parse (args [2]));                    
                    igo = findIGO (Convert.ToInt32 (args [1]));
                    
                    if (igo == null)
                        igo = addIGO (Convert.ToInt32 (args [1]));
                    else
                        ;

                    if (!(igo == null)) {
                        result = true;

                        IList<PlanValueItem> listPVI = m_MCApi.GetPlanValuesActual(date.LocalHqToSystemEx(), date.AddDays(1).LocalHqToSystemEx(), igo);
                        PPBR_Record ppbr = null;
                        SortedList<DateTime, PPBR_Record> srtListPPBR = new SortedList<DateTime,PPBR_Record> ();
                        DateTime dateCurrent;

                        if (listPVI.Count == 0)
                            Console.WriteLine("    Нет параметров генерации!");
                        else
                            ;

                        foreach (PlanValueItem pvi in listPVI.OrderBy(RRR => RRR.DT))
                        {
                            Console.WriteLine("    " + pvi.DT.SystemToLocalHqEx().ToString() + " " +
                                                        pvi.Type.ToString() + " [" + m_listPFI[pvi.ObjFactor].Description + "] " +
                                                        /*it.ObjName это id генерирующего объекта*/
                                                        m_listPFI[pvi.ObjFactor].Name + " =" + pvi.Value.ToString());

                            dateCurrent = pvi.DT.SystemToLocalHqEx();

                            if (srtListPPBR.ContainsKey(dateCurrent))
                                ppbr = srtListPPBR.First(item => item.Key == dateCurrent).Value;
                            else
                                ;

                            if (ppbr == null)
                            {
                                ppbr = new PPBR_Record();
                                ppbr.date_time = dateCurrent;
                                ppbr.wr_date_time = DateTime.Now;
                                ppbr.PBR_number = pvi.Type.ToString();

                                //После добавления можно продолжать модифицировать экземпляр класса - в коллекции та же самая ссылка хранится.
                                srtListPPBR.Add(dateCurrent, ppbr);
                            }
                            else
                                ;

                            switch (m_listPFI[pvi.ObjFactor].Id)
                            {
                                case 0:
                                    ppbr.pbr = pvi.Value;
                                    break;
                                case 1:
                                    ppbr.pmin = pvi.Value;
                                    break;
                                case 2:
                                    ppbr.pmax = pvi.Value;
                                    break;
                                default:
                                    break;
                            }
                            
                            /*techsite.WritePlanValue(igo.IdInner,
                                                    pvi.DT.SystemToLocalHqEx(),
                                                    pvi.Type.ToString(),
                                                    (MySQLtechsite.Params)listPVI[pvi.ObjFactor].Id,
                                                    pvi.Value);*/
                        }
                    }
                    else
                        ;
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
                if (!(IGOch.GenObjType.Id == 15))      //Оборудование типа ГОУ исключаем - по ним нет ни параметров, ни дочерних элементов
                {
                    Console.WriteLine(new System.String('-', Level) + IGOch.Description + " [" + IGOch.GenObjType.Description + "]  P:" + IGOch.VarParams.Count.ToString() + " Id:" + IGOch.Id.ToString() + " IdInner:" + IGOch.IdInner.ToString());
                    //ProcessParams(IGOch);
                    if ((IGOch.GenObjType.Id == 3) && (IGOch.IdInner == idInner))
                    {
                        //У оборудования типа Электростанция (id=1) нет параметров - только дочерние элементы
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
    }
}
