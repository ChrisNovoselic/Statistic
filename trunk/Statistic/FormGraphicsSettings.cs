using System;
using System.Collections.Generic;
//using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using HClassLibrary;
using StatisticCommon;

namespace Statistic
{
    public partial class FormGraphicsSettings : Form
    {
        public enum INDEX_COLOR
        {
            UDG, DIVIATION, ASKUE, SOTIASSO, REC, BG_ASKUE, BG_SOTIASSO, GRID
                , COUNT_INDEX_COLOR
        }

        public enum TYPE_UPDATEGUI
        {
            SCALE, LINEAR, COLOR, SOURCE_DATA
                , COUNT_TYPE_UPDATEGUI };

        private enum CONN_SETT_TYPE {
            ASKUE_PLUS_SOTIASSO, ASKUE, SOTIASSO_3_MIN, SOTIASSO_1_MIN
            , COSTUMIZE
                , COUNT_CONN_SETT_TYPE
        };
        HMark m_markSourceData;

        //public Color udgColor
        //    , divColor
        //    , pColor_ASKUE, pColor_SOTIASSO
        //    , recColor
        //    , m_bgColor_ASKUE, m_bgColor_SOTIASSO
        //    , gridColor;
        public bool scale;
        public GraphTypes m_graphTypes;
        public StatisticCommon.CONN_SETT_TYPE m_connSettType_SourceData;
        private DelegateIntFunc delegateUpdateActiveGui;
        private DelegateFunc delegateHideGraphicsSettings;
        private FormMain m_formMain;

        public enum GraphTypes
        {
            Linear,
            Bar,
        }

        public FormGraphicsSettings(FormMain fm, DelegateIntFunc delUp, DelegateFunc Hide)
        {
            InitializeComponent();

            delegateUpdateActiveGui = delUp;
            delegateHideGraphicsSettings = Hide;
            m_formMain = fm;

            scale = false;
            m_markSourceData = new HMark(0);

            bool bGroupBoxSourceData = false;
            CONN_SETT_TYPE cstGroupBoxSourceData = CONN_SETT_TYPE.ASKUE;
            //�������� ������� ���� ������� � ����������� ����� ��������� ������
            if (HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.SOURCEDATA_CHANGED) == true)
            //if (m_formMain.m_users.IsAllowed(HStatisticUsers.ID_ALLOWED.SOURCEDATA_CHANGED) == true)
            //if ((HStatisticUsers.RoleIsAdmin == true) || (HStatisticUsers.RoleIsKomDisp == true))
            {
                bGroupBoxSourceData = true;
                cstGroupBoxSourceData = CONN_SETT_TYPE.COSTUMIZE;

                m_arRadioButtonSourceData[(int)CONN_SETT_TYPE.ASKUE_PLUS_SOTIASSO].Enabled = HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.SOURCEDATA_ASKUE_PLUS_SOTIASSO);
                m_arRadioButtonSourceData[(int)CONN_SETT_TYPE.SOTIASSO_3_MIN].Enabled = HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.SOURCEDATA_SOTIASSO_3_MIN);
            }
            else
                ;

            this.groupBoxSourceData.Enabled = bGroupBoxSourceData;
            m_markSourceData.Marked((int)cstGroupBoxSourceData);

            checkedSourceData();

            m_graphTypes = GraphTypes.Bar; //�����������
        }

        private Color getForeColor (Color bgColor) {
            return Color.FromArgb((bgColor.R + 128) % 256, (bgColor.G + 128) % 256, (bgColor.B + 128) % 256);
        }

        public Color COLOR(INDEX_COLOR indx)
        {
            return m_arlblColor [(int)indx].BackColor;
        }

        private void checkedSourceData()
        {
            m_arRadioButtonSourceData[(int)CONN_SETT_TYPE.ASKUE_PLUS_SOTIASSO].Checked = m_markSourceData.IsMarked((int)(int)CONN_SETT_TYPE.ASKUE_PLUS_SOTIASSO);
            m_arRadioButtonSourceData[(int)CONN_SETT_TYPE.ASKUE].Checked = m_markSourceData.IsMarked((int)(int)CONN_SETT_TYPE.ASKUE);
            m_arRadioButtonSourceData[(int)CONN_SETT_TYPE.SOTIASSO_3_MIN].Checked = m_markSourceData.IsMarked((int)(int)CONN_SETT_TYPE.SOTIASSO_3_MIN);
            m_arRadioButtonSourceData[(int)CONN_SETT_TYPE.SOTIASSO_1_MIN].Checked = m_markSourceData.IsMarked((int)(int)CONN_SETT_TYPE.SOTIASSO_1_MIN);
            m_arRadioButtonSourceData[(int)CONN_SETT_TYPE.COSTUMIZE].Checked = m_markSourceData.IsMarked((int)(int)CONN_SETT_TYPE.COSTUMIZE);

            if (m_arRadioButtonSourceData[(int)CONN_SETT_TYPE.ASKUE_PLUS_SOTIASSO].Checked == true)
                m_connSettType_SourceData = StatisticCommon.CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO;
            else
                if (m_arRadioButtonSourceData[(int)CONN_SETT_TYPE.ASKUE].Checked == true)
                    m_connSettType_SourceData = StatisticCommon.CONN_SETT_TYPE.DATA_AISKUE;
                else
                    if (m_arRadioButtonSourceData[(int)CONN_SETT_TYPE.SOTIASSO_3_MIN].Checked == true)
                        m_connSettType_SourceData = StatisticCommon.CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN;
                    else
                        if (m_arRadioButtonSourceData[(int)CONN_SETT_TYPE.SOTIASSO_1_MIN].Checked == true)
                            m_connSettType_SourceData = StatisticCommon.CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN;
                        else
                            if (m_arRadioButtonSourceData[(int)CONN_SETT_TYPE.COSTUMIZE].Checked == true)
                                m_connSettType_SourceData = StatisticCommon.CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE;
                            else
                                ;
        }

        private void cbxScale_CheckedChanged(object sender, EventArgs e)
        {
            scale = cbxScale.Checked;
            delegateUpdateActiveGui((int)TYPE_UPDATEGUI.SCALE);
        }

        private void lbl_color_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.Color = ((Label)sender).BackColor;
            if (cd.ShowDialog(this) == DialogResult.OK)
            {
                ((Label)sender).BackColor = cd.Color;
                ((Label)sender).ForeColor = Color.FromArgb((((Label)sender).BackColor.R + 128) % 256, (((Label)sender).BackColor.G + 128) % 256, (((Label)sender).BackColor.B + 128) % 256);
                delegateUpdateActiveGui((int)TYPE_UPDATEGUI.COLOR);
            }
        }

        private void GraphicsSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            delegateHideGraphicsSettings();
        }

        public void SetScale()
        {
            cbxScale.Checked = !cbxScale.Checked;
        }

        private void rbtnLine_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnBar.Checked == true)
                m_graphTypes = GraphTypes.Bar;
            else
                if (rbtnLine.Checked == true)
                    m_graphTypes = GraphTypes.Linear;
                else
                    ;

            delegateUpdateActiveGui((int)TYPE_UPDATEGUI.LINEAR);
        }

        private void rbtnSourceData_Click()
        {
            checkedSourceData();

            delegateUpdateActiveGui((int)TYPE_UPDATEGUI.SOURCE_DATA);
        }

        private void rbtnSourceData_Click(CONN_SETT_TYPE indx)
        {
            if (m_arRadioButtonSourceData[(int)indx].Checked == false)
            {
                m_markSourceData.UnMarked();
                m_markSourceData.Marked((int)indx);

                rbtnSourceData_Click();
            }
            else
                ;
        }

        private void rbtnSourceData_ASKUEPLUSSOTIASSO_Click(object sender, EventArgs e)
        {
            rbtnSourceData_Click(CONN_SETT_TYPE.ASKUE_PLUS_SOTIASSO);
        }

        private void rbtnSourceData_ASKUE_Click(object sender, EventArgs e)
        {
                rbtnSourceData_Click(CONN_SETT_TYPE.ASKUE);
        }

        private void rbtnSourceData_SOTIASSO3min_Click(object sender, EventArgs e)
        {
            rbtnSourceData_Click(CONN_SETT_TYPE.SOTIASSO_3_MIN);
        }

        private void rbtnSourceData_SOTIASSO1min_Click(object sender, EventArgs e)
        {
            rbtnSourceData_Click(CONN_SETT_TYPE.SOTIASSO_1_MIN);
        }

        private void rbtnSourceData_COSTUMIZE_Click(object sender, EventArgs e)
        {
            rbtnSourceData_Click(CONN_SETT_TYPE.COSTUMIZE);
        }
    }
}