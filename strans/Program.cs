using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

using StatisticTrans;

using StatisticCommon;
using ASUTP.Database;
using ASUTP.Helper;
using ASUTP;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.Configuration;
using System.ServiceModel.Description;
using System.Reflection;

namespace strans
{
    class Program
    {
        static bool g_bList;
        static bool g_bWriteToWinEventLog;
        static DateTime g_dtList;

        public static FileINI m_fileINI = new FileINI("setup.ini", false);
        private static string m_strProgramNameSectionDB_INI = "Параметры соединения с БД (" + ProgramBase.AppName + @".exe" + ")";
        private static ASUTP.Core.Crypt m_crypt;

        static void Main(string[] args)
        {
            ServiceHost svc = null;
            Type serviceType;
            STrans.Service.ModesTerminale.ServiceModesTerminale svcSingleInstanse = null;
            IList<ChannelEndpointElement> ePointChannels = null;
            string addresses = string.Empty;

            Logging.SetMode (Logging.LOG_MODE.FILE_EXE);

            FileAppSettings.RequiredForced = false;
            string [] nameEndPointServices =  FileAppSettings.This ().GetValue ("NameEndPointServices").Split(new char [] { ';' });

            List<Type> serviceTypes = new List<Type> () {
                typeof(STrans.Service.ModesTerminale.ServiceModesTerminale)
                , typeof(STrans.Service.ModesCentre.ServiceModesCentre)
            };

            try {
                Assembly mainAsm = Assembly.GetEntryAssembly ();
                mainAsm.
                yield (mainAsm);

                ClientSection clientSection = (ClientSection)ConfigurationManager.GetSection ("system.serviceModel/client");
                ePointChannels = clientSection.Endpoints.Cast<ChannelEndpointElement> ().ToList ();

                Assembly asm = typeof (StatisticTrans.Contract.ModesTerminale.IServiceModesTerminale).Assembly;
                Type contractType = asm.GetType (ePointChannels [0].Contract);

                serviceType = serviceTypes.Find (type => {
                    return contractType.IsAssignableFrom (type);
                });
                if (Equals (serviceType, null) == false) {
                    svc = new ServiceHost (serviceType);
                    svc.AddServiceEndpoint (ePointChannels [0].Contract, new WSDualHttpBinding (ePointChannels [0].BindingConfiguration), ePointChannels [0].Address);

                    svc.Open ();

                    svc.Faulted += Svc_Faulted;
                    svc.Closing += Svc_Closing;

                    svcSingleInstanse = (STrans.Service.ModesTerminale.ServiceModesTerminale)svc.SingletonInstance;
                    //svcSingleInstanse.Initialize ();
                    svcSingleInstanse.Start ();

                    addresses = string.Join (";", from sep in svc.Description.Endpoints.Cast<ServiceEndpoint> () select sep.Address.Uri.AbsoluteUri);
                    Console.WriteLine ($"Service contract: {ePointChannels [0].Contract} <{svc.State}>: [{string.Join (";", addresses)}]...");
                } else {
                    //TODO:
                    Console.Write ($"service type can't found {string.Join (";", from sep in svc.Description.Endpoints.Cast<ServiceEndpoint> () select sep.Address.Uri.AbsoluteUri)}...");
                    Console.ReadKey (false);
                }
            } catch (Exception e) {
                Console.WriteLine ($"Service contract: {ePointChannels [0].Contract} opened error: {e.Message}...");

                Logging.Logg ().Exception (e, "Main - service opened...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            if (Equals (svcSingleInstanse, null) == false) {
                Console.Write ("to shutdown press any key...");
                Console.ReadKey (false); Console.WriteLine ();

                try {
                    svcSingleInstanse?.Stop ();
                    svc?.Close ();
                    Console.Write ($"Service contract: {ePointChannels [0].Contract} <{svc.State}>: [{addresses}]...");
                } catch (Exception e) {
                    Console.Write ($"Service contract: {ePointChannels [0].Contract} closed error: {e.Message}...");

                    Logging.Logg ().Exception (e, "Main - service closed...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            } else {
                Console.Write ("to exit program press any key...");
                Console.ReadKey (false);
            }
        }

        private static void Svc_Closing (object sender, EventArgs e)
        {

        }

        private static void Svc_Faulted (object sender, EventArgs e)
        {

        }

        public static bool WriteToWinEventLog
        {
            get
            {
                return g_bWriteToWinEventLog;
            }
        }

        /// <summary>
        /// Обрабатывает переданные при вызове параметры. Возвращает флаг необходимости выхода из программы.
        /// </summary>
        static bool ProcArgs(string[] args, out bool bNoWait, out bool bList, out bool bCalculatedHalfHourValues)
        {
            //Properties.Settings sett = new Properties.Settings();
            bool bDoExit = false;

            bNoWait =
            bList =
            bCalculatedHalfHourValues =
            false;

            string strProgramNameSectionINI = string.Empty;
            strProgramNameSectionINI = "Main settings (" + ProgramBase.AppName + @".exe" + ")";
            if (Boolean.TryParse(m_fileINI.ReadString(strProgramNameSectionINI, "СообщениеОтладкаЖурналОС", string.Empty), out g_bWriteToWinEventLog) == false)
                g_bWriteToWinEventLog = false;
            else
                ;

            strProgramNameSectionINI = "Параметры записи в БД (" + ProgramBase.AppName + @".exe" + ")";
            if (Boolean.TryParse(m_fileINI.ReadString(strProgramNameSectionINI, "Расчет30минЗначения", string.Empty), out bCalculatedHalfHourValues) == false)
                bCalculatedHalfHourValues = false;
            else
                ;

            if (args.Length > 0)
            {
                if (!(args[0].IndexOf("/list") < 0))
                {
                    bList = true;

                    bool bDTParse = false;
                    string[] list_args = args[0].Split('=');

                    if (list_args.Length == 2)
                    {
                        bDTParse = DateTime.TryParse(list_args[1], out g_dtList);
                    }
                    else
                        ;

                    if (bDTParse == false)
                        g_dtList = DateTime.Now.Date; //g_dtList = g_dtList.Date.LocalHqToSystemEx();
                    else
                        ;

                    Console.WriteLine("List plants to " + g_dtList.Date.ToShortDateString() + " ...");
                }
                else
                    if (!(args[0].IndexOf("/nowait") < 0))
                    {
                        bNoWait = true;
                    }
                    else
                        if (!(args[0].IndexOf("/setmysqlpassword") < 0))
                        {
                            if (args.Length == 2)
                            {
                                m_fileINI.WriteString(m_strProgramNameSectionDB_INI, @"ПортСУБД", m_crypt.Encrypt (args[1], ASUTP.Core.Crypt.KEY));
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

                            string strNameHostMC = "Host Modes-Centre";
                            if (strNameHostMC == string.Empty)
                                strNameHostMC = "?not set?";
                            else
                                ;
                            Console.WriteLine("Modes-Centre API Host (with NTLM authentication): " + "Host Modes-Centre");

                            Console.WriteLine(Environment.NewLine + "Known command line arguments: /? /list[=DD.MM.YYYY] /nowait /setmysqlpassword" + Environment.NewLine);

                            messageToExit(true);

                            bDoExit = true;
                        }
            }
            else
                ;

            return bDoExit;
        }

        static void messageToExit(bool bNoWait)
        {
            if (bNoWait == false)
            {
                Console.Write("End. Press any key...");
                Console.ReadKey();
                Console.Write(Environment.NewLine);
            }
            else
                ;
        }
    }
}
