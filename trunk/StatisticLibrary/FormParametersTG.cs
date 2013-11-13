using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace StatisticCommon {
    public partial class FormParametersTG : FormParametersBase
    {
        private const int COUNT_TG = 8;
        private const char SEP_ID_TG = ',';
        private string [] NAME_TIME = {"min", "hour"};
        private const string NAME_SECTION_TG_ID = "Идентификаторы ТГ Бийск"; //"Main settings"

        private System.Windows.Forms.TextBox[,] m_array_tbxTG;
        private System.Windows.Forms.Label[,] m_array_lblTG;

        private int[,] m_tg_id_default = { { 9223, 9222, 9431, 9430, 9433,9435, 9434, 9432 }, { 8436, 8470, 8878, 8674, 8980, 9150, 6974, 8266 } };
        private int [,] m_tg_id;

        //public FormParametersTG(DelegateFunc delParApp)
        public FormParametersTG(string nameFileINI) : base(nameFileINI)
        {
            InitializeComponent();

            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);

            //delegateParamsApply = delParApp;

            if ((m_tg_id_default.Length / m_tg_id_default.Rank == COUNT_TG) && (m_tg_id_default.Rank == (int)TG.ID_TIME.COUNT_ID_TIME)) ;
            else ;

            m_tg_id = new int[(int)TG.ID_TIME.COUNT_ID_TIME, COUNT_TG];
            for (int i = (int)TG.ID_TIME.MINUTES; i < (int)TG.ID_TIME.COUNT_ID_TIME; i++) {
                for (int j = 0; j < COUNT_TG; j ++) {
                    m_tg_id[i, j] = m_tg_id_default[i, j];
                }
            }
            m_array_tbxTG = new System.Windows.Forms.TextBox [(int) TG.ID_TIME.COUNT_ID_TIME, COUNT_TG];
            m_array_lblTG = new System.Windows.Forms.Label[(int)TG.ID_TIME.COUNT_ID_TIME, COUNT_TG];

            //m_array_lblTG[(int)TG.ID_TIME.MINUTES, 0] = new Label();
            //m_array_tbxTG[(int)TG.ID_TIME.MINUTES, 0] = tbxTG1Mins;

            for (int i = (int)TG.ID_TIME.MINUTES; i < (int)TG.ID_TIME.COUNT_ID_TIME; i++) {
                for (int j = 0; j < COUNT_TG; j ++) {
                    m_array_lblTG[i, j] = new System.Windows.Forms.Label();
                    m_array_lblTG[i, j].AutoSize = true;
                    m_array_lblTG[i, j].Location = new System.Drawing.Point(12 + i * 164, 9 + j * 27);
                    //m_array_lblTG[i, j].Name = "lblTG1Mins";
                    //m_array_lblTG[i, 0].Size = new System.Drawing.Size(79, 13);
                    m_array_lblTG[i, j].TabIndex = (i * COUNT_TG) + j + 6;
                    m_array_lblTG[i, j].Text = "ТГ" + (j + 1).ToString () + " ";
                    if (i % 2 == 0)
                        m_array_lblTG[i, j].Text += "минутный";
                    else
                        m_array_lblTG[i, j].Text += "получасовой";

                    //m_array_lblTG[i, j].Anchor = AnchorStyles.Top;

                    this.Controls.Add(m_array_lblTG[i, j]);

                    m_array_tbxTG[i, j] = new System.Windows.Forms.TextBox();
                    m_array_tbxTG[i, j].Location = new System.Drawing.Point(97 + i * 178, 6 + j * 27);
                    //m_array_tbxTG[i, j].Name = "tbxTG1Hours";
                    m_array_tbxTG[i, j].Size = new System.Drawing.Size(70, 20);
                    m_array_tbxTG[i, j].TabIndex = (i * COUNT_TG) + j + 7;
                    this.Controls.Add(m_array_tbxTG[i, j]);
                }
            }

            btnCancel.Location = new System.Drawing.Point(17, m_array_tbxTG[(int)TG.ID_TIME.COUNT_ID_TIME - 1, COUNT_TG - 1].Location.Y + m_array_tbxTG[(int)TG.ID_TIME.COUNT_ID_TIME - 1, COUNT_TG - 1].Size.Height + 9 * 2);
            btnOk.Location = new System.Drawing.Point(98, m_array_tbxTG[(int)TG.ID_TIME.COUNT_ID_TIME - 1, COUNT_TG - 1].Location.Y + m_array_tbxTG[(int)TG.ID_TIME.COUNT_ID_TIME - 1, COUNT_TG - 1].Size.Height + 9 * 2);
            btnReset.Location = new System.Drawing.Point(148, m_array_tbxTG[(int)TG.ID_TIME.COUNT_ID_TIME - 1, COUNT_TG - 1].Location.Y + m_array_tbxTG[(int)TG.ID_TIME.COUNT_ID_TIME - 1, COUNT_TG - 1].Size.Height + 9 * 2);

            this.ClientSize = new System.Drawing.Size(this.ClientSize.Width, btnReset.Location.Y + btnReset.Size.Height + 9);

            loadParam();
            mayClose = false;
        }

        public void loadParam()
        {
            string key_value;
            string [] key_values;
            int tg_id;

            for (int i = 0; i < (int) TG.ID_TIME.COUNT_ID_TIME; i++) {
                for (int j = 0; j < COUNT_TG; j ++) {
                    key_value = m_FileINI.ReadString(NAME_SECTION_TG_ID, "TG" + (j + 1).ToString() + " " + NAME_TIME[i], null);
                    if (key_value.Length > 0) {
                        key_values = key_value.Split(SEP_ID_TG);
                        if (int.TryParse(key_values[(int)TYPE_VALUE.CURRENT], out tg_id)) m_tg_id[i, j] = tg_id; else ;
                        if (key_values.GetLength(0) > 1)
                            if (int.TryParse(key_values[(int)TYPE_VALUE.PREVIOUS], out tg_id)) m_tg_id_default[i, j] = tg_id; else ;
                        else {
                            m_tg_id_default[i, j] = tg_id;
                            WriteTGIds(i, j);
                        }
                    }
                    else {
                        WriteTGIds (i, j);
                    }

                    m_array_tbxTG[i, j].Text = m_tg_id[i, j].ToString ();
                }
            }
        }

        public void WriteTGIds (int id_time, int num_tg) {
            string key_value;

            key_value = m_tg_id[id_time, num_tg].ToString();
            key_value += SEP_ID_TG;
            key_value += m_tg_id_default[id_time, num_tg].ToString();
            m_FileINI.WriteString(NAME_SECTION_TG_ID, "TG" + (num_tg + 1).ToString() + " " + NAME_TIME[id_time], key_value);
        }

        public void saveParam()
        {
            for (int i = 0; i < (int) TG.ID_TIME.COUNT_ID_TIME; i++)
            {
                for (int j = 0; j < COUNT_TG; j++)
                {
                    WriteTGIds (i, j);
                }
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            int tg_id;
            for (int i = 0; i < COUNT_TG; i++)
            {
                if (int.TryParse(m_array_tbxTG[(int)TG.ID_TIME.MINUTES, i].Text, out tg_id)) m_tg_id[(int)TG.ID_TIME.MINUTES, i] = tg_id; else m_array_tbxTG[(int)TG.ID_TIME.MINUTES, i].Text = m_tg_id[(int)TG.ID_TIME.MINUTES, i].ToString();
                if (int.TryParse(m_array_tbxTG[(int)TG.ID_TIME.HOURS, i].Text, out tg_id)) m_tg_id[(int)TG.ID_TIME.HOURS, i] = tg_id; else m_array_tbxTG[(int)TG.ID_TIME.HOURS, i].Text = m_tg_id[(int)TG.ID_TIME.HOURS, i].ToString();
            }

            saveParam();
            
            //delegateParamsApply();
            
            mayClose = true;

            if (m_State > 0)
                m_State--;
            else
                ;
            
            Close();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < COUNT_TG; i++)
            {
                m_tg_id[(int)TG.ID_TIME.MINUTES, i] = m_tg_id_default[(int)TG.ID_TIME.MINUTES, i];
                m_tg_id[(int)TG.ID_TIME.HOURS, i] = m_tg_id_default[(int)TG.ID_TIME.HOURS, i];
            }

            for (int i = 0; i < COUNT_TG; i++)
            {
                m_array_tbxTG[(int)TG.ID_TIME.MINUTES, i].Text = m_tg_id_default[(int)TG.ID_TIME.MINUTES, i].ToString();
                m_array_tbxTG[(int)TG.ID_TIME.HOURS, i].Text = m_tg_id_default[(int)TG.ID_TIME.HOURS, i].ToString();
            }

            m_State ++;
        }

        override public void buttonCancel_Click(object sender, EventArgs e)
        {
            loadParam ();
            
            m_State = 0;

            base.buttonCancel_Click(sender, e);
        }

        public int ParamsGetTgId(TG.ID_TIME id_time, int sensor)
        {
            return m_tg_id[(int)id_time, sensor];         
        }
    }
}