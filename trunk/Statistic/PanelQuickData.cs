﻿using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

using StatisticCommon;

namespace Statistic
{
    public partial class HPanelTableLayout : TableLayoutPanel
    {
        public HPanelTableLayout()
        {
            InitializeComponent();
        }

        public HPanelTableLayout(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        public Font[] GetFontHLabel()
        {
            string[] textOfMaxLengths = new string[(int)HLabel.TYPE_HLABEL.COUNT_TYPE_HLABEL] { string.Empty, string.Empty, string.Empty };
            SizeF[] szLabelOfMinSizes = new SizeF[(int)HLabel.TYPE_HLABEL.COUNT_TYPE_HLABEL]; //(float.MaxValue, float.MaxValue);

            Font[] fonts = null;
            float fSz = -1F,
                fSzMin = -1F, fSzMax = -1F, fSzStep = float.MinValue;
            SizeF sz;

            Graphics g = this.CreateGraphics();

            int indx = -1, i = -1;

            for (i = (int)HLabel.TYPE_HLABEL.TG; i < (int)HLabel.TYPE_HLABEL.COUNT_TYPE_HLABEL; i++)
            {
                textOfMaxLengths[i] = string.Empty;
                szLabelOfMinSizes[i].Height = szLabelOfMinSizes[i].Width = float.MaxValue;
            }

            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is StatisticCommon.HLabel)
                {
                    indx = (int)((HLabel)ctrl).m_type;
                    if (!(indx == (int)HLabel.TYPE_HLABEL.UNKNOWN))
                    {
                        if (textOfMaxLengths[indx].Length < ctrl.Text.Length) textOfMaxLengths[indx] = ctrl.Text; else ;
                        if ((szLabelOfMinSizes[indx].Height > ctrl.Height) || (szLabelOfMinSizes[indx].Width > ctrl.Width)) { szLabelOfMinSizes[indx].Height = ctrl.Height; szLabelOfMinSizes[indx].Width = ctrl.Width; } else ;
                    }
                    else
                    {
                        Logging.Logg().LogErrorToFile(@"HPanelTableLayout::GetFontHLabel () - type=UNKNOWN, ctrl.Text=" + ctrl.Text);

                        break;
                    }
                }
                else
                {
                }
            }

            if (!(indx == (int)HLabel.TYPE_HLABEL.UNKNOWN))
            {
                fonts = new Font[(int)HLabel.TYPE_HLABEL.COUNT_TYPE_HLABEL];

                for (i = (int)HLabel.TYPE_HLABEL.TG; i < (int)HLabel.TYPE_HLABEL.COUNT_TYPE_HLABEL; i++)
                {
                    fSzMin = szLabelOfMinSizes[i].Height * 0.2F; fSzMax = szLabelOfMinSizes[i].Height * 0.8F; fSzStep = 0.5F;

                    if ((szLabelOfMinSizes[i].Height < float.MaxValue) && (szLabelOfMinSizes[i].Width < float.MaxValue))
                    {
                        szLabelOfMinSizes[i].Height *= 0.86f;
                        szLabelOfMinSizes[i].Width *= 0.86f;


                        //ctrl.Height * 0.29F
                        //for (fSz = fSzMin; fSz < fSzMax; fSz += fSzStep)
                        for (fSz = fSzMax; fSz > fSzMin; fSz -= fSzStep)
                        {
                            fonts[i] = new System.Drawing.Font("Microsoft Sans Serif", fSz, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
                            sz = g.MeasureString(textOfMaxLengths[i], fonts[i]);

                            if ((!(sz.Height > szLabelOfMinSizes[i].Height)) && (!(sz.Width > szLabelOfMinSizes[i].Width)))
                            {
                                break;
                            }
                            else
                            {
                            }
                        }
                    }
                    else
                        ;
                }
            }
            else
                ; //Logging.Logg ().LogErrorToFile (@"HPanelTableLayout::GetFontHLabel () - type=UNKNOWN");

            return fonts;
        }

        public void OnSizeChanged(object obj, EventArgs ev)
        {
            Font[] fonts = GetFontHLabel();

            if (!(fonts == null))
                foreach (Control ctrl in ((TableLayoutPanel)obj).Controls)
                {
                    if (ctrl is StatisticCommon.HLabel)
                    {
                        if (!(fonts[(int)((HLabel)ctrl).m_type] == null))
                            ctrl.Font = fonts[(int)((HLabel)ctrl).m_type];
                        else
                            Logging.Logg().LogErrorToFile(@"HPanelTableLayout::OnSizeChanged () - fonts[" + ((HLabel)ctrl).m_type.ToString() + @"]=null");
                    }
                    else
                    {
                    }
                }
            else
                Logging.Logg().LogErrorToFile(@"HPanelTableLayout::OnSizeChanged () - fonts=null");
        }
    }

    partial class HPanelTableLayout
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            this.SizeChanged += new EventHandler(OnSizeChanged);
        }

        #endregion
    }

    partial class PanelQuickData
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        private TableLayoutPanelCellPosition getPositionCell(CONTROLS indx)
        {
            int row = -1,
                col = -1;
            switch (indx)
            {
                case CONTROLS.lblCommonP:
                    row = 0; col = 1;
                    break;
                case CONTROLS.lblCommonPVal_Fact:
                    row = 0; col = 2;
                    break;
                case CONTROLS.lblCommonPVal_TM:
                    row = 0; col = 3;
                    break;
                case CONTROLS.lblAverP:
                    row = 4; col = 1;
                    break;
                case CONTROLS.lblAverPVal:
                    row = 4; col = 2;
                    break;
                case CONTROLS.lblPBRrec:
                    row = 8; col = 0;
                    break;
                case CONTROLS.lblPBRrecVal:
                    row = 8; col = 1;
                    break;
                case CONTROLS.lblCurrentE:
                    row = 0; col = 4;
                    break;
                case CONTROLS.lblCurrentEVal:
                    row = 0; col = 5;
                    break;
                case CONTROLS.lblHourE:
                    row = 4; col = 4;
                    break;
                case CONTROLS.lblHourEVal:
                    row = 4; col = 5;
                    break;
                case CONTROLS.lblDevE:
                    row = 8; col = 4;
                    break;
                case CONTROLS.lblDevEVal:
                    row = 8; col = 5;
                    break;
                default:
                    break;
            }

            if ((((ToolStripMenuItem)ContextMenuStrip.Items [1]).Checked == false) && (col > 2))
                col --;
            else
                ;

            return new TableLayoutPanelCellPosition(col, row);
        }

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            bool bChecked = true;
            if ((Users.Role == (int)Users.ID_ROLES.NSS) || (Users.Role == (int)Users.ID_ROLES.MAJOR_MASHINIST) || (Users.Role == (int)Users.ID_ROLES.MASHINIST)) bChecked = false; else ;
            this.ContextMenuStrip = new ContextMenuStrip ();
            this.ContextMenuStrip.Items.AddRange (new ToolStripMenuItem [] {
                new ToolStripMenuItem (@"Прогноз ЭЭ"),
                new ToolStripMenuItem (@"Знач. телеметрии") });
            this.ContextMenuStrip.Items[0].Enabled = false; ((ToolStripMenuItem)this.ContextMenuStrip.Items[0]).Checked = bChecked; this.ContextMenuStrip.Items[0].Click += OnVisibleForecast;
            this.ContextMenuStrip.Items[1].Enabled = false; ((ToolStripMenuItem)this.ContextMenuStrip.Items[1]).Checked = bChecked; this.ContextMenuStrip.Items[0].Click += OnVisibleTM;

            if (((ToolStripMenuItem)ContextMenuStrip.Items[0]).Checked == false) {
                COL_TG_START -= 2;
            }
            else
                ;
            
            if (((ToolStripMenuItem)ContextMenuStrip.Items[1]).Checked == false) {
                COUNT_LABEL --;
                COL_TG_START --;
            }
            else
                ;

            this.RowCount = COUNT_ROWS;

            for (int i = 0; i < this.RowCount + 1; i++)
                this.RowStyles.Add(new RowStyle(SizeType.Percent, (float)Math.Round((float)100 / this.RowCount, 1)));

            this.btnSetNow = new System.Windows.Forms.Button();
            this.dtprDate = new System.Windows.Forms.DateTimePicker();
            this.lblServerTime = new System.Windows.Forms.Label();
            this.lblPBRNumber = new System.Windows.Forms.Label();

            this.m_arLabelCommon = new System.Windows.Forms.Label[iCountLabels];

            //
            // btnSetNow
            //
            //this.btnSetNow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSetNow.Dock = DockStyle.Fill;
            //this.btnSetNow.Location = arPlacement[(int)CONTROLS.btnSetNow].pt;
            this.btnSetNow.Name = "btnSetNow";
            //this.btnSetNow.Size = arPlacement[(int)CONTROLS.btnSetNow].sz;
            this.btnSetNow.TabIndex = 2;
            this.btnSetNow.Text = "Текущий час";
            this.btnSetNow.UseVisualStyleBackColor = true;
            this.Controls.Add(this.btnSetNow, 0, 0);
            this.SetRowSpan(this.btnSetNow, 3);
            // 
            // dtprDate
            // 
            //this.dtprDate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.dtprDate.Dock = DockStyle.Fill;
            //this.dtprDate.Location = arPlacement[(int)CONTROLS.dtprDate].pt;
            this.dtprDate.Name = "dtprDate";
            //this.dtprDate.Size = arPlacement[(int)CONTROLS.dtprDate].sz;
            this.dtprDate.TabIndex = 4;
            this.Controls.Add(this.dtprDate, 0, 3);
            this.SetRowSpan(this.dtprDate, 3);
            // 
            // lblServerTime
            // 
            //this.lblServerTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblServerTime.Dock = DockStyle.Fill;
            this.lblServerTime.AutoSize = false;
            //this.lblServerTime.Location = arPlacement[(int)CONTROLS.lblServerTime].pt;
            this.lblServerTime.Name = "lblServerTime";
            //this.lblServerTime.Size = arPlacement[(int)CONTROLS.lblServerTime].sz;
            this.lblServerTime.TabIndex = 5;
            this.lblServerTime.Text = "--:--:--";
            this.lblServerTime.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblServerTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblServerTime.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(this.lblServerTime, 0, 6);
            this.SetRowSpan(this.lblServerTime, 3);
            // 
            // lblPBRNumber
            // 
            //this.lblPBRNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblPBRNumber.Dock = DockStyle.Fill;
            this.lblPBRNumber.AutoSize = false;
            //this.lblPBRNumber.Location = arPlacement[(int)CONTROLS.lblPBRNumber].pt;
            this.lblPBRNumber.Name = "lblPBRNumber";
            //this.lblPBRNumber.Size = arPlacement[(int)CONTROLS.lblPBRNumber].sz;
            this.lblPBRNumber.TabIndex = 5;
            this.lblPBRNumber.Text = "---";
            this.lblPBRNumber.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblPBRNumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblPBRNumber.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(this.lblPBRNumber, 0, 9);
            this.SetRowSpan(this.lblPBRNumber, 3);

            //Ширина столбца группы "Элементы управления"
            this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));

            Color foreColor, backClolor;
            float szFont;
            ContentAlignment align;
            //Size sz;
            string text = string.Empty;
            //int row = -1, col = -1;

            for (CONTROLS i = (CONTROLS)m_indxStartCommonPVal; i < CONTROLS.lblPBRrecVal + 1; i++)
            {
                //szFont = 6F;

                switch (i)
                {
                    case CONTROLS.lblCommonP:
                    case CONTROLS.lblPBRrec:
                    case CONTROLS.lblAverP:
                        foreColor = Color.Black;
                        backClolor = Color.Empty;
                        szFont = 8F;
                        align = ContentAlignment.MiddleLeft;
                        //sz = new Size(-1, -1);
                        //col = 1;
                        break;
                    case CONTROLS.lblCommonPVal_Fact:
                    case CONTROLS.lblAverPVal:
                        foreColor = Color.LimeGreen;
                        backClolor = Color.Black;
                        szFont = 15F;
                        align = ContentAlignment.MiddleCenter;
                        //sz = arPlacement[(int)i].sz;
                        //col = 2;
                        break;
                    case CONTROLS.lblPBRrecVal:
                        foreColor = Color.Yellow;
                        backClolor = Color.Black;
                        szFont = 15F;
                        align = ContentAlignment.MiddleCenter;
                        break;
                    case CONTROLS.lblCommonPVal_TM:
                        foreColor = Color.Green;
                        backClolor = Color.Black;
                        szFont = 15F;
                        align = ContentAlignment.MiddleCenter;
                        //sz = arPlacement[(int)i].sz;
                        //col = 3;
                        break;
                    default:
                        foreColor = Color.Yellow;
                        backClolor = Color.Red;
                        szFont = 6F;
                        align = ContentAlignment.MiddleCenter;
                        //sz = new Size(-1, -1);
                        break;
                }

                switch (i)
                {
                    case CONTROLS.lblCommonP:
                        text = @"Pтек"; //@"P тек";
                        break;
                    case CONTROLS.lblPBRrec:
                        text = @"Pрек";
                        break;
                    case CONTROLS.lblAverP:
                        text = @"Pср";
                        break;
                    default:
                        text = string.Empty;
                        break;
                }

                if (text.Equals(string.Empty) == false)
                {
                    m_arLabelCommon[(int)i - m_indxStartCommonPVal] = HLabel.createLabel(text,
                                                                                    new HLabelStyles(/*arPlacement[(int)i].pt, sz,*/new Point(-1, -1), new Size(-1, -1),
                                                                                    foreColor, backClolor,
                                                                                    szFont, align));
                }
                else
                {
                    m_arLabelCommon[(int)i - m_indxStartCommonPVal] = new HLabel(/*i.ToString(); @"---",*/
                                                                                    new HLabelStyles(/*arPlacement[(int)i].pt, sz,*/new Point(-1, -1), new Size(-1, -1),
                                                                                    foreColor, backClolor,
                                                                                    szFont, align));
                    m_arLabelCommon[(int)i - m_indxStartCommonPVal].Text = @"---";
                    ((HLabel)m_arLabelCommon[(int)i - m_indxStartCommonPVal]).m_type = HLabel.TYPE_HLABEL.TOTAL;
                }

                bool bAddItem = false;
                if (((ToolStripMenuItem)ContextMenuStrip.Items [1]).Checked == false) {
                    if (i == CONTROLS.lblCommonPVal_TM)
                        ; //continue;
                    else {
                        bAddItem = true;
                    }
                }
                else
                {
                    bAddItem = true;
                }

                if (bAddItem == true)
                {
                    //this.Controls.Add(m_arLabelCommon[(int)i - m_indxStartCommonPVal]);
                    this.Controls.Add(this.m_arLabelCommon[(int)i - m_indxStartCommonPVal]);
                    this.SetCellPosition(this.m_arLabelCommon[(int)i - m_indxStartCommonPVal], getPositionCell(i));
                    this.SetRowSpan(this.m_arLabelCommon[(int)i - m_indxStartCommonPVal], 4);
                }
                else
                    ;
            }

            //Ширина столбцов группы "Рекомендация"
            this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40F));
            this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88F));
            if (((ToolStripMenuItem)ContextMenuStrip.Items[1]).Checked == true)
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88F));
            else
                ;

            if (((ToolStripMenuItem)ContextMenuStrip.Items[0]).Checked == true)
            {
                for (CONTROLS i = (CONTROLS)m_indxStartCommonEVal; i < CONTROLS.lblDevEVal + 1; i++)
                {
                    switch (i)
                    {
                        case CONTROLS.lblCurrentE:
                        case CONTROLS.lblHourE:
                        case CONTROLS.lblDevE:
                            foreColor = Color.Black;
                            backClolor = Color.Empty;
                            szFont = 8F;
                            align = ContentAlignment.MiddleRight;
                            //sz = new Size(-1, -1);
                            //col = 4;
                            break;
                        case CONTROLS.lblCurrentEVal:
                        case CONTROLS.lblHourEVal:
                        case CONTROLS.lblDevEVal:
                            foreColor = Color.LimeGreen;
                            backClolor = Color.Black;
                            szFont = 15F;
                            align = ContentAlignment.MiddleCenter;
                            //sz = arPlacement[(int)i].sz;
                            //col = 5;
                            break;
                        default:
                            foreColor = Color.Red;
                            backClolor = Color.Yellow;
                            szFont = 6F;
                            align = ContentAlignment.MiddleCenter;
                            //sz = new Size(-1, -1);
                            break;
                    }

                    switch (i)
                    {
                        case CONTROLS.lblCurrentE:
                            text = @"Етек"; //@"P тек";
                            break;
                        case CONTROLS.lblHourE:
                            text = @"Ечас";
                            break;
                        case CONTROLS.lblDevE:
                            text = @"Откл";
                            break;
                        default:
                            text = string.Empty;
                            //text = @"---";
                            break;
                    }

                    if (text.Equals(string.Empty) == false)
                    {
                        m_arLabelCommon[(int)i - m_indxStartCommonPVal] = HLabel.createLabel(text,
                                                                                            new HLabelStyles(/*arPlacement[(int)i].pt, sz,*/new Point(-1, -1), new Size(-1, -1),
                                                                                            foreColor, backClolor,
                                                                                            szFont, align));
                    }
                    else
                    {
                        m_arLabelCommon[(int)i - m_indxStartCommonPVal] = new HLabel(/*i.ToString(); @"---",*/
                                                                                        new HLabelStyles(/*arPlacement[(int)i].pt, sz,*/new Point(-1, -1), new Size(-1, -1),
                                                                                        foreColor, backClolor,
                                                                                        szFont, align));
                        m_arLabelCommon[(int)i - m_indxStartCommonPVal].Text = @"---";
                        ((HLabel)m_arLabelCommon[(int)i - m_indxStartCommonPVal]).m_type = HLabel.TYPE_HLABEL.TOTAL;
                    }

                    //this.Controls.Add(m_arLabelCommon[(int)i - m_indxStartCommonPVal]);
                    this.Controls.Add(this.m_arLabelCommon[(int)i - m_indxStartCommonPVal]);
                    this.SetCellPosition(this.m_arLabelCommon[(int)i - m_indxStartCommonPVal], getPositionCell(i));
                    this.SetRowSpan(this.m_arLabelCommon[(int)i - m_indxStartCommonPVal], 4);
                }

                //Ширина столбцов группы "Отклонение"
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40F));
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88F));
            }
            else
                ;

            //OnSizeChanged(this, EventArgs.Empty);
        }

        #endregion

        //PanelTecViewBase m_parent;
        PanelTecViewBase m_parent { get { return (PanelTecViewBase)Parent; } }

        public enum CONTROLS : uint
        {
            lblCommonP, lblCommonPVal_Fact,
            lblCommonPVal_TM,
            lblAverP, lblAverPVal,
            lblPBRrec, lblPBRrecVal,
            lblCurrentE, lblCurrentEVal,
            lblHourE, lblHourEVal,
            lblDevE, lblDevEVal,
            dtprDate,
            lblServerTime,
            btnSetNow,
            lblPBRNumber,
            COUNT_CONTROLS
        };

        //const int delimXCommonVal = 115, delimXCommonPair = 35,
        //    widthPanelDateTime = 85,
        //    widthLabelName = 30;
        //HPlacement[] arPlacement =
        //        {   new HPlacement (112, 6, widthLabelName, 13), new HPlacement (143, 0, 79, 27), //lblCommonP, lblCommonPVal
        //                                                        new HPlacement (224, 0, 79, 27),
        //            new HPlacement (112, 36, widthLabelName, 13), new HPlacement (143, 30, 79, 27), //lblAverP, lblAverPVal
        //            new HPlacement (112, 66, widthLabelName, 13), new HPlacement (143, 60, 79, 27),
        //            new HPlacement (330, 6, widthLabelName, 13), new HPlacement (360, 0, 79, 27), //lblCurrentE, lblCurrentEVal
        //            new HPlacement (330, 36, widthLabelName, 13), new HPlacement (360, 30, 79, 27), //lblDevE, lblDevEVal
        //            new HPlacement (330, 66, widthLabelName, 13), new HPlacement (360, 60, 79, 27),
        //            new HPlacement (3, 3, widthPanelDateTime, 20), //dtprDate
        //            new HPlacement (/*0, 1, 67, 20*/3, 26, widthPanelDateTime, 20), //lblServerTime
        //            new HPlacement (/*6, 643, 93, 23*/3, 49, widthPanelDateTime, 23), //btnSetNow
        //            new HPlacement (/*0, 22, 67, 20*/3, 75, widthPanelDateTime, 20) //lblPBRNumber
        //        };

        public const int m_indxStartCommonPVal = (int)CONTROLS.lblCommonP,
                    m_indxStartCommonEVal = (int)CONTROLS.lblCurrentE;
        private const int iCountLabels = (int)CONTROLS.lblDevEVal - (int)CONTROLS.lblCommonP + 1;

        public System.Windows.Forms.Label[] m_arLabelCommon;

        //private List<System.Windows.Forms.Label> tgsName;
        private List<System.Windows.Forms.Label[]> m_tgsValues = new List<System.Windows.Forms.Label[]>();

        public System.Windows.Forms.Button btnSetNow;
        public DateTimePicker dtprDate;
        private HLabel m_lblPowerFactZoom;
        public System.Windows.Forms.Label lblServerTime;
        private System.Windows.Forms.Label lblPBRNumber;

        int COUNT_LABEL = 3 
                , COUNT_TG_IN_COLUMN = 4
                , COL_TG_START = 6
                , COUNT_ROWS = 12;

        public void Initialize()
        {
            int cnt = ((m_tgsValues.Count / COUNT_TG_IN_COLUMN) + ((m_tgsValues.Count % COUNT_TG_IN_COLUMN == 0) ? 0 : 1));
            bool bPowerFactZoom = false;

            for (int i = 0; i < cnt; i++)
            {
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40F));
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 75));
                if (((ToolStripMenuItem)ContextMenuStrip.Items [1]).Checked == true)
                    this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 75));
                else
                    ;
            }

            m_lblPowerFactZoom = new HLabel(new Point(-1, -1), new Size(-1, -1), Color.LimeGreen, SystemColors.Control, 12F, ContentAlignment.MiddleCenter);
            m_lblPowerFactZoom.m_type = HLabel.TYPE_HLABEL.TOTAL_ZOOM;
            m_lblPowerFactZoom.Text = @"Pтек=----.--";

            if ((Users.Role == (int)Users.ID_ROLES.NSS) || (Users.Role == (int)Users.ID_ROLES.MAJOR_MASHINIST) || (Users.Role == (int)Users.ID_ROLES.MASHINIST)) {
                this.Controls.Add(m_lblPowerFactZoom, COL_TG_START + cnt * COUNT_LABEL + 0, 0);
                this.SetRowSpan(m_lblPowerFactZoom, COUNT_ROWS);
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88*3));

                bPowerFactZoom = true;
            }
            else
                ;

            Panel panelEmpty = new Panel();
            this.Controls.Add(panelEmpty, COL_TG_START + cnt * COUNT_LABEL + (bPowerFactZoom == true ? 1 : 0), 0);
            this.SetRowSpan(panelEmpty, COUNT_ROWS);
            this.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        }

        //public void addTGView(ref string name_shr, /*ref float val,*/ ref int positionXName, ref int positionYName, ref int positionXValue, ref int positionYValue)
        public void addTGView(ref string name_shr)
        {
            int cnt = -1;
            m_tgsValues.Add(new Label[(int)TG.INDEX_VALUE.COUNT_INDEX_VALUE]);
            cnt = m_tgsValues.Count;

            System.Windows.Forms.Label lblName = HLabel.createLabel(name_shr,
                                                                    new HLabelStyles(/*arPlacement[(int)i].pt, sz,*/new Point(-1, -1), new Size(-1, -1),
                                                                    Color.Black, Color.Empty,
                                                                    8F, ContentAlignment.MiddleRight))
                //, lblValue = null
                ;
            HLabel hlblValue;

            //lblValue = new System.Windows.Forms.Label();
            //createTGLabelValue(ref lblValue, name_shr + TG.INDEX_VALUE.FACT.ToString(), val, System.Drawing.Color.LimeGreen, positionXValue, positionYValue);
            //lblValue.TextAlign = ContentAlignment.MiddleCenter;
            //lblValue = HLabel.createLabel(name_shr + "_Fact", new HLabelStyles(new Point(positionXValue, positionYValue), new Size(63, 27), Color.LimeGreen, Color.Black, 15F, ContentAlignment.MiddleCenter));
            hlblValue = new HLabel(new HLabelStyles(new Point(-1, -1), new Size(-1, -1), Color.LimeGreen, Color.Black, 13F, ContentAlignment.MiddleCenter));
            hlblValue.Text = @"---.--"; //name_shr + @"_Fact";
            hlblValue.m_type = HLabel.TYPE_HLABEL.TG;
            m_tgsValues[m_tgsValues.Count - 1][(int)TG.INDEX_VALUE.FACT] = (Label)hlblValue;

            //positionYValue += 29;

            //lblValue = new System.Windows.Forms.Label();
            //createTGLabelValue(ref lblValue, name_shr + TG.INDEX_VALUE.TM.ToString(), val, Color.Green, positionXValue, positionYValue);
            //lblValue.TextAlign = ContentAlignment.MiddleCenter;
            //lblValue = HLabel.createLabel(name_shr + "_TM", new HLabelStyles(new Point(positionXValue, positionYValue), new Size(63, 27), Color.Green, Color.Black, 15F, ContentAlignment.MiddleCenter));
            hlblValue = new HLabel(new HLabelStyles(new Point(-1, -1), new Size(-1, -1), Color.Green, Color.Black, 13F, ContentAlignment.MiddleCenter));
            hlblValue.Text = @"---.--"; //name_shr + @"_TM";
            hlblValue.m_type = HLabel.TYPE_HLABEL.TG;
            m_tgsValues[m_tgsValues.Count - 1][(int)TG.INDEX_VALUE.TM] = (Label)hlblValue;

            //positionXName += 69; positionXValue += 69;

            this.Controls.Add(lblName);
            this.SetCellPosition(lblName, new TableLayoutPanelCellPosition((cnt - 1) / COUNT_TG_IN_COLUMN * COUNT_LABEL + (COL_TG_START + 0), (cnt - 1) % COUNT_TG_IN_COLUMN * (COUNT_ROWS / COUNT_TG_IN_COLUMN)));
            this.SetRowSpan(lblName, (COUNT_ROWS / COUNT_TG_IN_COLUMN));

            this.Controls.Add(m_tgsValues[cnt - 1][(int)TG.INDEX_VALUE.FACT]);
            this.SetCellPosition(m_tgsValues[cnt - 1][(int)TG.INDEX_VALUE.FACT], new TableLayoutPanelCellPosition((cnt - 1) / COUNT_TG_IN_COLUMN * COUNT_LABEL + (COL_TG_START + 1), (cnt - 1) % COUNT_TG_IN_COLUMN * (COUNT_ROWS / COUNT_TG_IN_COLUMN)));
            this.SetRowSpan(m_tgsValues[cnt - 1][(int)TG.INDEX_VALUE.FACT], (COUNT_ROWS / COUNT_TG_IN_COLUMN));

            if (((ToolStripMenuItem)ContextMenuStrip.Items [1]).Checked == true)
            {
                this.Controls.Add(m_tgsValues[cnt - 1][(int)TG.INDEX_VALUE.TM]);
                this.SetCellPosition(m_tgsValues[cnt - 1][(int)TG.INDEX_VALUE.TM], new TableLayoutPanelCellPosition((cnt - 1) / COUNT_TG_IN_COLUMN * COUNT_LABEL + (COL_TG_START + 2), (cnt - 1) % COUNT_TG_IN_COLUMN * (COUNT_ROWS / COUNT_TG_IN_COLUMN)));
                this.SetRowSpan(m_tgsValues[cnt - 1][(int)TG.INDEX_VALUE.TM], (COUNT_ROWS / COUNT_TG_IN_COLUMN));
            }
            else
                ;
        }
    }

    public partial class PanelQuickData : HPanelTableLayout
    {
        /// <summary>
        /// Класс для хранения информации о местоположении элемента управления
        /// </summary>
        private class HPlacement
        {
            public Size sz; public Point pt;
            public HPlacement(int x, int y, int w, int h)
            {
                pt.X = x; pt.Y = y; sz.Width = w; sz.Height = h;
            }
        };

        public PanelQuickData()
        {
            InitializeComponent();
        }

        public PanelQuickData(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        /// <summary>
        /// Отобразить текущие значения
        /// </summary>
        public void ShowTMValues()
        {
            double value_TM = 0.0;
            int i = 0;

            if (!(m_parent == null))
                if (m_parent.indx_TECComponent < 0) // значит этот view будет суммарным для всех ГТП
                {
                    foreach (TECComponent g in m_parent.m_tecView.m_tec.list_TECComponents)
                    {
                        if (g.m_id < 500)
                            //Только ГТП
                            foreach (TG tg in g.m_listTG)
                            {
                                if (tg.id_tm > 0)
                                {
                                    if (tg.power_TM > 1)
                                    {
                                        m_tgsValues[i][(int)TG.INDEX_VALUE.TM].Text = tg.power_TM.ToString("F2");
                                        value_TM += tg.power_TM;
                                    }
                                    else
                                        m_tgsValues[i][(int)TG.INDEX_VALUE.TM].Text = 0.ToString("F0");

                                }
                                else
                                {
                                    m_tgsValues[i][(int)TG.INDEX_VALUE.TM].Text = "---";
                                }
                                i++;
                            }
                        else
                            ;
                    }
                }
                else
                {
                    foreach (TG tg in m_parent.m_tecView.m_tec.list_TECComponents[m_parent.indx_TECComponent].m_listTG)
                    {
                        if (tg.id_tm > 0)
                        {
                            if (tg.power_TM > 1)
                            {
                                m_tgsValues[i][(int)TG.INDEX_VALUE.TM].Text = tg.power_TM.ToString("F2");
                                value_TM += tg.power_TM;
                            }
                            else
                                m_tgsValues[i][(int)TG.INDEX_VALUE.TM].Text = 0.ToString("F0");
                        }
                        else
                        {
                            m_tgsValues[i][(int)TG.INDEX_VALUE.TM].Text = "---";
                        }
                        i++;
                    }
                }
            else
                ;

            if (value_TM < 1)
                value_TM = 0.0;
            else
                ;

            showValue(ref m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblCommonPVal_TM - m_indxStartCommonPVal], value_TM, true, string.Empty);
        }

        /// <summary>
        /// Отобразить значение для элемента управления с условиями
        /// </summary>
        /// <param name="lbl">элемент управления, как ссылка</param>
        /// <param name="val">значение для отображения</param>
        private void showValue(ref System.Windows.Forms.Label lbl, double val, bool bPower, string adding)
        {
            if (! (lbl == null))
                if (val == double.NegativeInfinity)
                    lbl.Text = adding;
                else
                    if (bPower == true)
                        if (val > 1)
                            lbl.Text = val.ToString("F2") + adding;
                        else
                            lbl.Text = 0.ToString("F0");
                    else
                        lbl.Text = val.ToString("F2") + adding;
            else
                ;
        }

        /// <summary>
        /// Отобразить значение для элемента управления с условиями
        /// </summary>
        /// <param name="lbl">элемент управления</param>
        /// <param name="val">значение для отображения</param>
        private void showValue(System.Windows.Forms.Label lbl, double val)
        {
            if (val > 1)
                lbl.Text = val.ToString("F2");
            else
                lbl.Text = 0.ToString("F0");
        }

        /// <summary>
        /// Отобразить фактические значение
        /// </summary>
        public void ShowFactValues()
        {
            if (! (m_parent == null)) {
                int indxStartCommonPVal = PanelQuickData.m_indxStartCommonPVal;
                int i = -1, j = -1,
                    min = m_parent.m_tecView.lastMin;

                if (!(min == 0)) min--; else ;

                double valueEBefore = 0.0,
                        valueECur = 0.0,
                        valueEFuture = 0.0;
                for (i = 0; i < m_parent.m_tecView.listTG.Count; i++)
                    for (j = 0; j < min; j++)
                        valueEBefore += m_parent.m_tecView.listTG[i].power[j] / 20;

                double value = 0;
                for (i = 0; i < m_parent.m_tecView.listTG.Count; i++)
                    if (m_parent.m_tecView.listTG[i].power[min] > 1) value += m_parent.m_tecView.listTG[i].power[min]; else ;

                showValue(ref m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblCommonPVal_Fact - indxStartCommonPVal], value, true, string.Empty);
                m_lblPowerFactZoom.Text = @"Pтек=" + value.ToString (@"F2");

                valueECur = value / 20;
                showValue(ref m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblCurrentEVal - indxStartCommonPVal], valueECur, true, string.Empty);

                valueEFuture = valueECur * (20 - min - 0);
                showValue(ref m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblHourEVal - indxStartCommonPVal], valueEBefore + valueECur + valueEFuture, true, string.Empty);

                if ((m_parent.m_tecView.adminValuesReceived == true) && (m_parent.m_tecView.currHour == true))
                {
                    showValue(ref m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblPBRrecVal - indxStartCommonPVal], m_parent.m_tecView.recomendation, true, string.Empty);
                }
                else
                {
                    m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblPBRrecVal - indxStartCommonPVal].Text = "---";
                }

                double summ = 0;
                //Для возможности восстановления значения
                bool bPrevRecalcAver = m_parent.m_tecView.recalcAver;

                if ((m_parent.m_tecView.currHour == true) && (min == 0))
                    m_parent.m_tecView.recalcAver = false;
                else
                    ;

                if (m_parent.m_tecView.recalcAver == true)
                {
                    if (m_parent.m_tecView.currHour == true)
                    {
                        for (i = 1; i < m_parent.m_tecView.lastMin; i++)
                            summ += m_parent.m_tecView.m_valuesMins.valuesFact[i];
                        if (!(min == 0))
                            showValue(ref m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblAverPVal - indxStartCommonPVal], summ / min, true, string.Empty);
                        else
                            m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblAverPVal - indxStartCommonPVal].Text = 0.ToString("F0");
                    }
                    else
                    {
                        int hour = m_parent.m_tecView.lastHour;
                        if (hour == 24)
                            hour = 23;
                        else
                            ;

                        if ((m_parent.m_tecView.m_valuesHours.addonValues == true) && (hour == m_parent.m_tecView.m_valuesHours.hourAddon))
                            summ = m_parent.m_tecView.m_valuesHours.valuesFactAddon;
                        else
                            summ = m_parent.m_tecView.m_valuesHours.valuesFact[hour];

                        showValue(ref m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblAverPVal - indxStartCommonPVal], summ, true, string.Empty);
                    }

                    //if (! ([lastHour] == 0))
                    if ((m_parent.m_tecView.lastHour < m_parent.m_tecView.m_valuesHours.valuesUDGe.Length) &&
                        (!(m_parent.m_tecView.m_valuesHours.valuesUDGe[m_parent.m_tecView.lastHour] == 0)))
                    {
                        showValue(ref m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblDevEVal - indxStartCommonPVal], ((((valueEBefore + valueECur + valueEFuture) -
                                                    m_parent.m_tecView.m_valuesHours.valuesUDGe[m_parent.m_tecView.lastHour]) / m_parent.m_tecView.m_valuesHours.valuesUDGe[m_parent.m_tecView.lastHour]) * 100), false, @"%");
                        //m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblDevEVal - indxStartCommonPVal].Text = .ToString("F2") + "%";
                    }
                    else
                        showValue(ref m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblDevEVal - indxStartCommonPVal], double.NegativeInfinity, false, @"---");
                }

                if ((m_parent.m_tecView.currHour == true) && (min == 0))
                    m_parent.m_tecView.recalcAver = bPrevRecalcAver;
                else
                    ;

                if (m_parent.m_tecView.currHour == true)
                {
                    if (m_parent.m_tecView.lastHourError == true)
                    {
                        //m_parent.ErrorReport("По текущему часу значений не найдено!");
                        m_parent.m_tecView.ErrorReport("По текущему часу значений не найдено!");
                    }
                    else
                    {
                        if (m_parent.m_tecView.lastHourHalfError == true)
                        {
                            m_parent.m_tecView.ErrorReport("За текущий час не получены некоторые получасовые значения!");
                        }
                        else
                        {
                            if (m_parent.m_tecView.lastMinError == true)
                            {
                                m_parent.m_tecView.ErrorReport("По текущему трёхминутному отрезку значений не найдено!");
                                m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblAverPVal - indxStartCommonPVal].ForeColor =
                                m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblCommonPVal_Fact - indxStartCommonPVal].ForeColor =
                                m_lblPowerFactZoom.ForeColor = System.Drawing.Color.OrangeRed;
                            }
                            else
                            {
                                m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblAverPVal - indxStartCommonPVal].ForeColor =
                                m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblCommonPVal_Fact - indxStartCommonPVal].ForeColor =
                                m_lblPowerFactZoom.ForeColor = System.Drawing.Color.LimeGreen;

                                if (! (m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblCurrentEVal - indxStartCommonPVal] == null)) m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblCurrentEVal - indxStartCommonPVal].ForeColor = System.Drawing.Color.LimeGreen; else ;
                                if (!(m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblHourEVal - indxStartCommonPVal] == null)) m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblHourEVal - indxStartCommonPVal].ForeColor = System.Drawing.Color.Yellow; else ;
                            }
                        }
                    }
                }
                else
                {
                    m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblAverPVal - indxStartCommonPVal].ForeColor =
                    m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblCommonPVal_Fact - indxStartCommonPVal].ForeColor =
                    m_lblPowerFactZoom.ForeColor = System.Drawing.Color.OrangeRed;

                    if ((! (m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblCurrentEVal - indxStartCommonPVal] == null)) && (! (m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblHourEVal - indxStartCommonPVal] == null)))
                        m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblCurrentEVal - indxStartCommonPVal].ForeColor =
                        m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblHourEVal - indxStartCommonPVal].ForeColor = System.Drawing.Color.OrangeRed;
                    else
                        ;
                }

                lblPBRNumber.Text = m_parent.m_tecView.lastLayout;

                //ShowTGValue
                i = 0;
                if (m_parent.indx_TECComponent < 0) // значит этот view будет суммарным для всех ГТП
                {
                    foreach (TECComponent g in m_parent.m_tecView.m_localTECComponents)
                    {
                        if (g.m_id < 500)
                            //Только ГТП
                            foreach (TG tg in g.m_listTG)
                            {
                                if (tg.receivedMin[min] == true)
                                {
                                    showValue(m_tgsValues[i][(int)TG.INDEX_VALUE.FACT], tg.power[min]);
                                    if (m_parent.m_tecView.currHour == true)
                                        m_tgsValues[i][(int)TG.INDEX_VALUE.FACT].ForeColor = System.Drawing.Color.LimeGreen;
                                    else
                                        m_tgsValues[i][(int)TG.INDEX_VALUE.FACT].ForeColor = System.Drawing.Color.OrangeRed;
                                }
                                else
                                {
                                    m_tgsValues[i][(int)TG.INDEX_VALUE.FACT].Text = "---";
                                    m_tgsValues[i][(int)TG.INDEX_VALUE.FACT].ForeColor = System.Drawing.Color.OrangeRed;
                                }
                                i++;
                            }
                        else
                            ;
                    }
                }
                else
                {
                    foreach (TECComponent comp in m_parent.m_tecView.m_localTECComponents)
                    {
                        if (comp.m_listTG [0].receivedMin[min] == true)
                        {
                            showValue(m_tgsValues[i][(int)TG.INDEX_VALUE.FACT], comp.m_listTG[0].power[min]);
                            if (m_parent.m_tecView.currHour == true)
                                m_tgsValues[i][(int)TG.INDEX_VALUE.FACT].ForeColor = System.Drawing.Color.LimeGreen;
                            else
                                m_tgsValues[i][(int)TG.INDEX_VALUE.FACT].ForeColor = System.Drawing.Color.OrangeRed;
                        }
                        else
                        {
                            m_tgsValues[i][(int)TG.INDEX_VALUE.FACT].Text = "---";
                            m_tgsValues[i][(int)TG.INDEX_VALUE.FACT].ForeColor = System.Drawing.Color.OrangeRed;
                        }
                        i++;
                    }
                }

                //Logging.Logg().LogDebugToFile(@"PanelQuickData::ShowFactValues () - вЫход...");
            }
            else
                ;
        }

        private void OnVisibleForecast (object obj, EventArgs ev) {
        }

        private void OnVisibleTM(object obj, EventArgs ev)
        {
        }
    }
}
