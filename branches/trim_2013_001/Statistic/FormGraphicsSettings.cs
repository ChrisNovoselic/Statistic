using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using StatisticCommon;

namespace Statistic
{
    public partial class FormGraphicsSettings : Form
    {
        public Color udgColor;
        public Color divColor;
        public Color pColor;
        public Color recColor;
        public Color bgColor;
        public Color gridColor;
        public bool scale;
        public GraphTypes graphTypes;
        private DelegateFunc delegateUpdateActiveGui;
        private DelegateFunc delegateHideGraphicsSettings;
        private FormMain mainForm;

        public enum GraphTypes
        {
            Linear,
            Bar,
        }

        public FormGraphicsSettings(FormMain mf, DelegateFunc delUp, DelegateFunc Hide)
        {
            InitializeComponent();

            delegateUpdateActiveGui = delUp;
            delegateHideGraphicsSettings = Hide;
            mainForm = mf;

            scale = false;

            udgColor = Color.FromArgb(0, 0, 0);
            lblUDGcolor.BackColor = udgColor;
            lblUDGcolor.ForeColor = Color.FromArgb((udgColor.R + 128) % 256, (udgColor.G + 128) % 256, (udgColor.B + 128) % 256);

            divColor = Color.FromArgb(255, 0, 0);
            lblDIVcolor.BackColor = divColor;
            lblDIVcolor.ForeColor = Color.FromArgb((divColor.R + 128) % 256, (divColor.G + 128) % 256, (divColor.B + 128) % 256);

            pColor = Color.FromArgb(0, 128, 0);
            lblPcolor.BackColor = pColor;
            lblPcolor.ForeColor = Color.FromArgb((pColor.R + 128) % 256, (pColor.G + 128) % 256, (pColor.B + 128) % 256);

            recColor = Color.FromArgb(255, 255, 0);
            lblRECcolor.BackColor = recColor;
            lblRECcolor.ForeColor = Color.FromArgb((recColor.R + 128) % 256, (recColor.G + 128) % 256, (recColor.B + 128) % 256);

            bgColor = Color.FromArgb(230, 230, 230);
            lblBGcolor.BackColor = bgColor;
            lblBGcolor.ForeColor = Color.FromArgb((bgColor.R + 128) % 256, (bgColor.G + 128) % 256, (bgColor.B + 128) % 256);

            gridColor = Color.FromArgb(200, 200, 200);
            lblGRIDcolor.BackColor = gridColor;
            lblGRIDcolor.ForeColor = Color.FromArgb((gridColor.R + 128) % 256, (gridColor.G + 128) % 256, (gridColor.B + 128) % 256);

            graphTypes = GraphTypes.Bar;
        }

        private void cbxScale_CheckedChanged(object sender, EventArgs e)
        {
            scale = cbxScale.Checked;
            delegateUpdateActiveGui();
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
                delegateUpdateActiveGui();
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
                delegateUpdateActiveGui();
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
                delegateUpdateActiveGui();
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
                delegateUpdateActiveGui();
            }
        }

        private void lblBGcolor_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.Color = bgColor;
            if (cd.ShowDialog(this) == DialogResult.OK)
            {
                bgColor = cd.Color;
                lblBGcolor.BackColor = bgColor;
                lblBGcolor.ForeColor = Color.FromArgb((bgColor.R + 128) % 256, (bgColor.G + 128) % 256, (bgColor.B + 128) % 256);
                delegateUpdateActiveGui();
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
                delegateUpdateActiveGui();
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
                graphTypes = GraphTypes.Bar;
            else
                ;

            if (rbtnLine.Checked == true)
                graphTypes = GraphTypes.Linear;
            else
                ;

            delegateUpdateActiveGui();
        }
    }
}