using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;

//using Statistic;

namespace StatisticCommon
{
    public partial class FormConnectionSettings : Form
    {
        private List<ConnectionSettings> m_connectionSettingsEdit, m_connectionSettings;
        //public List<TEC> tec;

        private bool closing;
        private int oldSelectedIndex;
        private uint MAGIC = 0xA5A55A5A;
        private int magicPeriod = 10;
        private uint[] key = {
                                0x3a39ce37,   0xd3faf5cf,   0xabc27737,   0x5ac52d1b,   0x5cb0679e,   0x4fa33742,
                                0xd3822740,   0x99bc9bbe,   0xd5118e9d,   0xbf0f7315,   0xd62d1c7e,   0xc700c47b,
                                0xb78c1b6b,   0x21a19045,   0xb26eb1be,   0x6a366eb4,   0x5748ab2f,   0xbc946e79,
                                0xc6a376d2,   0x6549c2c8,   0x530ff5ee,   0x468dde7d,   0xd5730a1d,   0x4cd04dc6,
                                0x2939bbdb,   0xa9ba4650,   0xac9526e8,   0xbe5ee304,   0xa1fad5f0,   0x6a2d519a,
                                0x63ef8ce2,   0x9a86ee22,   0xc089c2b8,   0x43242ef6,   0xa51e03aa,   0x9cf2d0a4,
                                0x83c061ba,   0x9be96a4d,   0x8fe51550,   0xba645bd6,   0x2826a2f9,   0xa73a3ae1,
                                0x4ba99586,   0xef5562e9,   0xc72fefd3,   0xf752f7da,   0x3f046f69,   0x77fa0a59,
                                0x80e4a915,   0x87b08601,   0x9b09e6ad,   0x3b3ee593,   0xe990fd5a,   0x9e34d797,
                                0x2cf0b7d9,   0x022b8b51,   0x96d5ac3a,   0x017da67d,   0xd1cf3ed6,   0x7c7d2d28,
                                0x1f9f25cf,   0xadf2b89b,   0x5ad6b472,   0x5a88f54c,   0xe029ac71,   0xe019a5e6,
                                0x47b0acfd,   0xed93fa9b,   0xe8d3c48d,   0x283b57cc,   0xf8d56629,   0x79132e28,
                                0x785f0191,   0xed756055,   0xf7960e44,   0xe3d35e8c,   0x15056dd4,   0x88f46dba,
                                0x03a16125,   0x0564f0bd,   0xc3eb9e15,   0x3c9057a2,   0x97271aec,   0xa93a072a,
                                0x1b3f6d9b,   0x1e6321f5,   0xf59c66fb,   0x26dcf319,   0x7533d928,   0xb155fdf5,
                                0x03563482,   0x8aba3cbb,   0x28517711,   0xc20ad9f8,   0xabcc5167,   0xccad925f,
                                0x4de81751,   0x3830dc8e,   0x379d5862,   0x9320f991,   0xea7a90c2,   0xfb3e7bce,
                                0x5121ce64,   0x774fbe32,   0xa8b6e37e,   0xc3293d46,   0x48de5369,   0x6413e680,
                                0xa2ae0810,   0xdd6db224,   0x69852dfd,   0x09072166,   0xb39a460a,   0x6445c0dd,
                                0x586cdecf,   0x1c20c8ae,   0x5bbef7dd,   0x1b588d40,   0xccd2017f,   0x6bb4e3bb,
                                0xdda26a7e,   0x3a59ff45,   0x3e350a44,   0xbcb4cdd5,   0x72eacea8,   0xfa6484bb,
                                0x8d6612ae,   0xbf3c6f47,   0xd29be463,   0x542f5d9e,   0xaec2771b,   0xf64e6370,
                                0x740e0d8d,   0xe75b1357,   0xf8721671,   0xaf537d5d,   0x4040cb08,   0x4eb4e2cc,
                                0x34d2466a,   0x0115af84,   0xe1b00428,   0x95983a1d,   0x06b89fb4,   0xce6ea048,
                                0x6f3f3b82,   0x3520ab82,   0x011a1d4b,   0x277227f8,   0x611560b1,   0xe7933fdc,
                                0xbb3a792b,   0x344525bd,   0xa08839e1,   0x51ce794b,   0x2f32c9b7,   0xa01fbac9,
                                0xe01cc87e,   0xbcc7d1f6,   0xcf0111c3,   0xa1e8aac7,   0x1a908749,   0xd44fbd9a,
                                0xd0dadecb,   0xd50ada38,   0x0339c32a,   0xc6913667,   0x8df9317c,   0xe0b12b4f,
                                0x0f91fc71,   0x9b941525,   0xfae59361,   0xceb69ceb,   0xc2a86459,   0x12baa8d1,
                                0xb6c1075e,   0xe3056a0c,   0x10d25065,   0xcb03a442,   0xe0ec6e0e,   0x1698db3b,
                                0x4c98a0be,   0x3278e964,   0x9f1f9532,   0xe0d392df,   0xd3a0342b,   0x8971f21e,
                                0x1b0a7441,   0x4ba3348c,   0xc5be7120,   0xc37632d8,   0xdf359f8d,   0x9b992f2e,
                                0xe60b6f47,   0x0fe3f11d,   0xe54cda54,   0x1edad891,   0xce6279cf,   0xcd3e7e6f,
                                0x1618b166,   0xfd2c1d05,   0x848fd2c5,   0xf6fb2299,   0xf523f357,   0xa6327623,
                                0x93a83531,   0x56cccd02,   0xacf08162,   0x5a75ebb5,   0x6e163697,   0x88d273cc,
                                0xde966292,   0x81b949d0,   0x4c50901b,   0x71c65614,   0xe6c6c7bd,   0x327a140a,
                                0x45e1d006,   0xc3f27b9a,   0xc9aa53fd,   0x62a80f00,   0xbb25bfe2,   0x35bdd2f6,
                                0x71126905,   0xb2040222,   0xb6cbcf7c,   0xcd769c2b,   0x53113ec0,   0x1640e3d3,
                                0x38abbd60,   0x2547adf0,   0xba38209c,   0xf746ce76,   0x77afa1c5,   0x20756060,
                                0x85cbfe4e,   0x8ae88dd8,   0x7aaaf9b0,   0x4cf9aa7e,   0x1948c25c,   0x02fb8a8c,
                                0x01c36ae4,   0xd6ebe1f9,   0x90d4f869,   0xa65cdea0,   0x3f09252d,   0xc208e69f
                            };

        private string settingsFile = "settings.ini";
        private bool mayToProtected;

        private bool ParseSettingsFile()
        {
            bool bRes = true;

            mayToProtected = false;

            char[] file = new char[1024];
            int count;
            StreamReader sr = new StreamReader(settingsFile);
            count = sr.ReadBlock(file, 0, 1024);
            sr.Close();

            StringBuilder sb = new StringBuilder(1024);
            int i = 0, j = 0, k = 3,
                countParts = 0;

            uint magic;
            while (i < count)
            {
                sb.Append((char)(file[i] ^ (char)((key[j] >> (8 * k)) & 0xFF)));
                i++;
                k--;
                if (k < 0)
                {
                    k = 0;
                    j++;
                    if (j >= key.Length)
                        j = 0;
                }

                if (i % magicPeriod == 0)
                {
                    magic = 0;
                    magic = (char)(file[i] ^ (char)((key[j] >> (8 * k)) & 0xFF));
                    k--;
                    if (k < 0)
                    {
                        k = 0;
                        j++;
                        if (j >= key.Length)
                            j = 0;
                    }
                    magic = magic << 8;
                    magic += (char)(file[i + 1] ^ (char)((key[j] >> (8 * k)) & 0xFF));
                    k--;
                    if (k < 0)
                    {
                        k = 0;
                        j++;
                        if (j >= key.Length)
                            j = 0;
                    }
                    magic = magic << 8;
                    magic += (char)(file[i + 2] ^ (char)((key[j] >> (8 * k)) & 0xFF));
                    k--;
                    if (k < 0)
                    {
                        k = 0;
                        j++;
                        if (j >= key.Length)
                            j = 0;
                    }
                    magic = magic << 8;
                    magic += (char)(file[i + 3] ^ (char)((key[j] >> (8 * k)) & 0xFF));
                    k--;
                    if (k < 0)
                    {
                        k = 0;
                        j++;
                        if (j >= key.Length)
                            j = 0;
                    }
                    i += 4;

                    if (magic != MAGIC)
                    {
                        MessageBox.Show(this, "Файл с насртойками имеет неправильный формат!\nОбратитесь к поставщику программы.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        bRes = false;
                    }
                }
            }

            if (bRes == true)
            {
                int pos1 = 0, pos2 = 0, port;
                bool valid;
                string st = sb.ToString(), ignore;

                i = 0;

                pos1 = st.IndexOf('#', 0);
                if (pos1 > 0) {
                    countParts = System.Int32.Parse (st.Substring(0, pos1));
                    pos1 ++;

                    //foreach (ConnectionSettings cs in m_connectionSettings)
                    while (i < countParts)
                    {
                        addConnSett (new ConnectionSettings ());
                    
                        pos2 = st.IndexOf(';', pos1);
                        if (pos2 < 0)
                        {
                            //MessageBox.Show(this, "Файл с насртойками имеет неправильный формат!\nОбратитесь к поставщику программы.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ClearSettings();
                            bRes = false;
                            break;
                        }

                        m_connectionSettingsEdit[i].server =
                        m_connectionSettings[i].server =
                        st.Substring(pos1, pos2 - pos1);
                        pos1 = pos2 + 1;

                        pos2 = st.IndexOf(';', pos1);
                        if (pos2 < 0)
                        {
                            //MessageBox.Show(this, "Файл с насртойками имеет неправильный формат!\nОбратитесь к поставщику программы.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ClearSettings();
                            bRes = false;
                            break;
                        }
                        valid = int.TryParse(st.Substring(pos1, pos2 - pos1), out port);
                        if (!valid)
                        {
                            //MessageBox.Show(this, "В файле настроек неправильно задан порт!\nОбратитесь к поставщику программы.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ClearSettings();
                            bRes = false;
                            break;
                        }
                        m_connectionSettingsEdit[i].port =
                        m_connectionSettings[i].port =
                        port;
                        pos1 = pos2 + 1;

                        pos2 = st.IndexOf(';', pos1);
                        if (pos2 < 0)
                        {
                            //MessageBox.Show(this, "Файл с насртойками имеет неправильный формат!\nОбратитесь к поставщику программы.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ClearSettings();
                            bRes = false;
                            break;
                        }
                        m_connectionSettingsEdit[i].dbName =
                        m_connectionSettings[i].dbName =
                        st.Substring(pos1, pos2 - pos1);
                        pos1 = pos2 + 1;

                        pos2 = st.IndexOf(';', pos1);
                        if (pos2 < 0)
                        {
                            //MessageBox.Show(this, "Файл с насртойками имеет неправильный формат!\nОбратитесь к поставщику программы.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ClearSettings();
                            bRes = false;
                            break;
                        }
                        m_connectionSettingsEdit[i].userName =
                        m_connectionSettings[i].userName =
                        st.Substring(pos1, pos2 - pos1);
                        pos1 = pos2 + 1;

                        pos2 = st.IndexOf(';', pos1);
                        if (pos2 < 0)
                        {
                            MessageBox.Show(this, "Файл с насртойками имеет неправильный формат!\nОбратитесь к поставщику программы.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ClearSettings();
                            bRes = false;
                            break;
                        }
                        m_connectionSettingsEdit[i].password =
                        m_connectionSettings[i].password =
                        st.Substring(pos1, pos2 - pos1);
                        pos1 = pos2 + 1;

                        pos2 = st.IndexOf(';', pos1);
                        if (pos2 < 0)
                        {
                            MessageBox.Show(this, "Файл с насртойками имеет неправильный формат!\nОбратитесь к поставщику программы.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ClearSettings();
                            bRes = false;
                            break;
                        }
                        ignore = st.Substring(pos1, pos2 - pos1);
                        if (ignore != "1" && ignore != "0")
                        {
                            MessageBox.Show(this, "В файле настроек неправильно задано игнорирование!\nОбратитесь к поставщику программы.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ClearSettings();
                            bRes = false;
                            break;
                        }
                        m_connectionSettingsEdit[i].ignore =
                        m_connectionSettings[i].ignore =
                        (ignore == "1");

                        //m_connectionSettingsEdit[i].ignore = cs.ignore = (ignore == "0");

                        pos1 = pos2 + 1;

                        i++;
                    }
                }
                else
                    bRes = false; //Не найдено количество блоков соединений
            }
            else
                bRes = false;

            if (bRes == true)
                mayToProtected = true;
            else
                ;

            return bRes;
        }

        private void ClearSettings()
        {
            for (int i = 0; i < m_connectionSettingsEdit.Count; i++)
                m_connectionSettingsEdit[i].SetDefault();
        }

        public void SaveSettingsFile()
        {
            StreamWriter sw = new StreamWriter(settingsFile, false);

            StringBuilder sb = new StringBuilder(1024);

            sb.Append(m_connectionSettings.Count.ToString () + '#');

            foreach (ConnectionSettings cs in m_connectionSettings)
            {
                sb.Append(cs.server + ";");
                sb.Append(cs.port.ToString() + ";");
                sb.Append(cs.dbName + ";");
                sb.Append(cs.userName + ";");
                sb.Append(cs.password + ";");
                sb.Append(cs.ignore ? "1;" : "0;");
            }

            char[] file = new char[1024];

            int i = 0, j = 0, k = 3, t = 0;
            while (t < sb.Length)
            {
                file[i] = (char)(sb[t] ^ (char)((key[j] >> (8 * k)) & 0xFF));
                i++;
                t++;
                k--;
                if (k < 0)
                {
                    k = 0;
                    j++;
                    if (j >= key.Length)
                        j = 0;
                }

                if (i % magicPeriod == 0)
                {
                    file[i] = (char)((char)((MAGIC >> 24) & 0xFF) ^ (char)((key[j] >> (8 * k)) & 0xFF));
                    k--;
                    if (k < 0)
                    {
                        k = 0;
                        j++;
                        if (j >= key.Length)
                            j = 0;
                    }
                    file[i + 1] = (char)((char)((MAGIC >> 16) & 0xFF) ^ (char)((key[j] >> (8 * k)) & 0xFF));
                    k--;
                    if (k < 0)
                    {
                        k = 0;
                        j++;
                        if (j >= key.Length)
                            j = 0;
                    }
                    file[i + 2] = (char)((char)((MAGIC >> 8) & 0xFF) ^ (char)((key[j] >> (8 * k)) & 0xFF));
                    k--;
                    if (k < 0)
                    {
                        k = 0;
                        j++;
                        if (j >= key.Length)
                            j = 0;
                    }
                    file[i + 3] = (char)((char)(MAGIC & 0xFF) ^ (char)((key[j] >> (8 * k)) & 0xFF));
                    k--;
                    if (k < 0)
                    {
                        k = 0;
                        j++;
                        if (j >= key.Length)
                            j = 0;
                    }
                    i += 4;
                }
            }

            sw.Write(file, 0, i);
            sw.Close();

            mayToProtected = true;
        }

        //public FormConnectionSettings(List<TEC> tec)
        public FormConnectionSettings()
        {
            InitializeComponent();

            bool bConnSettEmpty = false;

            //this.tec = tec;
            m_connectionSettingsEdit = new List<ConnectionSettings>();
            m_connectionSettings = new List<ConnectionSettings>();

            cbxConnFor.DropDownStyle = ComboBoxStyle.DropDownList;

            //foreach (TEC t in tec)
            //{
            //    cbxConnFor.Items.Add(t.name);
            //    m_connectionSettingsEdit.Add(new ConnectionSettings());
            //    m_connectionSettings.Add(new ConnectionSettings());
            //    t.connSett = m_connectionSettings[m_connectionSettings.Count - 1];
            //}

            cbxConnFor.Items.Add("БД конфигурации");

            closing = false;
            if (!File.Exists(settingsFile)) {
                //Не найден файл
                bConnSettEmpty = true;
            }
            else
                if (ParseSettingsFile())
                {
                    tbxServer.Text = m_connectionSettingsEdit[0].server;
                    nudnPort.Value = m_connectionSettingsEdit[0].port;
                    tbxDataBase.Text = m_connectionSettingsEdit[0].dbName;
                    tbxUserId.Text = m_connectionSettingsEdit[0].userName;
                    mtbxPass.Text = m_connectionSettingsEdit[0].password;
                    cbxIgnore.Checked = m_connectionSettingsEdit[0].ignore;
                }
                else
                    //Ошибка при разборе файла
                    bConnSettEmpty = true;

            if (bConnSettEmpty == true)
            {
                m_connectionSettingsEdit.Add(new ConnectionSettings());
                m_connectionSettingsEdit[m_connectionSettingsEdit.Count - 1].port = 3306;

                m_connectionSettings.Add(new ConnectionSettings());
                m_connectionSettings[m_connectionSettings.Count - 1].port = 3306;
            }
            else
                ;

            cbxConnFor.SelectedIndex = oldSelectedIndex = 0;
        }

        public ConnectionSettings getConnSett(int indx = -1) {
            if (indx < 0)
                indx = cbxConnFor.SelectedIndex;
            else
                ;

            //return m_connectionSettings[m_connectionSettings.Count - 1];
            return m_connectionSettings[indx];
        }

        public void setConnSett(int indx, ConnectionSettings connSett)
        {
            addConnSett (connSett);
        }

        public void addConnSett(ConnectionSettings connSett)
        {
            m_connectionSettings.Add(connSett);
            m_connectionSettingsEdit.Add(connSett);

            cbxConnFor.Items.Add("БД конфигурации" + " - Дополн.(" + (cbxConnFor.Items.Count - 1) + ")");
        }

        public void btnOk_Click(object obj, EventArgs ev)
        {
            ConnectionSettings.ConnectionSettingsError error;

            m_connectionSettingsEdit[cbxConnFor.SelectedIndex].server = tbxServer.Text;
            m_connectionSettingsEdit[cbxConnFor.SelectedIndex].port = (int)nudnPort.Value;
            m_connectionSettingsEdit[cbxConnFor.SelectedIndex].dbName = tbxDataBase.Text;
            m_connectionSettingsEdit[cbxConnFor.SelectedIndex].userName = tbxUserId.Text;
            m_connectionSettingsEdit[cbxConnFor.SelectedIndex].password = mtbxPass.Text;
            m_connectionSettingsEdit[cbxConnFor.SelectedIndex].ignore = cbxIgnore.Checked;

            for (int i = 0; i < m_connectionSettingsEdit.Count; i++ )
            {
                if ((error = m_connectionSettingsEdit[i].Validate()) != ConnectionSettings.ConnectionSettingsError.NoError)
                {
                    string msgError = string.Empty;
                    switch (error)
                    {
                        case ConnectionSettings.ConnectionSettingsError.WrongIp:
                            msgError = "Недопустимый IP-адрес";
                            break;
                        case ConnectionSettings.ConnectionSettingsError.WrongPort:
                           msgError = "Порт должен лежать в пределах [0:65535].";
                            break;
                        case ConnectionSettings.ConnectionSettingsError.WrongDbName:
                            msgError = "Не задано имя базы данных.";
                            break;
                        case ConnectionSettings.ConnectionSettingsError.IllegalSymbolDbName:
                            msgError = "Недопустимый символ в имени базы.";
                            break;
                        case ConnectionSettings.ConnectionSettingsError.IllegalSymbolUserName:
                            msgError = "Недопустимый символ в имени пользователя.";
                            break;
                        case ConnectionSettings.ConnectionSettingsError.IllegalSymbolPassword:
                            msgError = "Недопустимый символ в пароле пользователя.";
                            break;
                        case ConnectionSettings.ConnectionSettingsError.NotConnect:
                            msgError = "Недопустимый символ в пароле пользователя.";
                            break;
                    }
                    msgError += "\nПроверьте правильность настроек для соединения \"" + cbxConnFor.Items[i].ToString() + "\".";
                    MessageBox.Show(this, msgError, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    closing = false;
                    return;
                }

                m_connectionSettings[i].server = m_connectionSettingsEdit[i].server;
                m_connectionSettings[i].port = m_connectionSettingsEdit[i].port;
                m_connectionSettings[i].dbName = m_connectionSettingsEdit[i].dbName;
                m_connectionSettings[i].userName = m_connectionSettingsEdit[i].userName;
                m_connectionSettings[i].password = m_connectionSettingsEdit[i].password;
                m_connectionSettings[i].ignore = m_connectionSettingsEdit[i].ignore;
            }

            SaveSettingsFile();

            closing = true;
            this.DialogResult = DialogResult.Yes;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < m_connectionSettingsEdit.Count; i++)
            {
                m_connectionSettingsEdit[i].server = m_connectionSettings[i].server;
                m_connectionSettingsEdit[i].port = m_connectionSettings[i].port;
                m_connectionSettingsEdit[i].dbName = m_connectionSettings[i].dbName;
                m_connectionSettingsEdit[i].userName = m_connectionSettings[i].userName;
                m_connectionSettingsEdit[i].password = m_connectionSettings[i].password;
                m_connectionSettingsEdit[i].ignore = m_connectionSettings[i].ignore;
            }
            tbxServer.Text = m_connectionSettingsEdit[cbxConnFor.SelectedIndex].server;
            nudnPort.Value = m_connectionSettingsEdit[cbxConnFor.SelectedIndex].port;
            tbxDataBase.Text = m_connectionSettingsEdit[cbxConnFor.SelectedIndex].dbName;
            tbxUserId.Text = m_connectionSettingsEdit[cbxConnFor.SelectedIndex].userName;
            mtbxPass.Text = m_connectionSettingsEdit[cbxConnFor.SelectedIndex].password;
            cbxIgnore.Checked = m_connectionSettingsEdit[cbxConnFor.SelectedIndex].ignore;

            
            closing = true;
            this.DialogResult = DialogResult.No;
            Close();
        }

        public ConnectionSettings ConnectionSettingsEdit {
            get {
                return m_connectionSettingsEdit[cbxConnFor.SelectedIndex];
            }

            set {
                tbxServer.Text = value.server;
                nudnPort.Value = value.port;
                tbxDataBase.Text = value.dbName;
                tbxUserId.Text = value.userName;
                mtbxPass.Text = value.password;
                cbxIgnore.Checked = false; //value.ignore;
            }
        }

        public int SelectedIndex
        {
            get
            {
                return cbxConnFor.SelectedIndex;
            }

            set
            {
                cbxConnFor.SelectedIndex = value;
            }
        }

        private void cbxConnFor_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_connectionSettingsEdit[oldSelectedIndex].server = tbxServer.Text;
            m_connectionSettingsEdit[oldSelectedIndex].port = (int)nudnPort.Value;
            m_connectionSettingsEdit[oldSelectedIndex].dbName = tbxDataBase.Text;
            m_connectionSettingsEdit[oldSelectedIndex].userName = tbxUserId.Text;
            m_connectionSettingsEdit[oldSelectedIndex].password = mtbxPass.Text;
            m_connectionSettingsEdit[oldSelectedIndex].ignore = cbxIgnore.Checked;

            tbxServer.Text = m_connectionSettingsEdit[cbxConnFor.SelectedIndex].server;
            nudnPort.Value = m_connectionSettingsEdit[cbxConnFor.SelectedIndex].port;
            tbxDataBase.Text = m_connectionSettingsEdit[cbxConnFor.SelectedIndex].dbName;
            tbxUserId.Text = m_connectionSettingsEdit[cbxConnFor.SelectedIndex].userName;
            mtbxPass.Text = m_connectionSettingsEdit[cbxConnFor.SelectedIndex].password;
            cbxIgnore.Checked = m_connectionSettingsEdit[cbxConnFor.SelectedIndex].ignore;

            oldSelectedIndex = cbxConnFor.SelectedIndex;

            //if (cbxConnFor.SelectedIndex == cbxConnFor.Items.Count - 1)
                cbxIgnore.Enabled = false;
            //else
            //    cbxIgnore.Enabled = true;
        }

        private void ConnectionSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!closing)
                e.Cancel = true;
            else
                closing = false;
        }

        public bool Protected
        {
            get { return mayToProtected; }
        }

        public int Count
        {
            get { return m_connectionSettings.Count; }
        }

        private void component_Changed(object sender, EventArgs e)
        {

        }
    }
}