using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;

using StatisticCommon;

namespace StatisticTransModes
{
    public class AdminTS_Modes : AdminTS
    {
        public AdminTS_Modes(bool[] arMarkPPBRValues)
            : base(arMarkPPBRValues)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        public override Errors SaveChanges()
        {
            Logging.Logg().Debug("AdminTS_Modes::SaveChanges () - вХод...");

            try { delegateStartWait(); }
            catch (Exception e) {
                Logging.Logg().Exception(e, @"AdminTS_Modes::SaveChanges () - delegateStartWait() - ...");
            }

            int msecWaitSemaDbAccess = DbInterface.MAX_RETRY * DbInterface.MAX_WAIT_COUNT * DbInterface.WAIT_TIME_MS;
            Logging.Logg().Debug("AdminTS_Modes::SaveChanges () - delegateStartWait() - Интервал ожидания для semaDBAccess=" + msecWaitSemaDbAccess);

            if (semaDBAccess.WaitOne(msecWaitSemaDbAccess) == true)
            //if ((semaState.WaitOne(msecWaitSemaDbAccess) == true) && (semaDBAccess.WaitOne(msecWaitSemaDbAccess) == true))
            {
                lock (m_lockState)
                {
                    ClearStates();

                    saveResult = Errors.NoAccess;
                    saving = true;
                    using_date = false;
                    m_curDate = m_prevDate;

                    Logging.Logg().Debug("AdminTS_Modes::SaveChanges () - states.Clear()");

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
                        Logging.Logg().Debug("AdminTS_Modes::SaveChanges () - semaState.Release(1)");
                    }
                }

                Logging.Logg().Debug("AdminTS_Modes::SaveChanges () - semaDBAccess.WaitOne()=" + semaDBAccess.WaitOne(DbInterface.MAX_WATING).ToString());
                try
                {
                    semaDBAccess.Release(1);
                }
                catch
                {
                }

                saving = false;

                saveComplete ();
            }
            else {
                Logging.Logg().Debug("AdminTS_Modes::SaveChanges () - semaDBAccess.WaitOne()=false");

                saveResult = Errors.NoAccess;
                saving = true;
            }

            try { delegateStopWait(); }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, @"AdminTS_Modes::SaveChanges () - delegateStopWait() - ...");
            }

            return saveResult;
        }
    }
}
