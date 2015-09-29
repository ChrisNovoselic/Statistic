﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;

using HClassLibrary;
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
            //Logging.Logg().Debug("AdminTS_Modes::SaveChanges () - вХод...", Logging.INDEX_MESSAGE.NOT_SET);

            bool bResSemaDbAccess = false;
            
            try { delegateStartWait(); }
            catch (Exception e) {
                Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"AdminTS_Modes::SaveChanges () - delegateStartWait() - ...");
            }

            int msecWaitSemaDbAccess = DbInterface.MAX_RETRY * DbInterface.MAX_WAIT_COUNT * DbInterface.WAIT_TIME_MS;
            //Logging.Logg().Debug("AdminTS_Modes::SaveChanges () - delegateStartWait() - Интервал ожидания для semaDBAccess=" + msecWaitSemaDbAccess, Logging.INDEX_MESSAGE.NOT_SET);

            bResSemaDbAccess = semaDBAccess.WaitOne(msecWaitSemaDbAccess);
            if (bResSemaDbAccess == true)
            //if ((semaState.WaitOne(msecWaitSemaDbAccess) == true) && (semaDBAccess.WaitOne(msecWaitSemaDbAccess) == true))
            {
                lock (m_lockState)
                {
                    ClearStates();

                    saveResult = Errors.NoAccess;
                    saving = true;
                    using_date = false;
                    m_curDate = m_prevDate;

                    //Logging.Logg().Debug("AdminTS_Modes::SaveChanges () - states.Clear()", Logging.INDEX_MESSAGE.NOT_SET);

                    AddState((int)StatesMachine.CurrentTime);
                    //AddState((int)StatesMachine.AdminDates);
                    //??? Состояния позволяют НАЧать процесс разработки возможности редактирования ПЛАНа на вкладке 'Редактирование ПБР'
                    AddState((int)StatesMachine.PPBRDates);
                    //AddState((int)StatesMachine.SaveAdminValues);
                    AddState((int)StatesMachine.SavePPBRValues);
                    //AddState((int)StatesMachine.UpdateValuesPPBR);

                    Run(@"AdminTS_Modes::SaveChanges ()");
                }

                bResSemaDbAccess = semaDBAccess.WaitOne(DbInterface.MAX_WATING);
                //Logging.Logg().Debug("AdminTS_Modes::SaveChanges () - semaDBAccess.WaitOne(" + DbInterface.MAX_WATING + @")=" + bResSemaDbAccess.ToString(), Logging.INDEX_MESSAGE.NOT_SET);

                try
                {
                    semaDBAccess.Release(1);
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"AdminTS_Modes::SaveChanges () - semaDBAccess.Release(1)");
                }

                saving = false;

                if (!(saveComplete == null)) saveComplete(); else ;
            }
            else {
                Logging.Logg().Debug("AdminTS_Modes::SaveChanges () - semaDBAccess.WaitOne(" + msecWaitSemaDbAccess + @")=false", Logging.INDEX_MESSAGE.NOT_SET);

                saveResult = Errors.NoAccess;
                saving = true;
            }

            try { delegateStopWait(); }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"AdminTS_Modes::SaveChanges () - delegateStopWait() - ...");
            }

            return saveResult;
        }
    }
}
