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
        public enum TYPE_UPDATEGUI
        {
            SCALE, LINEAR, COLOR, SOURCE_DATA
        , COUNT_TYPE_UPDATEGUI };

        private enum CONN_SETT_TYPE {
            ASKUE, SOTIASSO, COSTUMIZE
            , COUNT_CONN_SETT_TYPE
        };
        HMark m_markSourceData;

        public Color udgColor
            , divColor
            , pColor
            , recColor
            , m_bgColor_ASKUE, m_bgColor_SOTIASSO
            , gridColor;
        public bool scale;
        public GraphTypes m_graphTypes;
        public StatisticCommon.CONN_SETT_TYPE m_connSettType_SourceData;
        private DelegateIntFunc delegateUpdateActiveGui;
        private DelegateFunc delegateHideGraphicsSettings;
        private FormMain mainForm;

        public enum GraphTypes
        {
            Linear,
            Bar,
        }

        public FormGraphicsSettings(FormMain mf, DelegateIntFunc delUp, DelegateFunc Hide)
        {
            InitializeComponent();

            delegateUpdateActiveGui = delUp;
            delegateHideGraphicsSettings = Hide;
            mainForm = mf;

            scale = false;
            m_markSourceData = new HMark();
            //Проверка условия прав доступа к возможности смены источника данных
            //...
            m_markSourceData.Marked((int)CONN_SETT_TYPE.COSTUMIZE);
            this.groupBoxSourceData.Enabled = true;            

            checkedSourceData();

            udgColor = Color.FromArgb(0, 0, 0);
            lblUDGcolor.BackColor = udgColor;
            lblUDGcolor.ForeColor = getForeColor(lblUDGcolor.BackColor);

            divColor = Color.FromArgb(255, 0, 0);
            lblDIVcolor.BackColor = divColor;
            lblDIVcolor.ForeColor = getForeColor(lblDIVcolor.BackColor);

            pColor = Color.FromArgb(0, 128, 0);
            lblPcolor.BackColor = pColor;
            lblPcolor.ForeColor = getForeColor(lblPcolor.BackColor);

            recColor = Color.FromArgb(255, 255, 0);
            lblRECcolor.BackColor = recColor;
            lblRECcolor.ForeColor = getForeColor(lblRECcolor.BackColor);

            m_bgColor_ASKUE = Color.FromArgb(231, 231, 238 /*230, 230, 230*/);
            lblBG_ASKUE_color.BackColor = m_bgColor_ASKUE;
            lblBG_ASKUE_color.ForeColor = getForeColor(lblBG_ASKUE_color.BackColor);

            m_bgColor_SOTIASSO = Color.FromArgb (231, 238, 231);
            lblBG_SOTIASSO_color.BackColor = m_bgColor_SOTIASSO;
            lblBG_SOTIASSO_color.ForeColor = getForeColor(lblBG_SOTIASSO_color.BackColor);

            gridColor = Color.FromArgb(200, 200, 200);
            lblGRIDcolor.BackColor = gridColor;
            lblGRIDcolor.ForeColor = getForeColor(lblGRIDcolor.BackColor);

            m_graphTypes = GraphTypes.Bar; //Гистограмма
        }

        private Color getForeColor (Color bgColor) {
            return Color.FromArgb((bgColor.R + 128) % 256, (bgColor.G + 128) % 256, (bgColor.B + 128) % 256);
        }

        private void checkedSourceData()
        {
            rbtnSourceData_ASKUE.Checked = m_markSourceData.IsMarked((int)(int)CONN_SETT_TYPE.ASKUE);
            rbtnSourceData_SOTIASSO.Checked = m_markSourceData.IsMarked((int)(int)CONN_SETT_TYPE.SOTIASSO);
            rbtnSourceData_COSTUMIZE.Checked = m_markSourceData.IsMarked((int)(int)CONN_SETT_TYPE.COSTUMIZE);

            if (rbtnSourceData_ASKUE.Checked == true)
                m_connSettType_SourceData = StatisticCommon.CONN_SETT_TYPE.DATA_ASKUE;
            else
                if (rbtnSourceData_SOTIASSO.Checked == true)
                    m_connSettType_SourceData = StatisticCommon.CONN_SETT_TYPE.DATA_SOTIASSO;
                else
                    if (rbtnSourceData_COSTUMIZE.Checked == true)
                        m_connSettType_SourceData = StatisticCommon.CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE;
                    else
                        ;
        }

        private void cbxScale_CheckedChanged(object sender, EventArgs e)
        {
            scale = cbxScale.Checked;
            delegateUpdateActiveGui((int)TYPE_UPDATEGUI.SCALE);
        }

        private void lblUDGcolor_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.Color = udgColor;
            if (cd.ShowDialog(this) == DialogResult.OK)
            {
                udgColor = cd.Color;
                lblUDGcolor.BackColor = udgColor;
                lblUDGcolor.ForeColor = Color.FromArgb((udgColor.R + 128) % 256, (udgColor.G + 128) % 256, (udgColor.B + 128) % 256);
                delegateUpdateActiveGui((int)TYPE_UPDATEGUI.COLOR);
            }
        }

        private void lblDIVcolor_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.Color = divColor;
            if (cd.ShowDialog(this) == DialogResult.OK)
            {
                divColor = cd.Color;
                lblDIVcolor.BackColor = divColor;
                lblDIVcolor.ForeColor = Color.FromArgb((divColor.R + 128) % 256, (divColor.G + 128) % 256, (divColor.B + 128) % 256);
                delegateUpdateActiveGui((int)TYPE_UPDATEGUI.COLOR);
            }
        }

        private void lblPcolor_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.Color = pColor;
            if (cd.ShowDialog(this) == DialogResult.OK)
            {
                pColor = cd.Color;
                lblPcolor.BackColor = pColor;
                lblPcolor.ForeColor = Color.FromArgb((pColor.R + 128) % 256, (pColor.G + 128) % 256, (pColor.B + 128) % 256);
                delegateUpdateActiveGui((int)TYPE_UPDATEGUI.COLOR);
            }
        }

        private void lblRECcolor_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.Color = recColor;
            if (cd.ShowDialog(this) == DialogResult.OK)
            {
                recColor = cd.Color;
                lblRECcolor.BackColor = recColor;
                lblRECcolor.ForeColor = Color.FromArgb((recColor.R + 128) % 256, (recColor.G + 128) % 256, (recColor.B + 128) % 256);
                delegateUpdateActiveGui((int)TYPE_UPDATEGUI.COLOR);
            }
        }

        private void lblBG_ASKUE_color_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.Color = m_bgColor_ASKUE;
            if (cd.ShowDialog(this) == DialogResult.OK)
            {
                m_bgColor_ASKUE = cd.Color;
                lblBG_ASKUE_color.BackColor = m_bgColor_ASKUE;
                lblBG_ASKUE_color.ForeColor = getForeColor(lblBG_ASKUE_color.BackColor);
                delegateUpdateActiveGui((int)TYPE_UPDATEGUI.COLOR);
            }
        }

        private void lblBG_SOTIASSO_color_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.Color = m_bgColor_SOTIASSO;
            if (cd.ShowDialog(this) == DialogResult.OK)
            {
                m_bgColor_SOTIASSO = cd.Color;
                lblBG_ASKUE_color.BackColor = m_bgColor_SOTIASSO;
                lblBG_ASKUE_color.ForeColor = getForeColor(lblBG_ASKUE_color.BackColor);
                delegateUpdateActiveGui((int)TYPE_UPDATEGUI.COLOR);
            }
        }

        private void lblGRIDcolor_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.Color = gridColor;
            if (cd.ShowDialog(this) == DialogResult.OK)
            {
                gridColor = cd.Color;
                lblGRIDcolor.BackColor = gridColor;
                lblGRIDcolor.ForeColor = Color.FromArgb((gridColor.R + 128) % 256, (gridColor.G + 128) % 256, (gridColor.B + 128) % 256);
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

        private void rbtnSourceData_ASKUE_Click(object sender, EventArgs e)
        {
            if (rbtnSourceData_ASKUE.Checked == false)
            {
                m_markSourceData.UnMarked();
                m_markSourceData.Marked((int)CONN_SETT_TYPE.ASKUE);

                rbtnSourceData_Click();
            }
            else
                ;
        }

        private void rbtnSourceData_SOTIASSO_Click(object sender, EventArgs e)
        {
            if (rbtnSourceData_SOTIASSO.Checked == false)
            {
                m_markSourceData.UnMarked();
                m_markSourceData.Marked((int)CONN_SETT_TYPE.SOTIASSO);

                rbtnSourceData_Click();
            }
            else
                ;
        }

        private void rbtnSourceData_COSTUMIZE_Click(object sender, EventArgs e)
        {
            if (rbtnSourceData_COSTUMIZE.Checked == false)
            {
                m_markSourceData.UnMarked();
                m_markSourceData.Marked((int)CONN_SETT_TYPE.COSTUMIZE);

                rbtnSourceData_Click();
            }
            else
                ;
        }
    }
}