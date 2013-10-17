using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Statistic
{
    public partial class FormTECComponent : Form
    {
        private enum UI_CONTROL_ITEM : int { LISTBOX_ITEM,
                                                BUTTON_TEC_ADD, BUTTON_TEC_DEL,
                                                BUTTON_ITEM_ADD, BUTTON_ITEM_DEL,
                                                BUTTON_TG_ADD, BUTTON_TG_DEL,
                                                COUNT_UI_CONTROL_ITEM };
        bool[] m_arUIControlItemEnabled;
        System.Windows.Forms.Control[] m_arUIControlItem;

        private ConnectionSettings m_connectionSetttings;

        private Object m_lockObj;

        private Thread m_threadUIControl;
        private Semaphore m_semUIControl;
        private volatile bool m_bThreadUIControlIsWorking;

        private DataTable m_list_tec = null, m_list_TECComponents = null, m_list_tg = null,
                        m_list_tec_original = null;
        private DataTable[,] m_list_TECComponents_original = null;
        private DataTable[] m_list_tg_original = null;

        public FormTECComponent(ConnectionSettings connSett)
        {
            m_connectionSetttings = connSett;

            InitializeComponent();

            m_lockObj = new Object();

            m_arUIControlItem = new System.Windows.Forms.Control [(int)UI_CONTROL_ITEM.COUNT_UI_CONTROL_ITEM];
            m_arUIControlItem [(int)UI_CONTROL_ITEM.LISTBOX_ITEM] = listBoxItem;
            m_arUIControlItem[(int)UI_CONTROL_ITEM.BUTTON_TEC_ADD] = buttonTECAdd;
            m_arUIControlItem[(int)UI_CONTROL_ITEM.BUTTON_TEC_DEL] = buttonTECDel;
            m_arUIControlItem[(int)UI_CONTROL_ITEM.BUTTON_ITEM_ADD] = buttonItemAdd;
            m_arUIControlItem[(int)UI_CONTROL_ITEM.BUTTON_ITEM_DEL] = buttonItemDel;
            m_arUIControlItem[(int)UI_CONTROL_ITEM.BUTTON_TG_ADD] = buttonTGAdd;
            m_arUIControlItem[(int)UI_CONTROL_ITEM.BUTTON_TG_DEL] = buttonTGDel;

            lock (m_lockObj)
            {
                m_arUIControlItemEnabled = new bool[(int)UI_CONTROL_ITEM.COUNT_UI_CONTROL_ITEM];
                for (int i = 0; i < (int)UI_CONTROL_ITEM.COUNT_UI_CONTROL_ITEM; i++)
                {
                    m_arUIControlItemEnabled[i] = true;
                }
            }

            //Пока добавлять/удалять ТЭЦ нельзя
            m_arUIControlItemEnabled[(int)UI_CONTROL_ITEM.BUTTON_TEC_ADD] = false;
            m_arUIControlItemEnabled[(int)UI_CONTROL_ITEM.BUTTON_TEC_DEL] = false;

            m_list_tec = m_list_tec_original = InitTEC.getListTEC(m_connectionSetttings, true);
            fillListBox(listBoxTEC, m_list_tec);

            m_list_TECComponents_original = new DataTable[(int)ChangeMode.MODE_TECCOMPONENT.UNKNOWN - 1, m_list_tec_original.Rows.Count];
            m_list_tg_original = new DataTable[m_list_tec_original.Rows.Count];

            comboBoxMode.Items.Add("ТЭЦ");
            for (int i = (int)ChangeMode.MODE_TECCOMPONENT.GTP; i < (int)ChangeMode.MODE_TECCOMPONENT.UNKNOWN; i++)
            {
                comboBoxMode.Items.Add(ChangeMode.getNameMode((short)i));
            }
            comboBoxMode.SelectedIndex = 0;

            //StartThreadUIControl();
            timerUIControl.Start();
        }

        public void StartThreadUIControl()
        {
            m_bThreadUIControlIsWorking = true;

            m_threadUIControl = new Thread(new ParameterizedThreadStart(FunctionThreadUIControl));
            m_threadUIControl.Name = "Контроль интерфейса пользователя";
            m_threadUIControl.IsBackground = true;

            m_semUIControl = new Semaphore(1, 1);

            m_semUIControl.WaitOne();
            m_threadUIControl.Start();
        }

        public void StopThreadUIControl()
        {
            bool joined;
            m_bThreadUIControlIsWorking = false;
            lock (m_lockObj)
            {
            }

            if (m_threadUIControl.IsAlive)
            {
                try
                {
                    m_semUIControl.Release(1);
                }
                catch
                {
                }

                joined = m_threadUIControl.Join(1000);
                if (!joined)
                    m_threadUIControl.Abort();
                else
                    ;
            }
            else
                ;
        }

        private void EnabledUIControl()
        {
            lock (m_lockObj)
            {
                for (int i = 0; i < (int)UI_CONTROL_ITEM.COUNT_UI_CONTROL_ITEM; i++)
                    if (!(m_arUIControlItem[i].Enabled == m_arUIControlItemEnabled[i]))
                        {
                            m_arUIControlItem[i].Enabled = m_arUIControlItemEnabled[i];

                            if ((i == (int)UI_CONTROL_ITEM.LISTBOX_ITEM) && (m_arUIControlItemEnabled[i] == false))
                                listBoxItem.Items.Clear();
                            else ;
                        }
                        else ;
            }
        }

        private void FunctionThreadUIControl(object data)
        {
            while (m_bThreadUIControlIsWorking)
            {
                //m_semUIControl.WaitOne();

                EnabledUIControl();

                //while (true) { }
            }

            try { m_semUIControl.Release(1); }
            catch { }
        }

        private void comboBoxMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBoxTEC_SelectedIndexChanged(null, null);
        }

        private void listBoxTEC_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxMode.SelectedIndex > 0)
            {                
                m_list_TECComponents = InitTEC.getListTECComponent(m_connectionSetttings, InitTEC.getPrefixTECComponent(comboBoxMode.SelectedIndex - 1), Convert.ToInt32(m_list_tec.Rows[listBoxTEC.SelectedIndex]["ID"]));

                fillListBox(listBoxItem, m_list_TECComponents);
            }
            else
            {
                m_list_tg = InitTEC.getListTG(m_connectionSetttings, "TEC", Convert.ToInt32(m_list_tec.Rows[listBoxTEC.SelectedIndex]["ID"]));

                fillListBox(listBoxTG, m_list_tg);
            }

            checkBoxTECInUse.Checked = Convert.ToBoolean (m_list_tec.Rows[listBoxTEC.SelectedIndex]["InUse"]);
        }

        private void listBoxItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_list_tg = InitTEC.getListTG(m_connectionSetttings, InitTEC.getPrefixTECComponent(comboBoxMode.SelectedIndex - 1), Convert.ToInt32(m_list_TECComponents.Rows[listBoxItem.SelectedIndex]["ID"]));

            fillListBox(listBoxTG, m_list_tg);
        }

        private static void fillListBox(ListBox listBox, DataTable data)
        {
            listBox.Items.Clear();
            for (int i = 0; i < data.Rows.Count; i++)
            {
                listBox.Items.Add(data.Rows[i]["NAME_SHR"].ToString());
            }
            if (listBox.Items.Count > 0)
                listBox.SelectedIndex = 0;
            else
                ;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            buttonClick(DialogResult.Yes);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            buttonClick(DialogResult.No);
        }

        private void buttonClick(DialogResult res)
        {
            //StopThreadUIControl();
            timerUIControl.Stop();

            this.DialogResult = res;
            Close();
        }

        private void buttonTECDel_Click(object sender, EventArgs e)
        {

        }

        private void buttonTECAdd_Click(object sender, EventArgs e)
        {

        }

        private void buttonItemAdd_Click(object sender, EventArgs e)
        {

        }

        private void buttonItemDel_Click(object sender, EventArgs e)
        {

        }

        private void buttonTGAdd_Click(object sender, EventArgs e)
        {

        }

        private void buttonTGDel_Click(object sender, EventArgs e)
        {

        }

        private void checkBoxTECInUse_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void timerUIControl_Tick(object sender, EventArgs e)
        {
            lock (m_lockObj)
            {
                if (comboBoxMode.SelectedIndex > 0)
                {
                    //Только в режимах ГТП, ЩУ
                    m_arUIControlItemEnabled[(int)UI_CONTROL_ITEM.BUTTON_TG_ADD] = true;
                    m_arUIControlItemEnabled[(int)UI_CONTROL_ITEM.BUTTON_TG_DEL] = true;
                    
                    m_arUIControlItemEnabled[(int)UI_CONTROL_ITEM.LISTBOX_ITEM] = true;
                    m_arUIControlItemEnabled[(int)UI_CONTROL_ITEM.BUTTON_ITEM_ADD] = true;
                    m_arUIControlItemEnabled[(int)UI_CONTROL_ITEM.BUTTON_ITEM_DEL] = true;
                }
                else
                {
                    //Только в режиме ТЭЦ
                    m_arUIControlItemEnabled[(int)UI_CONTROL_ITEM.BUTTON_TG_ADD] = false;
                    m_arUIControlItemEnabled[(int)UI_CONTROL_ITEM.BUTTON_TG_DEL] = false;
                    
                    m_arUIControlItemEnabled[(int)UI_CONTROL_ITEM.LISTBOX_ITEM] = false;
                    m_arUIControlItemEnabled[(int)UI_CONTROL_ITEM.BUTTON_ITEM_ADD] = false;
                    m_arUIControlItemEnabled[(int)UI_CONTROL_ITEM.BUTTON_ITEM_DEL] = false;
                }
            }

            EnabledUIControl();
        }
    }
}
