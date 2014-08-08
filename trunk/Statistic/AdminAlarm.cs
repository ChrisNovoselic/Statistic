using System;
using System.Windows.Forms;
using System.IO;
using System.Data;

namespace StatisticCommon
{
    public class AdminAlarm : HAdmin
    {
        protected DataTable m_tablePPBRValuesResponse,
                    m_tableRDGExcelValuesResponse;

        protected enum StatesMachine
        {
            Init,
            CurrentTime,
            AdminDates, //Получение списка сохранённых часовых значений
            PPBRDates,
            Current_TM,
            AdminValues, //Получение административных данных
            PPBRValues,
        }

        protected override void Initialize () {
            base.Initialize ();
        }

        public AdminAlarm(HReports rep, bool[] arMarkSavePPBRValues)
            : base(rep)
        {
        }

        public override void Activate(bool active)
        {
            base.Activate(active);

            if ((active == true) && (threadIsWorking == 1))
                ; //GetRDGValues(m_typeFields, indxTECComponents);
            else
                ;
        }

        public override void Start()
        {
            if (!(m_list_tec == null))
                foreach (TEC t in m_list_tec)
                {
                    t.StartDbInterfaces();
                }
            else
                Logging.Logg().LogErrorToFile(@"AdminTS::Start () - m_list_tec == null");

            base.Start();
        }

        public override void Stop()
        {
            base.Stop();

            if (!(m_list_tec == null))
                foreach (TEC t in m_list_tec)
                {
                    t.StopDbInterfaces();
                }
            else
                Logging.Logg().LogErrorToFile(@"AdminTS::Stop () - m_list_tec == null");
        }

        public override bool WasChanged()
        {
            bool bRes = false;

            return bRes;
        }

        public override void  ClearValues()
        {
        }

        public override void CopyCurToPrevRDGValues()
        {
        }

        public override void getCurRDGValues(HAdmin source)
        {
        }

        protected override void GetPPBRDatesRequest(DateTime date)
        {
        }

        protected override void GetPPBRValuesRequest(TEC t, TECComponent comp, DateTime date, AdminTS.TYPE_FIELDS mode)
        {
        }

        protected override bool GetPPBRDatesResponse(System.Data.DataTable table, DateTime date)
        {
            bool bRes = true;

            return bRes;
        }

        protected override bool GetPPBRValuesResponse(System.Data.DataTable table, DateTime date)
        {
            bool bRes = true;

            return bRes;
        }

        public override void GetRDGValues(int mode, int indx, DateTime date)
        {
        }

        public override bool Response(int idListener, out bool error, out System.Data.DataTable table)
        {
            bool bRes = true;

            error = false;
            table = null;

            return bRes;
        }

        protected override bool StateCheckResponse(int state, out bool error, out System.Data.DataTable table)
        {
            bool bRes = true;

            error = false;
            table = null;

            return bRes;
        }

        protected override void StateErrors(int state, bool response)
        {
        }

        protected override bool StateRequest(int state)
        {
            bool bRes = true;

            return bRes;
        }

        protected override bool StateResponse(int state, System.Data.DataTable table)
        {
            bool bRes = true;

            return bRes;
        }
    }
}
