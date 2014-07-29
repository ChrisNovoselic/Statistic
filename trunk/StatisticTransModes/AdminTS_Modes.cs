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
        public AdminTS_Modes(HReports rep, bool[] arMarkPPBRValues)
            : base(rep, arMarkPPBRValues)
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

                Logging.Logg().LogDebugToFile("AdminTS_MC::SaveChanges () - states.Clear()");

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
                    Logging.Logg().LogDebugToFile("catch - SaveChanges () - semaState.Release(1)");
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
