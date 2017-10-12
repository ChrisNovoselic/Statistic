// ���������� ������ �� ����, ������������ � ������������ ���� System
using System;                                                                    
using System.Collections.Generic;
//using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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
            /// � ������������ INDEX_COLOR  ���������� 11 ����������� ��������, �� ���������
            /// ������� ��-�� ������������� 0, ��������� n+1
            /// </summary>
            UDG, DIVIATION, ASKUE, ASKUE_LK_REGULAR, SOTIASSO, REC, TEMP_ASKUTE,
            BG_ASKUE, BG_SOTIASSO, BG_ASKUTE, GRID, COUNT_INDEX_COLOR                                                     
        }

        /// <summary>
        /// �������� ������������  TYPE_UPDATEGUI  (���� ���������������� ��������)   
        /// </summary>
        public enum TYPE_UPDATEGUI                                                                                              
        {
        
            SCALE, LINEAR, COLOR, SOURCE_DATA, COLOR_SHEMA, COLOR_CHANGESHEMA
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

        /// <summary>
        /// �������� ������������ ColorShemas  (�������� �����)
        /// </summary>
        public enum ColorShemas {
            /// <summary>
            /// ��������� �����
            /// </summary>
            System,
            /// <summary>
            /// ��������� �����
            /// </summary>
            Custom,
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

        #region ����
        /// <summary>
        /// �������� ���� scale (�������) ���� bool
        /// </summary>
        public bool scale;

        /// <summary>
        /// �������� ���� m_graphTypes (���� ��������) ���� GraphTypes
        /// </summary>
        public GraphTypes m_graphTypes;

        public ColorShemas m_colorShema;
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
        private bool _allowedChangeShema;

        #region �����������
        /// <summary>
        /// �������� ���������������� ����������� FormGraphicsSettings �������������� ���� m_formMain, delegateUpdateActiveGui, delegateHideGraphicsSettings
        /// </summary>
        /// <param name="form">������������ ����� - ������� ���� ����������</param>
        /// <param name="fUpdate">����� ��� ���������� ���������</param>
        /// <param name="fHide">����� ������ � ����������� ����������� ����</param>
        /// <param name="bAllowedChangeShema">�������(������������� �� ��) ���������� �������� �������� �����</param>
        public FormGraphicsSettings (DelegateIntFunc fUpdate, DelegateFunc fHide, bool bAllowedChangeShema) 
        {
            // ������������� ����� ��������� ������������� ����������
            delegateUpdateActiveGui = fUpdate;
            delegateHideGraphicsSettings = fHide;
            _allowedChangeShema = bAllowedChangeShema;
            //��������������� ��������� �� ���������
            scale = false;
            // ���� m_markSourceData ����������� ������ �� ��������� ������ HMark, �������� ����������� HMark � ����� ����������, �������� 0
            m_markSourceData = new HMark (0);

            InitializeComponent ();

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
                m_arRbtnSourceData[(int)CONN_SETT_TYPE.AISKUE_PLUS_SOTIASSO].Enabled = HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.SOURCEDATA_ASKUE_PLUS_SOTIASSO);
                m_arRbtnSourceData[(int)CONN_SETT_TYPE.SOTIASSO_3_MIN].Enabled = HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.SOURCEDATA_SOTIASSO_3_MIN);
            }
            else
                ;

            this.gbxSourceData.Enabled = bGroupBoxSourceData;       //??                                 
            m_markSourceData.Marked((int)cstGroupBoxSourceData);

            checkedSourceData();           // ����� ������ �������� ��������� ������

            m_graphTypes = GraphTypes.Bar; // ��� �������-����������� �� ���������
        }
        #endregion

        #region ������
        /// <summary>
        /// �������� ����� getForeColor (�������� ���� �������) ��������� �������� ���� ��������� Color (��������� ���� ������� �����)
        /// � ���������� ���� ������� (��������� �����)
        /// </summary>
        /// <param name="bgColor">������� ���� ����, �� ������� ����������� �������</param>
        /// <returns>���� ��� �������</returns>
        private Color getForeColor (Color bgColor)  
        {            
            return Color.FromArgb((bgColor.R + 128) % 256, (bgColor.G + 128) % 256, (bgColor.B + 128) % 256); 
        }

        /// <summary>
        /// �������� ����� COLOR ��������� �������� ���� INDEX_COLOR (������ �������������� ���������)
        /// � ���������� ��������������� ���� � �������
        /// </summary>
        /// <param name="indx">������ ����� � �������</param>
        /// <returns>���� �� �������</returns>
        public Color COLOR(INDEX_COLOR indx)
        {
            Color colorRes = Color.Empty;

            if (m_colorShema == ColorShemas.System)
                colorRes = m_arlblColor [(int)indx].BackColor;
            else
                switch (indx) {
                    case INDEX_COLOR.BG_ASKUE:
                    case INDEX_COLOR.BG_SOTIASSO:
                    case INDEX_COLOR.BG_ASKUTE:
                        colorRes = DarkColorTable._Custom;
                        break;
                    default:
                        colorRes = m_arlblColor [(int)indx].BackColor;
                        break;
                }

            return colorRes;
        }
        /// <summary>
        /// ���������� �������� ������������� ����� ���������� ������
        /// , �������� ����� checkedSourceData (��������� �������� ������) ������ �� ���������, ������ �� ����������
        /// </summary>
        private void checkedSourceData()     
        {
            m_arRbtnSourceData[(int)CONN_SETT_TYPE.AISKUE_PLUS_SOTIASSO].Checked = m_markSourceData.IsMarked((int)(int)CONN_SETT_TYPE.AISKUE_PLUS_SOTIASSO);//??
            m_arRbtnSourceData[(int)CONN_SETT_TYPE.AISKUE_3_MIN].Checked = m_markSourceData.IsMarked((int)(int)CONN_SETT_TYPE.AISKUE_3_MIN);
            m_arRbtnSourceData[(int)CONN_SETT_TYPE.SOTIASSO_3_MIN].Checked = m_markSourceData.IsMarked((int)(int)CONN_SETT_TYPE.SOTIASSO_3_MIN);
            m_arRbtnSourceData[(int)CONN_SETT_TYPE.SOTIASSO_1_MIN].Checked = m_markSourceData.IsMarked((int)(int)CONN_SETT_TYPE.SOTIASSO_1_MIN);
            m_arRbtnSourceData[(int)CONN_SETT_TYPE.COSTUMIZE].Checked = m_markSourceData.IsMarked((int)(int)CONN_SETT_TYPE.COSTUMIZE);
            
            //���� ������ ������ "������+��������", �� ��������� ������ ��������� ��������  "������+��������"
            if (m_arRbtnSourceData[(int)CONN_SETT_TYPE.AISKUE_PLUS_SOTIASSO].Checked == true)    
                m_connSettType_SourceData = StatisticCommon.CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO;
            else
            //���� ������ ������ "������(3���)", �� ��������� ������ ��������� ��������  "������(3���)"
                if (m_arRbtnSourceData[(int)CONN_SETT_TYPE.AISKUE_3_MIN].Checked == true)
                    m_connSettType_SourceData = StatisticCommon.CONN_SETT_TYPE.DATA_AISKUE;
                else
                    if (m_arRbtnSourceData[(int)CONN_SETT_TYPE.SOTIASSO_3_MIN].Checked == true)           //����������
                        m_connSettType_SourceData = StatisticCommon.CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN;
                    else
                        if (m_arRbtnSourceData[(int)CONN_SETT_TYPE.SOTIASSO_1_MIN].Checked == true)
                            m_connSettType_SourceData = StatisticCommon.CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN;
                        else
                            if (m_arRbtnSourceData[(int)CONN_SETT_TYPE.COSTUMIZE].Checked == true)
                                m_connSettType_SourceData = StatisticCommon.CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE;
                            else
                                ;
        }
        /// <summary>
        /// �������� ����� cbxScale_CheckedChanged (�������� ��������� ��������), 
        /// ����������� ������� ������� �� ������ "���������������" � ������ �� ������������
        /// </summary>
        /// <param name="sender">������, �������������� �������</param>
        /// <param name="e">�������� �������</param>
        private void cbxScale_CheckedChanged(object sender, EventArgs e)
        {
            scale = cbxScale.Checked;                              //���� ��������� �����������  ��������
            delegateUpdateActiveGui((int)TYPE_UPDATEGUI.SCALE);    //�������� �������� ��������� (�������)
        }
        /// <summary>
        /// �������� ����� lbl_color_Click (������� �����), ����������� ������� ������� �� ��������� ����
        /// � ������ �� ������������
        /// </summary>
        /// <param name="sender">������, �������������� ������� (�������)</param>
        /// <param name="e">�������� �������</param>
        private void lbl_color_Click(object sender, EventArgs e)   
        {
            TYPE_UPDATEGUI typeUpdate = ((sender as Control).Tag.GetType ().Equals (typeof (INDEX_COLOR))) == true
                ? TYPE_UPDATEGUI.COLOR
                    : TYPE_UPDATEGUI.COLOR_CHANGESHEMA;

            ColorDialog cd = new ColorDialog();                   // ������� ��������� cd ������ ColorDialog (���������� ���� "����")
            cd.Color = ((Label)sender).BackColor;                 // ������� ��������� Color �� ����������, ��������� ��������� �������� ���������� �����
            if (cd.ShowDialog(this) == DialogResult.OK)           //  , ���� ������ ���� � ����� ��, ��
            {
                // ������� ����� ��������� ��������� ����
                ((Label)sender).BackColor = cd.Color;
                // ��������� ����� (�������) ��������� ��������� �������� ����
                ((Label)sender).ForeColor = getForeColor (cd.Color);
                // ��� ���� 'TYPE_UPDATEGUI.COLOR_SHEMA' ��������� ������. ��������
                if (typeUpdate == TYPE_UPDATEGUI.COLOR) {
                    // �������� �������� ��������� (����)
                    // ��� 'TYPE_UPDATEGUI.COLOR_SHEMA' �������� ��������� ��������� � 'BackColorChanged'
                    delegateUpdateActiveGui ((int)typeUpdate);
                    
                } else
                    //// �������� � ���� �������
                    //((Label)sender).BorderColor = getForeColor (cd.Color)
                    ;
            } else
                ;
        }

        /// <summary>
        /// �������� ����� GraphicsSettings_FormClosing (�������� �����),
        /// ����������� ������� �������� ����� (������� �� �������?) � ������ �� ������������
        /// </summary>
        /// <param name="sender">������, �������������� �������</param>
        /// <param name="e">�������� �������</param>
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

        private void cbUseSystemColors_CheckedChanged (object sender, EventArgs e)
        {
            m_colorShema = (sender as System.Windows.Forms.CheckBox).Checked == true ? ColorShemas.System : ColorShemas.Custom;

            m_arlblColor [(int)INDEX_COLOR.BG_ASKUE].Enabled =
            m_arlblColor [(int)INDEX_COLOR.BG_ASKUE].Enabled =
            m_arlblColor [(int)INDEX_COLOR.BG_ASKUE].Enabled =
                (sender as System.Windows.Forms.CheckBox).Checked;

            m_labelColorShema.Enabled = !(sender as System.Windows.Forms.CheckBox).Checked;

            delegateUpdateActiveGui ((int)TYPE_UPDATEGUI.COLOR_SHEMA);   //�������� �������� ��������� (�������� �����)
        }

        /// <summary>
        /// �������� ����� rbtnLine_CheckedChanged (�������� ��������� ���� �������), 
        /// ����������� ������� ������� �� ������ "��������" ��� "�����������"
        /// </summary>
        /// <param name="sender">������, �������������� �������</param>
        /// <param name="e">�������� �������</param>
        private void rbtnTypeGraph_CheckedChanged(object sender, EventArgs e)
        {
            foreach (RadioButton rbtn in m_arRbtnTypeGraph)
                if (rbtn.Checked == true) {
                    m_graphTypes = (GraphTypes)(rbtn as Control).Tag;

                    break;
                } else
                    ;
            
            delegateUpdateActiveGui((int)TYPE_UPDATEGUI.LINEAR);   //�������� �������� ��������� (��� �������)
        }

        private void rbtnSourceData_Click (object sender, EventArgs e)
        {
            rbtnSourceData_Click ((CONN_SETT_TYPE)(sender as Control).Tag);
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
        /// <param name="indx">������-���-������������� ���� ��������� ������ ��� �����������</param>
        private void rbtnSourceData_Click(CONN_SETT_TYPE indx)   
        {
            if (m_arRbtnSourceData[(int)indx].Checked == false) 
            {
                m_markSourceData.UnMarked();
                m_markSourceData.Marked((int)indx);

                rbtnSourceData_Click();
            }
            else
                ;
        }        
        #endregion
    }

    public class DarkColorTable : ProfessionalColorTable
    {
        public DarkColorTable (Color colorSystem, string colorCustom)
        {
            int [] rgb;

            _System = colorSystem;

            try {
                rgb = Array.ConvertAll<string, int> (colorCustom.Split (','), Convert.ToInt32);                
                _custom = Color.FromArgb (rgb [0], rgb [1], rgb [2]);
            } catch (Exception e) {
                Logging.Logg ().Exception (e, string.Format ("DarkColorTable::ctor () - ..."), Logging.INDEX_MESSAGE.NOT_SET);
            }

            //UseSystemColors = true;
        }

        public static Color _System = Color.Empty;

        private static Color _custom;

        public static Color _Custom
        {// Color.FromArgb (255, 112, 128, 144);  // Color.SlateGray;
            get { return _custom; }

            set { _custom = value; }
        }

        public string CustomToString ()
        {
            return string.Format ("{0},{1},{2}", _custom.R, _custom.G, _custom.B);
        }

        private Color _pressed = Color.FromArgb (255, 52, 68, 84);

        private Color _border = Color.Black;

        public override Color ToolStripBorder { get { return _border; } }

        //public override Color ToolStripGradientBegin { get { return culoare; } }

        //public override Color ToolStripGradientEnd { get { return culoare; } }

        public override Color ToolStripDropDownBackground { get { return _Custom; } }

        //public override Color MenuItemBorder { get { return _Background; } }

        //public override Color MenuItemSelected { get { return _Background; } }        

        //public override Color MenuItemSelectedGradientBegin { get { return _Background; } }

        //public override Color MenuItemSelectedGradientEnd { get { return _Background; } }

        public override Color MenuItemPressedGradientBegin { get { return _pressed; } }

        public override Color MenuItemPressedGradientEnd { get { return _pressed; } }

        public override Color MenuBorder { get { return _border; } }        
    }
}