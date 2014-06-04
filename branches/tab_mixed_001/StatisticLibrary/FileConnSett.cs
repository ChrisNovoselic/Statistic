using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms; //Для MessageBox

namespace StatisticCommon
{
    public class FIleConnSett
    {
        private bool mayToProtected;
        
        public string m_NameFile = string.Empty;
        //private List<ConnectionSettings> m_connectionSettings;

        public FIleConnSett(string nameFile)
        {
            m_NameFile = nameFile;
            //m_connectionSettings = new List<ConnectionSettings> ();
        }

        private int ParseSettingsFile(out List <ConnectionSettings> connSetts, out string msgErr)
        {
            connSetts = new List<ConnectionSettings> ();
            msgErr = string.Empty;

            mayToProtected = true;

            char[] file = new char[1024];
            int count;
            StreamReader sr = new StreamReader(m_NameFile);
            count = sr.ReadBlock(file, 0, 1024);
            sr.Close();

            StringBuilder sb = new StringBuilder(1024);
            int i = 0, j = 0, k = 3,
                countParts = 0;

            sb = Crypt.Crypting().Decrypt(file, count, out msgErr);
            if (msgErr.Length > 0)
                mayToProtected = false;
            else
                ;

            if (mayToProtected == true)
            {
                int pos1 = 0, pos2 = 0, port;
                bool valid;
                string st = sb.ToString(), ignore;

                i = 0;

                pos1 = st.IndexOf('#', 0);
                if (pos1 > 0)
                {
                    countParts = System.Int32.Parse(st.Substring(0, pos1));
                    pos1++;

                    //foreach (ConnectionSettings cs in m_connectionSettings)
                    while (i < countParts)
                    {
                        connSetts.Add(new ConnectionSettings ());
                        connSetts[connSetts.Count - 1].port = 1433;

                        pos2 = st.IndexOf(';', pos1);
                        if (pos2 < 0)
                        {
                            //MessageBox.Show(this, "Файл с насртойками имеет неправильный формат!\nОбратитесь к поставщику программы.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            mayToProtected = false;
                            break;
                        }
                        else
                            ;

                        connSetts[i].server =
                        st.Substring(pos1, pos2 - pos1);
                        pos1 = pos2 + 1;

                        pos2 = st.IndexOf(';', pos1);
                        if (pos2 < 0)
                        {
                            //MessageBox.Show(this, "Файл с насртойками имеет неправильный формат!\nОбратитесь к поставщику программы.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            mayToProtected = false;
                            break;
                        }
                        else
                            ;
                        valid = int.TryParse(st.Substring(pos1, pos2 - pos1), out port);
                        if (!valid)
                        {
                            //MessageBox.Show(this, "В файле настроек неправильно задан порт!\nОбратитесь к поставщику программы.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            mayToProtected = false;
                            break;
                        }
                        else
                            ;
                        connSetts[i].port =
                        port;
                        pos1 = pos2 + 1;

                        pos2 = st.IndexOf(';', pos1);
                        if (pos2 < 0)
                        {
                            //MessageBox.Show(this, "Файл с насртойками имеет неправильный формат!\nОбратитесь к поставщику программы.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            mayToProtected = false;
                            break;
                        }
                        else
                            ;
                        connSetts[i].dbName =
                        st.Substring(pos1, pos2 - pos1);
                        pos1 = pos2 + 1;

                        pos2 = st.IndexOf(';', pos1);
                        if (pos2 < 0)
                        {
                            //MessageBox.Show(this, "Файл с насртойками имеет неправильный формат!\nОбратитесь к поставщику программы.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            mayToProtected = false;
                            break;
                        }
                        else
                            ;
                        connSetts[i].userName =
                        st.Substring(pos1, pos2 - pos1);
                        pos1 = pos2 + 1;

                        pos2 = st.IndexOf(';', pos1);
                        if (pos2 < 0)
                        {
                            msgErr = "Файл с насртойками имеет неправильный формат!\nОбратитесь к поставщику программы.";
                            mayToProtected = false;
                            break;
                        }
                        else
                            ;
                        connSetts[i].password =
                        st.Substring(pos1, pos2 - pos1);
                        pos1 = pos2 + 1;

                        pos2 = st.IndexOf(';', pos1);
                        if (pos2 < 0)
                        {
                            msgErr = "Файл с насртойками имеет неправильный формат!\nОбратитесь к поставщику программы.";
                            mayToProtected = false;
                            break;
                        }
                        else
                            ;
                        ignore = st.Substring(pos1, pos2 - pos1);
                        if (ignore != "1" && ignore != "0")
                        {
                            msgErr = "В файле настроек неправильно задано игнорирование!\nОбратитесь к поставщику программы.";
                            mayToProtected = false;
                            break;
                        }
                        else
                            ;
                        connSetts[i].ignore =
                        (ignore == "1");

                        //m_connectionSettingsEdit[i].ignore = cs.ignore = (ignore == "0");

                        pos1 = pos2 + 1;

                        i++;
                    }

                    if (mayToProtected == false)
                        for (int c = 0; c < connSetts.Count; c++)
                            connSetts[c].SetDefault();
                    else
                        ;
                }
                else
                    mayToProtected = false; //Не найдено количество блоков соединений
            }
            else
                mayToProtected = false;

            if (mayToProtected == true)
                return 0; //Успех
            else
                return 1; //Ошибка
        }

        public void ReadSettingsFile (out List <ConnectionSettings> listConnSett, out int res, out string mes)
        {
            if (File.Exists(m_NameFile) == false)
            {//Не найден файл
                listConnSett = new List<ConnectionSettings>();
                res = 1;
                mes = "Не найден файл";
            }
            else
                res = ParseSettingsFile(out listConnSett, out mes);
        }

        public void SaveSettingsFile(List <ConnectionSettings> listConnSett, out int err)
        {
            err = 1;

            StreamWriter sw = new StreamWriter(m_NameFile, false);

            StringBuilder sb = new StringBuilder(1024);

            sb.Append(listConnSett.Count.ToString() + '#');

            foreach (ConnectionSettings cs in listConnSett)
            {
                sb.Append(cs.server + ";");
                sb.Append(cs.port.ToString() + ";");
                sb.Append(cs.dbName + ";");
                sb.Append(cs.userName + ";");
                sb.Append(cs.password + ";");
                sb.Append(cs.ignore ? "1;" : "0;");
            }

            char[] file = Crypt.Crypting().Encrypt(sb, out err);

            if (err > 0)
            {
                sw.Write(file, 0, err);

                err = 0; //Успешно
                mayToProtected = true;
            }
            else
            {
                err = 1; //Ошибка
                mayToProtected = false;
            }

            sw.Close();
        }
    }
}