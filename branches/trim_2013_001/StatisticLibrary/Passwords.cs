using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Data.OleDb;
using System.IO;
using MySql.Data.MySqlClient;
using System.Threading;
using System.Globalization;

namespace StatisticCommon
{    
    public class Passwords : object
    {
        private MD5CryptoServiceProvider md5;

        private DelegateFunc delegateStartWait;
        private DelegateFunc delegateStopWait;
        private DelegateFunc delegateEventUpdate;

        private DelegateStringFunc errorReport;
        private DelegateStringFunc actionReport;

        private string getOwnerPass () {
            string[] ownersPass = { "диспетчера", "администратора", "НССа" };

            return ownersPass [m_idPass - 1];            
        }

        private bool is_connection_error;
        private bool is_data_error;

        public volatile string last_error;
        public DateTime last_time_error;
        public volatile bool errored_state;

        public volatile string last_action;
        public DateTime last_time_action;
        public volatile bool actioned_state;

        private Semaphore semaGetPass;
        private Semaphore semaSetPass;
        private volatile HAdmin.Errors passResult;
        private volatile string passReceive;
        private volatile uint m_idPass;
        private Object m_lockObj;

        private Thread taskThread;
        private Semaphore semaState;
        public volatile bool threadIsWorking;
        private volatile bool newState;
        private volatile List<StatesMachine> states;

        private List <DbInterface> m_listDbInterfaces;
        private List <int> m_listListenerIdCurrent;
        private int m_indxDbInterfaceCurrent; //Индекс в списке 'm_listDbInterfaces'

        public ConnectionSettings connSettConfigDB;
        int m_indxDbInterfaceConfigDB,
            m_listenerIdConfigDB;

        private enum StatesMachine
        {
            GetPass,
            SetPassInsert,
            SetPassUpdate,
        }

        private bool started;

        //public bool isActive;

        public Passwords()
        {
            Initialize ();
        }

        private void Initialize () {
            m_listDbInterfaces = new List<DbInterface>();
            m_listListenerIdCurrent = new List<int>();

            started = false;

            md5 = new MD5CryptoServiceProvider();

            is_data_error = is_connection_error = false;

            m_lockObj = new Object();

            semaGetPass = new Semaphore(1, 1);
            semaSetPass = new Semaphore(1, 1);

            states = new List<StatesMachine>();
        }

        public void Start()
        {
            if (started)
                ;
            else
                started = true;
        }

        public void Reinit()
        {
            if (!started)
                return;
            else
                ;

            InitDbInterfaces ();

            lock (m_lockObj)
            {
            }
        }

        public void Stop()
        {
            if (!started)
                return;
            else
                ;

            started = false;
        }

        public void SetDelegateWait(DelegateFunc dStart, DelegateFunc dStop, DelegateFunc dStatus)
        {
            this.delegateStartWait = dStart;
            this.delegateStopWait = dStop;
            this.delegateEventUpdate = dStatus;
        }

        public void SetDelegateReport(DelegateStringFunc ferr, DelegateStringFunc fact)
        {
            this.errorReport = ferr;
            this.actionReport = fact;
        }

        void MessageBox(string msg, MessageBoxButtons btn = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Error)
        {
            //MessageBox.Show(this, msg, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

            Logging.Logg().LogLock();
            Logging.Logg().LogToFile(msg, true, true, false);
            Logging.Logg().LogUnlock();
        }

        public bool SetPassword(string password, uint idPass)
        {
            m_idPass = idPass;

            byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(password));

            StringBuilder hashedString = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
                hashedString.Append(hash[i].ToString("x2"));

            semaGetPass.WaitOne();
            lock (m_lockObj)
            {
                passResult = HAdmin.Errors.NoAccess;

                newState = true;
                states.Clear();
                states.Add(StatesMachine.GetPass);

                try
                {
                    semaState.Release(1);
                }
                catch
                {
                    Logging.Logg().LogLock();
                    Logging.Logg().LogToFile("catch - SetPassword () - semaState.Release(1)", true, true, false);
                    Logging.Logg().LogUnlock();
                }
            }
            delegateStartWait();
            semaGetPass.WaitOne();
            try
            {
                semaGetPass.Release(1);
            }
            catch
            {
            }

            if (passResult != HAdmin.Errors.NoError)
            {
                delegateStopWait();
                
                //MessageBox.Show(this, "Ошибка получения пароля " + getOwnerPass () + ". Пароль не сохранён.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox("Ошибка получения пароля " + getOwnerPass() + ". Пароль не сохранён.");
                
                return false;
            }


            semaSetPass.WaitOne();
            lock (m_lockObj)
            {
                passResult = HAdmin.Errors.NoAccess;

                newState = true;
                states.Clear();

                if (passReceive == null)
                    states.Add(StatesMachine.SetPassInsert);
                else
                    states.Add(StatesMachine.SetPassUpdate);

                if (password != "")
                    passReceive = hashedString.ToString();

                try
                {
                    semaState.Release(1);
                }
                catch
                {
                    Logging.Logg().LogLock();
                    Logging.Logg().LogToFile("catch - SetPassword () - semaState.Release(1)", true, true, false);
                    Logging.Logg().LogUnlock();
                }
            }
            semaSetPass.WaitOne();
            try
            {
                semaSetPass.Release(1);
            }
            catch
            {
            }
            delegateStopWait();

            if (passResult != HAdmin.Errors.NoError)
            {
                //MessageBox.Show(this, "Ошибка сохранения пароля " + getOwnerPass () + ". Пароль не сохранён.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox("Ошибка сохранения пароля " + getOwnerPass() + ". Пароль не сохранён.");                
                return false;
            }

            return true;
        }

        public HAdmin.Errors ComparePassword(string password, uint id)
        {
            if (semaState == null)
                return HAdmin.Errors.NoAccess;
            else
                ;

            if (password.Length < 1)
            {
                //MessageBox.Show(this, "Длина пароля меньше допустимой.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox("Длина пароля меньше допустимой.");

                return HAdmin.Errors.InvalidValue;
            }
            else
                ;

            m_idPass = id;

            string hashFromForm = "";
            byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(password));

            StringBuilder hashedString = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
                hashedString.Append(hash[i].ToString("x2"));

            delegateStartWait();
            semaGetPass.WaitOne();
            lock (m_lockObj)
            {
                passResult = HAdmin.Errors.NoAccess;

                newState = true;
                states.Clear();
                states.Add(StatesMachine.GetPass);

                try
                {
                    semaState.Release(1);
                }
                catch
                {
                    Logging.Logg().LogLock();
                    Logging.Logg().LogToFile("catch - ComparePassword () - semaState.Release(1)", true, true, false);
                    Logging.Logg().LogUnlock();
                }
            }
            semaGetPass.WaitOne();
            try
            {
                semaGetPass.Release(1);
            }
            catch
            {
            }

            //???
            //passResult = Errors.NoError;

            delegateStopWait();
            if (passResult != HAdmin.Errors.NoError)
            {                
                //MessageBox.Show(this, "Ошибка получения пароля " + getOwnerPass () + ".", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox("Ошибка получения пароля " + getOwnerPass() + ".");

                return HAdmin.Errors.ParseError;
            }

            if (passReceive == null)
            {
                //MessageBox.Show(this, "Пароль не установлен.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox("Пароль не установлен.");

                return HAdmin.Errors.NoSet;
            }
            else
            {
                hashFromForm = hashedString.ToString();
             
                if (hashFromForm != passReceive)
                {
                    //MessageBox.Show(this, "Пароль введён неверно.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    MessageBox("Пароль введён неверно.");

                    return HAdmin.Errors.InvalidValue;
                }
                else
                    return HAdmin.Errors.NoError;
            }
        }

        private void GetPassRequest(uint id)
        {
            string request = "SELECT HASH FROM passwords WHERE ID_ROLE=" + id;
            
            Request(m_indxDbInterfaceConfigDB, m_listenerIdConfigDB, request);
        }

        private bool GetPassResponse(DataTable table)
        {
            if (table.Rows.Count != 0)
                try
                {
                    if (table.Rows[0][0] is System.DBNull)
                        passReceive = "";
                    else
                        passReceive = (string)table.Rows[0][0];
                }
                catch
                {
                    return false;
                }
            else
                passReceive = null;

            return true;
        }

        private void SetPassRequest(string password, uint id, bool insert)
        {
            string query = string.Empty;
            
            if (insert)
                switch (m_idPass) {
                    case 1:
                        query = "INSERT INTO passwords (ID_ROLE, HASH) VALUES (" + id + ", '" + password + "')";
                        break;
                    case 2:
                        query = "INSERT INTO passwords (ID_ROLE, HASH) VALUES (" + id + ", '" + password + "')";
                        break;
                    case 3:
                        query = "INSERT INTO passwords (ID_ROLE, HASH) VALUES (" + id + ", '" + password + "')";
                        break;
                    default:
                        break;
                }
            else {
                switch (m_idPass)
                {
                    case 1:
                        query = "UPDATE passwords SET HASH='" + password + "'";
                        break;
                    case 2:
                        query = "UPDATE passwords SET HASH='" + password + "'";
                        break;
                    case 3:
                        query = "UPDATE passwords SET HASH='" + password + "'";
                        break;
                    default:
                        break;
                }

                query += " WHERE ID_USER=ID_ROLE AND ID_ROLE=" + id;
            }

            Request(m_indxDbInterfaceConfigDB, m_listenerIdConfigDB, query);
        }

        private void ErrorReport(string error_string)
        {
            last_error = error_string;
            last_time_error = DateTime.Now;
            errored_state = true;
            
            errorReport (error_string);
        }

        private void ActionReport(string action_string)
        {
            last_action = action_string;
            last_time_action = DateTime.Now;
            actioned_state = true;

            //stsStrip.BeginInvoke(delegateEventUpdate);
            //delegateEventUpdate ();
            actionReport(action_string);
        }

        public void Request(int indxDbInterface, int listenerId, string request)
        {
            m_indxDbInterfaceCurrent = indxDbInterface;
            m_listListenerIdCurrent[indxDbInterface] = listenerId;
            m_listDbInterfaces[indxDbInterface].Request(m_listListenerIdCurrent[indxDbInterface], request);
        }

        public bool GetResponse(int indxDbInterface, int listenerId, out bool error, out DataTable table/*, bool isTec*/)
        {
            if ((!(m_indxDbInterfaceCurrent < 0)) && (m_listListenerIdCurrent.Count > 0) && (!(m_indxDbInterfaceCurrent < 0))) {
                //m_listListenerIdCurrent [m_indxDbInterfaceCurrent] = -1;
                //m_indxDbInterfaceCurrent = -1;
                ;
            }
            else
                ;

            return m_listDbInterfaces[indxDbInterface].GetResponse(listenerId, out error, out table);

            //if (isTec)
            //    return dbInterface.GetResponse(listenerIdTec, out error, out table);
            //else
            //    return dbInterface.GetResponse(listenerIdAdmin, out error, out table);
        }

        private bool InitDbInterfaces () {
            bool bRes = true;
            
            m_listDbInterfaces.Clear ();

            m_listListenerIdCurrent.Clear();
            m_indxDbInterfaceCurrent = -1;

            m_listDbInterfaces.Add(new DbTSQLInterface(DbTSQLInterface.DB_TSQL_INTERFACE_TYPE.MySQL, "Интерфейс MySQL-БД: Конфигурация"));
            m_listListenerIdCurrent.Add(-1);

            m_indxDbInterfaceConfigDB = m_listDbInterfaces.Count - 1;
            m_listenerIdConfigDB = m_listDbInterfaces[m_indxDbInterfaceConfigDB].ListenerRegister();

            m_listDbInterfaces[m_listDbInterfaces.Count - 1].Start();

            m_listDbInterfaces[m_listDbInterfaces.Count - 1].SetConnectionSettings(connSettConfigDB);

            return bRes;
        }

        public void StartDbInterface()
        {
            InitDbInterfaces ();

            threadIsWorking = true;

            taskThread = new Thread (new ParameterizedThreadStart(Passwords_ThreadFunction));
            taskThread.Name = "Интерфейс получения пароля (Passwords.cs)";
            taskThread.IsBackground = true;

            semaState = new Semaphore(1, 1);

            semaState.WaitOne();
            taskThread.Start();
        }

        public void StopDbInterface()
        {
            bool joined;
            threadIsWorking = false;
            lock (m_lockObj)
            {
                newState = true;
                states.Clear();
                errored_state = false;
            }

            if ((!(taskThread == null)) && (taskThread.IsAlive))
            {
                try { semaState.Release(1); }
                catch {
                    Logging.Logg().LogLock();
                    Logging.Logg().LogToFile("catch - StopDbInterface () - semaState.Release(1)", true, true, false);
                    Logging.Logg().LogUnlock();
                }

                joined = taskThread.Join(1000);
                if (!joined)
                    taskThread.Abort();
                else
                    ;
            }
            else ;

            if ((m_listDbInterfaces.Count > 0) && (!(m_indxDbInterfaceConfigDB < 0)) && (!(m_listenerIdConfigDB < 0)))
            {
                m_listDbInterfaces[m_indxDbInterfaceConfigDB].ListenerUnregister(m_listenerIdConfigDB);
                m_indxDbInterfaceConfigDB = -1;
                m_listenerIdConfigDB = -1;

                foreach (DbInterface dbi in m_listDbInterfaces)
                {
                    dbi.Stop ();
                }
            }
            else
                ;
        }

        private bool StateRequest(StatesMachine state)
        {
            bool result = true;
            switch (state)
            {
                case StatesMachine.GetPass:
                    ActionReport("Получение пароля " + getOwnerPass () + ".");

                    GetPassRequest(m_idPass);
                    break;
                case StatesMachine.SetPassInsert:
                    ActionReport("Сохранение пароля " + getOwnerPass() + ".");
                    
                    SetPassRequest(passReceive, m_idPass, true);
                    break;
                case StatesMachine.SetPassUpdate:
                    ActionReport("Обновление пароля " + getOwnerPass() + ".");

                    SetPassRequest(passReceive, m_idPass, false);
                    break;
                default:
                    break;
            }

            return result;
        }

        private bool StateCheckResponse(StatesMachine state, out bool error, out DataTable table)
        {
            bool bRes = false;

            error = true;
            table = null;

            if ((!(m_indxDbInterfaceCurrent < 0)) && (m_listListenerIdCurrent.Count > 0))
            {
                switch (state)
                {
                    case StatesMachine.GetPass:
                        bRes = GetResponse(m_indxDbInterfaceCurrent, m_listListenerIdCurrent[m_indxDbInterfaceCurrent], out error, out table/*, false*/);
                        break;
                    case StatesMachine.SetPassInsert:
                    case StatesMachine.SetPassUpdate:
                        bRes = GetResponse(m_indxDbInterfaceCurrent, m_listListenerIdCurrent[m_indxDbInterfaceCurrent], out error, out table/*, true*/);
                        break;
                    default:
                        break;
                }
            }
            else {
                //Ошибка???

                error = true;
                table = null;

                bRes = false;
            }

            return bRes;
        }

        private bool StateResponse(StatesMachine state, DataTable table)
        {
            bool result = false;
            switch (state)
            {
                case StatesMachine.GetPass:
                    result = GetPassResponse(table);
                    if (result)
                    {
                        passResult = HAdmin.Errors.NoError;
                        try
                        {
                            semaGetPass.Release(1);
                        }
                        catch
                        {
                        }
                    }
                    break;
                case StatesMachine.SetPassInsert:
                    passResult = HAdmin.Errors.NoError;
                    try
                    {
                        semaSetPass.Release(1);
                    }
                    catch
                    {
                    }
                    result = true;
                    if (result)
                    {
                    }
                    break;
                case StatesMachine.SetPassUpdate:
                    passResult = HAdmin.Errors.NoError;
                    try
                    {
                        semaSetPass.Release(1);
                    }
                    catch
                    {
                    }
                    result = true;
                    if (result)
                    {
                    }
                    break;
                default:
                    break;
            }

            if (result)
                errored_state = actioned_state = false;
            else
                ;

            return result;
        }

        private void StateErrors(StatesMachine state, bool response)
        {
            bool bClear = false;
            
            switch (state)
            {
                case StatesMachine.GetPass:
                    if (response)
                    {
                        ErrorReport("Ошибка разбора пароля " + getOwnerPass() + ". Переход в ожидание.");

                        passResult = HAdmin.Errors.ParseError;
                    }
                    else
                    {
                        ErrorReport("Ошибка получения пароля " + getOwnerPass() + ". Переход в ожидание.");

                        passResult = HAdmin.Errors.NoAccess;
                    }
                    try
                    {
                        semaGetPass.Release(1);
                    }
                    catch
                    {
                    }
                    break;
                case StatesMachine.SetPassInsert:
                case StatesMachine.SetPassUpdate:
                    ErrorReport("Ошибка сохранения пароля " + getOwnerPass() + ". Переход в ожидание.");

                    passResult = HAdmin.Errors.NoAccess;
                    try
                    {
                        semaSetPass.Release(1);
                    }
                    catch
                    {
                    }
                    break;
                default:
                    break;
            }
        }

        private void Passwords_ThreadFunction(object data)
        {
            int index;
            StatesMachine currentState;

            while (threadIsWorking)
            {
                semaState.WaitOne();

                index = 0;

                lock (m_lockObj)
                {
                    if (states.Count == 0)
                        continue;
                    currentState = states[index];
                    newState = false;
                }

                while (true)
                {
                    bool requestIsOk = true;
                    bool error = true;
                    bool dataPresent = false;
                    DataTable table = null;
                    for (int i = 0; i < DbInterface.MAX_RETRY && !dataPresent && !newState; i++)
                    {
                        if (error)
                        {
                            requestIsOk = StateRequest(currentState);
                            if (!requestIsOk)
                                break;
                            else
                                ;
                        }
                        else
                            ;

                        error = false;
                        for (int j = 0; j < DbInterface.MAX_WAIT_COUNT && !dataPresent && !error && !newState; j++)
                        {
                            System.Threading.Thread.Sleep(DbInterface.WAIT_TIME_MS);
                            dataPresent = StateCheckResponse(currentState, out error, out table);
                        }
                    }

                    if (requestIsOk)
                    {
                        bool responseIsOk = true;
                        if ((dataPresent == true) && (error == false) && (newState == false))
                            responseIsOk = StateResponse(currentState, table);
                        else
                            ;

                        if (((responseIsOk == false) || (dataPresent == false) || (error == true)) && (newState == false))
                        {
                            StateErrors(currentState, !responseIsOk);
                            lock (m_lockObj)
                            {
                                if (newState == false)
                                {
                                    states.Clear();
                                    break;
                                }
                                else
                                    ;
                            }
                        }
                        else
                            ;
                    }
                    else
                    {
                        lock (m_lockObj)
                        {
                            if (newState == false)
                            {
                                states.Clear();
                                break;
                            }
                            else
                                ;
                        }
                    }

                    index++;

                    lock (m_lockObj)
                    {
                        if (index == states.Count)
                            break;
                        else
                            ;

                        if (newState)
                            break;
                        else
                            ;
                        currentState = states[index];
                    }
                }
            }
            try
            {
                semaState.Release(1);
            }
            catch (System.Threading.SemaphoreFullException e) //(Exception e)
            {
                Logging.Logg().LogLock();
                Logging.Logg().LogToFile("catch - Passwords_ThreadFunction () - semaState.Release(1)", true, true, false);
                Logging.Logg().LogToFile("Исключение обращения к переменной (semaState)", true, true, false);
                Logging.Logg().LogToFile("Исключение: " + e.Message, false, false, false);
                Logging.Logg().LogToFile(e.ToString(), false, false, false);
                Logging.Logg().LogUnlock();
            }
        }
    }
}
