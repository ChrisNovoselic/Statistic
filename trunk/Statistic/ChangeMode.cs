using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Statistic
{
    public partial class ChangeMode : Form
    {
        public List<TEC> tec;
        public List<int> tec_index;
        public List<int> TECComponent_index;
        public List<int> was_checked;
        public bool admin_was_checked;
        public bool closing;

        private ConnectionSettings m_connSet;

        public enum MODE_TECCOMPONENT : ushort { TEC, GTP, PC, /*, BLOCK*/ UNKNOWN };

        public ChangeMode(ConnectionSettings connSet)
        {
            InitializeComponent();

            InitTEC (connSet);

            //clbMode.Items.Add("Назначение ПБР");

            closing = false;
        }

        public int getModeTECComponent() { return comboBoxModeTECComponent.SelectedIndex + (int)MODE_TECCOMPONENT.GTP; }

        public static string getPrefixMode(int indx)
        {
            String[] arPREFIX_COMPONENT = { "TEC", "GTP", "PC", "TG" };

            return arPREFIX_COMPONENT[indx];
        }

        public static string getNameMode (Int16 indx) {
            string [] nameModes = {"ТЭЦ", "ГТП", "ЩУ", /*"Поблочно",*/ "Неизвестно"};

            return nameModes[indx];
        }
        
        public string getNameAdminValues (Int16 indx) {
            string[] arNameAdminValues = { "Диспетчер", "ДИС" };

            return @"ПБР - " + arNameAdminValues[indx];
        }

        public void InitTEC (ConnectionSettings connSet) {
            m_connSet = connSet;

            int index_tec = 0, index_gtp = 0;

            clbMode.Items.Clear ();

            this.tec = new InitTEC(m_connSet, (short) getModeTECComponent ()).tec;

            tec_index = new List<int>();
            TECComponent_index = new List<int>();
            was_checked = new List<int>();
            admin_was_checked = false;

            foreach (TEC t in this.tec)
            {
                clbMode.Items.Add(t.name);
                tec_index.Add(index_tec);
                TECComponent_index.Add(-1);
                if (t.list_TECComponents.Count > 1)
                {
                    index_gtp = 0;
                    foreach (TECComponent g in t.list_TECComponents)
                    {
                        clbMode.Items.Add(t.name + " - " + g.name);
                        tec_index.Add(index_tec);
                        TECComponent_index.Add(index_gtp);
                        index_gtp++;
                    }
                }
                index_tec++;
            }

            //clbMode.Items.Add("Редактирование ПБР");
            clbMode.Items.Add(getNameAdminValues ((short) comboBoxModeTECComponent.SelectedIndex));
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            int i;
            //if (clbMode.CheckedIndices.Count == 0)
            //{
            //    MessageBox.Show("Вы не выбрали отображаемые объекты, выберите хотя бы один", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            //    return;
            //}

            was_checked.Clear();
            admin_was_checked = false;

            for (i = 0; i < clbMode.CheckedIndices.Count; i++) {
                if (clbMode.CheckedIndices[i] == clbMode.Items.Count - 1)
                    admin_was_checked = true;
                else
                    was_checked.Add(clbMode.CheckedIndices[i]);
            }
            this.DialogResult = DialogResult.OK;
            closing = true;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            btnOk.Focus();
            this.DialogResult = DialogResult.Cancel;
            closing = true;
            Close();
        }

        private void ChangeMode_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!closing)
                e.Cancel = true;
            else
                closing = false;
        }

        private void btnSetAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbMode.Items.Count; i++)
                clbMode.SetItemChecked(i, true);
            btnOk.Focus();
        }

        public void btnClearAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbMode.Items.Count; i++)
                clbMode.SetItemChecked(i, false);
            btnOk.Focus();
        }

        private void ChangeMode_Shown(object sender, EventArgs e)
        {
            clbMode.SetItemChecked(clbMode.Items.Count - 1, admin_was_checked);
        }

        private void comboBoxModeTEC_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.InitTEC (m_connSet);

            closing = false;
        }
    }
}