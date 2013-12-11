using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

using Modes;
using ModesApiExternal;

using StatisticCommon;

namespace trans_mc_cmd
{
    class Program
    {
        /// <summary>
        /// Перечень параметров генерации (генерация; минимум генерации; максимум генерации)
        /// </summary>
        static IList<PlanFactorItem> LPFI;

        static MySQLtechsite techsite;
        static bool g_bList;
        static DateTime g_dtList;

        public static string m_strFileNameSetup = "setup.ini";

        static void Main(string[] args)
        {
            bool bNoWait = false;
            if (ProcArgs(args, out bNoWait, out g_bList) == true)
                return;
            else
                ;

            int iTechsiteInitialized = 0;
            
            if (g_bList == false)
            {
                Console.WriteLine(Environment.NewLine + "DB PPBR Initializing - Please Wait..." + Environment.NewLine);

                try { techsite = new MySQLtechsite(); }
                catch (Exception e)
                {
                }

                iTechsiteInitialized = techsite.Initialized;
            }
            else
                ;

            if ((iTechsiteInitialized == 0) || (g_bList == true))
            {
                Console.WriteLine(Environment.NewLine + "API Initializing - Please Wait..." + Environment.NewLine);

                ModesApiFactory.Initialize(GetNameHostModesCentre(new FileINI(m_strFileNameSetup)));

                if (ModesApiFactory.IsInitilized == true)
                {
                    IApiExternal api_ = ModesApiFactory.GetModesApi();
                    LPFI = api_.GetPlanFactors();
                    DateTime dt = DateTime.Now.Date.LocalHqToSystemEx();    //"Дата начала суток по московскому времени в формате UTC" (из документации) - так по московскому или в UTC? Правильнее - дата-время начала суток в Москве по Гринвичу.
                    //dt = DateTime.Now.Date.ToUniversalTime();               //Вот это реально в UTC, но API выдаёт ошибку - не на начало суток указывает
                    //dt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now.Date.ToUniversalTime(), TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"));    //Вот это Московское, но API его не принимает - требует в UTC
                    //dt = TimeZoneInfo.ConvertTime(DateTime.Now.Date, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"));

                    TimeSpan dtSpan = TimeSpan.Zero;
                    if (g_bList == true)
                        dtSpan = g_dtList.Date.LocalHqToSystemEx() - dt;
                    else
                        ;

                    Modes.BusinessLogic.IModesTimeSlice ts = api_.GetModesTimeSlice(dt.AddHours (dtSpan.TotalHours), SyncZone.First, TreeContent.PGObjects, true);

                    foreach (Modes.BusinessLogic.IGenObject IGO in ts.GenTree)
                    {
                        Console.WriteLine(IGO.Description + " [" + IGO.GenObjType.Description + "]");
                        ProcessParams(IGO);
                        ProcessChilds(IGO, 1, api_);
                    }

                    if (g_bList == false)
                    {
                        //А время апдейта (поле wr_date_time) Новосибирское
                        Console.WriteLine(Environment.NewLine + "Время Московское, в т.ч. при записи в БД. В поле wr_date_time - Новосибирское." + Environment.NewLine);

                        techsite.FlushDataToDatabase();
                    }
                    else
                        ;
                }
                else
                    itssAUX.PrintErrorMessage("Ошибка инициализации API Modes-Centre при обращении к сервису.");
            }
            else
            {//Не инициализировано соединение с БД Статистика
                if (iTechsiteInitialized > 0)
                {//Нет соединения с БД (конфигурации или назаначения)
                }
                else
                    switch (iTechsiteInitialized)
                    {
                        case -1:
                            itssAUX.PrintErrorMessage("Ошибка! MySQLtechsite::MySQLtechsite () - список идентификаторов ГТП пуст...");
                            break;
                        default:
                            break;
                    }
            }

            Console.WriteLine("End. Press any key.");

            if (bNoWait == false)
                Console.ReadKey();
            else
                ;
        }

        private static string GetNameHostModesCentre(FileINI fi)
        {
            return fi.ReadString("Параметры соединения с Modes-Centre (trans_mc_cmd.exe)", "ИмяСервер", string.Empty);
        }

        public static ConnectionSettings ReadConnSettFromFileINI (FileINI fileINI)
        {
            ConnectionSettings connSettRes = new ConnectionSettings();
            string strProgramNameSectionINI = "Параметры соединения с БД (trans_mc_cmd.exe)"; //MySQLtechsite.m_strNameSectionINI
            connSettRes.server = fileINI.ReadString(strProgramNameSectionINI, "IPСервер", string.Empty);
            connSettRes.dbName = fileINI.ReadString(strProgramNameSectionINI, "ИмяБД", string.Empty);
            connSettRes.userName = fileINI.ReadString(strProgramNameSectionINI, "ИмяПользователь", string.Empty);
            connSettRes.port = 3306;
            connSettRes.ignore = false;

            return connSettRes;
        }

        /// <summary>
        /// Обрабатывает переданные при вызове параметры. Возвращает флаг необходимости выхода из программы.
        /// </summary>
        static bool ProcArgs(string[] args, out bool bNoWait, out bool bList)
        {
            //Properties.Settings sett = new Properties.Settings();
            bool bDoExit = false;

            bNoWait =
            bList =
            false;

            if (args.Length > 0)
            {
                FileINI fileINI = new FileINI("setup.ini");

                if (!(args[0].IndexOf ("/list") < 0))
                {
                    bList = true;

                    bool bDTParse = false;
                    string [] list_args = args[0].Split ('=');

                    if (list_args.Length == 2)
                    {
                        bDTParse = DateTime.TryParse (list_args[1], out g_dtList);
                    }
                    else
                        ;

                    if (bDTParse == true)
                        ; //g_dtList = g_dtList.Date.LocalHqToSystemEx();
                    else
                        g_dtList = DateTime.Now.Date.LocalHqToSystemEx();
                }
                else
                    if (!(args[0].IndexOf ("/nowait") < 0))
                    {
                        bNoWait = true;
                    }
                    else
                        if (!(args[0].IndexOf ("/setmysqlpassword") < 0))
                        {
                            if (args.Length == 2)
                            {
                                ConnectionSettings connSett = ReadConnSettFromFileINI (fileINI);
                                connSett.password = args[1];

                                FormConnectionSettings formConnSett = new FormConnectionSettings (MySQLtechsite.m_strFileNameConnSett);
                                formConnSett.setConnSett(0, connSett);
                                formConnSett.btnOk_Click (null, EventArgs.Empty);
                                formConnSett.SaveSettingsFile();
                            }
                            else
                                Console.WriteLine("Укажите новый пароль вторым аргументом или аргументов больше, чем необходимо");

                            bDoExit = true;
                        }
                        else
                        {//case "/?":
                            Console.WriteLine(System.Diagnostics.FileVersionInfo.GetVersionInfo(AppDomain.CurrentDomain.SetupInformation.ApplicationName).FileDescription);
                            Console.WriteLine(System.Diagnostics.FileVersionInfo.GetVersionInfo(AppDomain.CurrentDomain.SetupInformation.ApplicationName).CompanyName);
                            Console.WriteLine(System.Diagnostics.FileVersionInfo.GetVersionInfo(AppDomain.CurrentDomain.SetupInformation.ApplicationName).LegalCopyright);

                            string strNameHostMC = GetNameHostModesCentre(fileINI);
                            if (strNameHostMC == string.Empty)
                                strNameHostMC = "?not set?";
                            else
                                ;
                            Console.WriteLine("Modes-Centre API Host (with NTLM authentication): " + GetNameHostModesCentre (fileINI));

                            Console.WriteLine(Environment.NewLine + "Known command line arguments: /? /nowait /setmysqlpassword" + Environment.NewLine);
                            Console.WriteLine("End. Press any key.");
                            
                            Console.ReadKey();
                            
                            bDoExit = true;
                        }
            }
            else
                ;

            return bDoExit;
        }

        static void ProcessParams(Modes.BusinessLogic.IGenObject IGO)
        {
            foreach (Modes.BusinessLogic.IVarParam param in IGO.VarParams)
            {
                Console.WriteLine("Параметр: " + param.Name + " [" + param.Description + "] " + param.GetValue(0));   //пройтись 0...23 - по часам величины
            }
        }

        static void ProcessChilds(Modes.BusinessLogic.IGenObject IGO, int Level, IApiExternal api_)
        {
            foreach (Modes.BusinessLogic.IGenObject IGOch in IGO.Children)
            {
                if (!(IGOch.GenObjType.Id == 15))      //Оборудование типа ГОУ исключаем - по ним нет ни параметров, ни дочерних элементов
                {
                    Console.WriteLine(new System.String('-', Level) + IGOch.Description + " [" + IGOch.GenObjType.Description + "]  P:" + IGOch.VarParams.Count.ToString() + " Id:" + IGOch.Id.ToString() + " IdInner:" + IGOch.IdInner.ToString());
                    //if (g_bList == false) ProcessParams(IGOch); else ;
                    if (IGOch.GenObjType.Id == 3)
                        //У оборудования типа Электростанция (id=1) нет параметров - только дочерние элементы
                        if (g_bList == false) GetPlanValuesActual(api_, IGOch); else ;
                    else
                        ;

                    ProcessChilds(IGOch, Level + 1, api_);
                }
                else
                    ;
            }
        }

        /// <summary>
        /// Получение актуального(действующего) планового графика на текущие сутки
        /// </summary>
        static void GetPlanValuesActual(IApiExternal api_, Modes.BusinessLogic.IGenObject IGO)
        {
            //api_.GetPlanFactors() - это список: генерация; минимум генерации; максимум генерации
            //Величины в тестовом примере читаются так: Values (это коллекция) = api_.GetPlanValuesActual(...)
            //А потом цикл по ним, и у них берётся .DT и .Value

            IList<PlanValueItem> LPVI = api_.GetPlanValuesActual(DateTime.Now.Date.LocalHqToSystemEx(), DateTime.Now.Date.AddDays(1).LocalHqToSystemEx(), IGO);

            if (LPVI.Count == 0) Console.WriteLine("    Нет параметров генерации!");

            foreach (PlanValueItem pvi in LPVI.OrderBy(RRR => RRR.DT))
            {
                Console.WriteLine("    " + pvi.DT.SystemToLocalHqEx().ToString() + " " + pvi.Type.ToString() + " [" + LPFI[pvi.ObjFactor].Description + "] " + /*it.ObjName это id генерирующего объекта*/ LPFI[pvi.ObjFactor].Name + " =" + pvi.Value.ToString());

                techsite.WritePlanValue(IGO.IdInner, pvi.DT.SystemToLocalHqEx(), pvi.Type.ToString(), (MySQLtechsite.Params)LPFI[pvi.ObjFactor].Id, pvi.Value);     //Запись в БД techsite           //02.09.2013 меняю IGO.Id на IGO.IdInner

                //pvi.DT.SystemToLocalHqEx()    даёт Московское время, как и нужно
                //Аналогичный результат даст явное преобразование TimeZoneInfo.ConvertTimeFromUtc(pvi.DT, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"))
                //"Russian Standard Time" - это Москва. "N. Central Asia Standard Time" - это Новосибирск.
                //Про конвертацию времени: http://msdn.microsoft.com/ru-ru/library/bb397769.aspx
            }

        }
    }
}
