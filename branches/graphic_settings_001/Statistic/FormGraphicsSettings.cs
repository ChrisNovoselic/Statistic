// ���������� ������ �� ����, ������������ � ������������ ���� System
using System;                                                                    
using System.Collections.Generic;
//using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using HClassLibrary;
using StatisticCommon;
/// <summary>
/// ������������ ���� Statistic 
/// </summary>
namespace Statistic
{
    /// <summary>
    /// �������� ��������� ����� FormGraphicsSettings (��������� ��������) 
    /// ����������� �� �������� ������ Form
    /// </summary>
    public partial class FormGraphicsSettings : Form                                 
    {
        #region ������������
        /// <summary>
        /// �������� ������������ INDEX_COLOR (������ �����)
        /// </summary>
        public enum INDEX_COLOR                                                    
        {
            /// <summary>
            /// � ������������ INDEX_COLOR  ���������� 9 ����������� ��������, �� ���������
            /// ������� ��-�� ������������� 0, ��������� n+1
            /// </summary>
            UDG, DIVIATION, ASKUE, SOTIASSO, REC, BG_ASKUE, BG_SOTIASSO, GRID      
                , COUNT_INDEX_COLOR                                                     
        }

        /// <summary>
        /// �������� ������������  TYPE_UPDATEGUI  (���� ��������)   
        /// </summary>
        public enum TYPE_UPDATEGUI                                                                                              
        {
        
            SCALE, LINEAR, COLOR, SOURCE_DATA                                     
               , COUNT_TYPE_UPDATEGUI
        };

        /// <summary>
        /// �������� ������������  CONN_SETT_TYPE (��������� ������)
        /// </summary>
        private enum CONN_SETT_TYPE
        {                                         
            AISKUE_PLUS_SOTIASSO                                                
            , AISKUE_3_MIN//, AISKUE_30_MIN
            , SOTIASSO_3_MIN, SOTIASSO_1_MIN
            , COSTUMIZE
                , COUNT_CONN_SETT_TYPE
        }

        /// <summary>
        /// �������� ������������ GraphTypes  (���� ��������)
        /// </summary>
        public enum GraphTypes                                                       
        {
            //��������    
            Linear,
            //�����������                                                     
            Bar,                                                                
        }
        #endregion

        /// <summary>
        /// ������ ��������� ������������� ���������� ������
        /// </summary>
        HMark m_markSourceData;

        /// <summary>
        /// �������� ���� ���� Color. 
        /// </summary>
        //public Color udgColor                                                     
        //    , divColor
        //    , pColor_ASKUE, pColor_SOTIASSO
        //    , recColor
        //    , m_bgColor_ASKUE, m_bgColor_SOTIASSO
        //    , gridColor;

        #region ������ 1
        /// <summary>
        /// �������� ���� scale (�������) ���� bool
        /// </summary>
        public bool scale;

        /// <summary>
        /// �������� ���� m_graphTypes (���� ��������) ���� GraphTypes
        /// </summary>
        public GraphTypes m_graphTypes;
        #endregion

        /// <summary>
        /// �������� ���� m_connSettType_SourceData (�������� ������) ���� StatisticCommon.CONN_SETT_TYPE
        /// </summary>
        public StatisticCommon.CONN_SETT_TYPE m_connSettType_SourceData;

        /// <summary>
        /// ?? �������� �������� ��������� (����, � ����� �������)
        /// </summary>
        private DelegateIntFunc delegateUpdateActiveGui;

        /// <summary>
        /// ??������� ���� �������� �������� (����, � ����� �������)
        /// </summary>
        private DelegateFunc delegateHideGraphicsSettings;

        /// <summary>
        /// �������� ���� m_formMain ���� FormMain. ����� ���?
        /// </summary>
        private FormMain m_formMain;






        /// <summary>
        /// �������� ���������������� ����������� FormGraphicsSettings �������������� ���� m_formMain, delegateUpdateActiveGui, delegateHideGraphicsSettings
        /// </summary>
        /// <param name="fm"></param>
        /// <param name="delUp"></param>
        /// <param name="Hide"></param>
        public FormGraphicsSettings(FormMain fm, DelegateIntFunc delUp, DelegateFunc Hide) 
        {
            
            InitializeComponent();                                                                       //??                                                        

            // ������������� ����� ��������� ������������� ����������
            delegateUpdateActiveGui = delUp;                                                            
            delegateHideGraphicsSettings = Hide;                                                     
            m_formMain = fm;
            //��������������� ��������� �� ���������
            scale = false;
            // ���� m_markSourceData ����������� ������ �� ��������� ������ HMark, �������� ����������� HMark � ����� ����������, �������� 0                                                                  
            m_markSourceData = new HMark(0);          

            bool bGroupBoxSourceData = false;                                                            //���������� bGroupBoxSourceData ����������� false
            CONN_SETT_TYPE cstGroupBoxSourceData = CONN_SETT_TYPE.AISKUE_3_MIN;                          //���������� cstGroupBoxSourceData ����������� ���������=1 (AISKUE_3_MIN)
            //�������� ������� ���� ������� � ����������� ����� ��������� ������
            if (HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.SOURCEDATA_CHANGED) == true)   
            //if (m_formMain.m_users.IsAllowed(HStatisticUsers.ID_ALLOWED.SOURCEDATA_CHANGED) == true)
            //if ((HStatisticUsers.RoleIsAdmin == true) || (HStatisticUsers.RoleIsKomDisp == true))
            {
                bGroupBoxSourceData = true;                        //���������� bGroupBoxSourceData ����������� true (��������� �������� ������)
                cstGroupBoxSourceData = CONN_SETT_TYPE.COSTUMIZE;  //���������� cstGroupBoxSourceData ����������� ���������=4 (�� ��������� ���������� COSTUMIZE)

                //������ ������+�������� � ��������(3 ���) ���������� ��������� (�� ����� ��� ��������..?)
                m_arRadioButtonSourceData[(int)CONN_SETT_TYPE.AISKUE_PLUS_SOTIASSO].Enabled = HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.SOURCEDATA_ASKUE_PLUS_SOTIASSO);
                m_arRadioButtonSourceData[(int)CONN_SETT_TYPE.SOTIASSO_3_MIN].Enabled = HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.SOURCEDATA_SOTIASSO_3_MIN);
            }
            else
                ;

            this.groupBoxSourceData.Enabled = bGroupBoxSourceData;       //??                                 
            m_markSourceData.Marked((int)cstGroupBoxSourceData);

            checkedSourceData();           // ����� ������ �������� ��������� ������

            m_graphTypes = GraphTypes.Bar; // ��� �������-����������� �� ���������
        }




        /// <summary>
        /// �������� ����� getForeColor (�������� ���� �������) ��������� �������� ���� ��������� Color (��������� ���� ������� �����)
        /// � ���������� ���� ������� (��������� �����)
        /// </summary>
        /// <param name="bgColor"></param>
        /// <returns></returns>

        private Color getForeColor (Color bgColor)  
        {
            
            return Color.FromArgb((bgColor.R + 128) % 256, (bgColor.G + 128) % 256, (bgColor.B + 128) % 256); 
        }


        /// <summary>
        /// �������� ����� COLOR ��������� �������� ���� INDEX_COLOR (������ �������������� ���������)
        /// � ���������� ��������������� ���� � �������
        /// </summary>
        /// <param name="indx"></param>
        /// <returns></returns>
        public Color COLOR(INDEX_COLOR indx)
        {
            return m_arlblColor [(int)indx].BackColor;     
        }

        /// <summary>
        /// �������� ����� checkedSourceData (��������� �������� ������) ������ �� ���������, ������ �� ����������
        /// </summary>
        private void checkedSourceData()     
        {
            m_arRadioButtonSourceData[(int)CONN_SETT_TYPE.AISKUE_PLUS_SOTIASSO].Checked = m_markSourceData.IsMarked((int)(int)CONN_SETT_TYPE.AISKUE_PLUS_SOTIASSO);//??
            m_arRadioButtonSourceData[(int)CONN_SETT_TYPE.AISKUE_3_MIN].Checked = m_markSourceData.IsMarked((int)(int)CONN_SETT_TYPE.AISKUE_3_MIN);
            m_arRadioButtonSourceData[(int)CONN_SETT_TYPE.SOTIASSO_3_MIN].Checked = m_markSourceData.IsMarked((int)(int)CONN_SETT_TYPE.SOTIASSO_3_MIN);
            m_arRadioButtonSourceData[(int)CONN_SETT_TYPE.SOTIASSO_1_MIN].Checked = m_markSourceData.IsMarked((int)(int)CONN_SETT_TYPE.SOTIASSO_1_MIN);
            m_arRadioButtonSourceData[(int)CONN_SETT_TYPE.COSTUMIZE].Checked = m_markSourceData.IsMarked((int)(int)CONN_SETT_TYPE.COSTUMIZE);
            
            //���� ������ ������ "������+��������", �� ��������� ������ ��������� ��������  "������+��������"
            if (m_arRadioButtonSourceData[(int)CONN_SETT_TYPE.AISKUE_PLUS_SOTIASSO].Checked == true)    
                m_connSettType_SourceData = StatisticCommon.CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO;
            else
            //���� ������ ������ "������(3���)", �� ��������� ������ ��������� ��������  "������(3���)"
                if (m_arRadioButtonSourceData[(int)CONN_SETT_TYPE.AISKUE_3_MIN].Checked == true)
                    m_connSettType_SourceData = StatisticCommon.CONN_SETT_TYPE.DATA_AISKUE;
                else
                    if (m_arRadioButtonSourceData[(int)CONN_SETT_TYPE.SOTIASSO_3_MIN].Checked == true)           //����������
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


        /// <summary>
        /// �������� ����� cbxScale_CheckedChanged (�������� ��������� ��������), 
        /// ����������� ������� ������� �� ������ "���������������" � ������ �� ������������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxScale_CheckedChanged(object sender, EventArgs e)
        {
            scale = cbxScale.Checked;                              //���� ��������� �����������  ��������
            delegateUpdateActiveGui((int)TYPE_UPDATEGUI.SCALE);    //�������� �������� ��������� (�������)
        }



        /// <summary>
        /// �������� ����� lbl_color_Click (������� �����), ����������� ������� ������� �� ��������� ����
        /// � ������ �� ������������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbl_color_Click(object sender, EventArgs e)   
        {
            ColorDialog cd = new ColorDialog();                   //������� ��������� cd ������ ColorDialog (���������� ���� "����")
            cd.Color = ((Label)sender).BackColor;                 //������� ��������� Color �� ����������, ��������� ��������� �������� ���������� �����
            if (cd.ShowDialog(this) == DialogResult.OK)           // ���� ������ ���� � ����� ��, ��
            {
                //������� ����� ��������� ��������� ����
                ((Label)sender).BackColor = cd.Color;
                //��������� ����� (�������) ��������� ��������� �������� ����
                ((Label)sender).ForeColor = Color.FromArgb((((Label)sender).BackColor.R + 128) % 256, (((Label)sender).BackColor.G + 128) % 256, (((Label)sender).BackColor.B + 128) % 256);
                //�������� �������� ��������� (����)
                delegateUpdateActiveGui((int)TYPE_UPDATEGUI.COLOR);
            } else
                ;
        }

        /// <summary>
        /// �������� ����� GraphicsSettings_FormClosing (�������� �����),
        /// ����������� ������� �������� ����� (������� �� �������?) � ������ �� ������������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GraphicsSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;                   // ������=true
            delegateHideGraphicsSettings();    //����� ����?
        }

        /// <summary>
        /// �������� ����� SetScale (���������� �������),������ �� ��������� � �� ����������
        /// </summary>
        public void SetScale()
        {
            //������������� ������� �������� ��������
            cbxScale.Checked = !cbxScale.Checked;        
        }

        /// <summary>
        /// �������� ����� rbtnLine_CheckedChanged (�������� ��������� ���� �������), 
        /// ����������� ������� ������� �� ������ "��������" ��� "�����������"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbtnLine_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnBar.Checked == true)               //���� ������ "�����������" ������
                m_graphTypes = GraphTypes.Bar;         // ����  m_graphTypes ��������� "������������"
            else
                if (rbtnLine.Checked == true)          //���� ������ "��������" ������
                m_graphTypes = GraphTypes.Linear;      // ����  m_graphTypes ��������� "��������"
            else
                    ;
            
            delegateUpdateActiveGui((int)TYPE_UPDATEGUI.LINEAR);   //�������� �������� ��������� (��������)
        }


        /// <summary>
        /// �������� ����� rbtnSourceData_Click( ������� ��������� ������), ������ �� ��������� � �� ����������
        /// </summary>
        private void rbtnSourceData_Click()
        {
            checkedSourceData();                //����� ������-��������� �������� ������

            delegateUpdateActiveGui((int)TYPE_UPDATEGUI.SOURCE_DATA); //�������� �������� ��������� (�������� ������)
        }


        /// <summary>
        /// ������������� �����, ����������� ������ ��������� ����������
        /// </summary>
        /// <param name="indx"></param>
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


        /// <summary>
        /// �������� ����� rbtnSourceData_ASKUEPLUSSOTIASSO_Click, ����������� ������� ������� �� "������+��������"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbtnSourceData_ASKUEPLUSSOTIASSO_Click(object sender, EventArgs e)
        {
            //���������� ����� rbtnSourceData_Click, ����������� ������ ������������ (� ������ ������ ���������� 0)
            rbtnSourceData_Click(CONN_SETT_TYPE.AISKUE_PLUS_SOTIASSO);
        }

        private void rbtnSourceData_ASKUE_Click(object sender, EventArgs e)        // ����������
        {
                rbtnSourceData_Click(CONN_SETT_TYPE.AISKUE_3_MIN);
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