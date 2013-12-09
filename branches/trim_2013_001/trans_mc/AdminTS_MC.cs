using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;

using StatisticCommon;

namespace trans_mc
{
    class AdminTS_MC : AdminTS
    {
        public AdminTS_MC()
            : base()
        {
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        public override Errors SaveChanges()
        {
            delegateStartWait();
            semaDBAccess.WaitOne();
            lock (m_lockObj)
            {
                saveResult = Errors.NoAccess;
                saving = true;
                using_date = false;
                m_curDate = m_prevDate;

                newState = true;
                states.Clear();

                Logging.Logg().LogLock();
                Logging.Logg().LogToFile("AdminTS_MC::SaveChanges () - states.Clear()", true, true, false);
                Logging.Logg().LogUnlock();

                states.Add((int)StatesMachine.CurrentTime);
                //states.Add((int)StatesMachine.AdminDates);
                //??? Состояния позволяют НАЧать процесс разработки возможности редактирования ПЛАНа на вкладке 'Редактирование ПБР'
                states.Add((int)StatesMachine.PPBRDates);
                //states.Add((int)StatesMachine.SaveAdminValues);
                states.Add((int)StatesMachine.SavePPBRValues);
                //states.Add((int)StatesMachine.UpdateValuesPPBR);

                try
                {
                    semaState.Release(1);
                }
                catch
                {
                    Logging.Logg().LogLock();
                    Logging.Logg().LogToFile("catch - SaveChanges () - semaState.Release(1)", true, true, false);
                    Logging.Logg().LogUnlock();
                }
            }

            semaDBAccess.WaitOne();
            try
            {
                semaDBAccess.Release(1);
            }
            catch
            {
            }
            delegateStopWait();
            saving = false;

            return saveResult;
        }
    }
}
