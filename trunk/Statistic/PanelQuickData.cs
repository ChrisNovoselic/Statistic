using System;
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

using HClassLibrary;
using StatisticCommon;

namespace Statistic
{
    partial class PanelTecViewBase
    {
        protected partial class HPanelTableLayout : TableLayoutPanel
        {
            public HPanelTableLayout()
            {
                Thread.CurrentThread.CurrentCulture =
                Thread.CurrentThread.CurrentUICulture =
                    ProgramBase.ss_MainCultureInfo;

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
                string strValue = string.Empty;
                SizeF[] szLabelOfMinSizes = new SizeF[(int)HLabel.TYPE_HLABEL.COUNT_TYPE_HLABEL]; //(float.MaxValue, float.MaxValue);

                Font[] arFontRes = null;

                Graphics g = this.CreateGraphics();

                int indx = -1, i = -1;

                for (i = (int)HLabel.TYPE_HLABEL.TG; i < (int)HLabel.TYPE_HLABEL.COUNT_TYPE_HLABEL; i++)
                {
                    textOfMaxLengths[i] = string.Empty;
                    szLabelOfMinSizes[i].Height =
                    szLabelOfMinSizes[i].Width =
                        float.MaxValue;
                }

                foreach (Control ctrl in this.Controls)
                {
                    if (ctrl is HClassLibrary.HLabel)
                    {
                        indx = (int)((HLabel)ctrl).m_type;
                        if (!(indx == (int)HLabel.TYPE_HLABEL.UNKNOWN))
                        {
                            if (textOfMaxLengths[indx].Length < ctrl.Text.Length)
                            {
                                switch (indx)
                                {
                                    case (int)HLabel.TYPE_HLABEL.TG:
                                        strValue = ctrl.Text;
                                        break;
                                    case (int)HLabel.TYPE_HLABEL.TOTAL:
                                        if (ctrl.Text.LongCount(delegate(char ch) { return ch == '-'; }) > 1)
                                            strValue = new string('8', ctrl.Text.Length + 3);
                                        else
                                            strValue = ctrl.Text;
                                        break;
                                    case (int)HLabel.TYPE_HLABEL.TOTAL_ZOOM:
                                        strValue = ctrl.Text;
                                        break;
                                    default:
                                        break;
                                }

                                if (textOfMaxLengths[indx].Length < strValue.Length)
                                    textOfMaxLengths[indx] = strValue;
                                else
                                    ;
                            }
                            else
                                ;
                            if ((szLabelOfMinSizes[indx].Height > ctrl.Height) || (szLabelOfMinSizes[indx].Width > ctrl.Width)) { szLabelOfMinSizes[indx].Height = ctrl.Height; szLabelOfMinSizes[indx].Width = ctrl.Width; } else ;
                        }
                        else
                        {
                            Logging.Logg().Error(@"HPanelTableLayout::GetFontHLabel () - type=UNKNOWN, ctrl.Text=" + ctrl.Text, Logging.INDEX_MESSAGE.NOT_SET);

                            break;
                        }
                    }
                    else
                    {
                    }
                }

                if (!(indx == (int)HLabel.TYPE_HLABEL.UNKNOWN))
                {
                    arFontRes = new Font[(int)HLabel.TYPE_HLABEL.COUNT_TYPE_HLABEL];

                    for (i = (int)HLabel.TYPE_HLABEL.TG; i < (int)HLabel.TYPE_HLABEL.COUNT_TYPE_HLABEL; i++)
                        if ((szLabelOfMinSizes[i].Height < float.MaxValue) && (szLabelOfMinSizes[i].Width < float.MaxValue))
                        {
                            arFontRes[i] = HLabel.FitFont(g, textOfMaxLengths[i], szLabelOfMinSizes[i], new SizeF(0.85F, 0.85F), 0.1F);
                        }
                        else
                            ;
                }
                else
                    ; //Logging.Logg ().Error (@"HPanelTableLayout::GetFontHLabel () - type=UNKNOWN");

                return arFontRes;
            }

            /// <summary>
            /// Обработчик события изменеия размера панели
            /// </summary>
            /// <param name="obj">Объект, инициировавший событие</param>
            /// <param name="ev">Аргумент события</param>
            public void OnSizeChanged(object obj, EventArgs ev)
            {
                Font[] fonts = GetFontHLabel();

                if (obj == null)
                    obj = this;
                else
                    ;

                if (!(fonts == null))
                    foreach (Control ctrl in ((TableLayoutPanel)obj).Controls)
                        if (ctrl is HClassLibrary.HLabel)
                            if (!(fonts[(int)((HLabel)ctrl).m_type] == null))
                                ctrl.Font = fonts[(int)((HLabel)ctrl).m_type];
                            else
                                Logging.Logg().Error(@"HPanelTableLayout::OnSizeChanged () - fonts[" + ((HLabel)ctrl).m_type.ToString() + @"]=null", Logging.INDEX_MESSAGE.NOT_SET);
                        else
                            ;
                else
                    Logging.Logg().Error(@"HPanelTableLayout::OnSizeChanged () - fonts=null", Logging.INDEX_MESSAGE.NOT_SET);
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

        protected abstract class HPanelQuickData : HPanelTableLayout
        {
            protected int COUNT_LABEL
                    , COUNT_TG_IN_COLUMN
                    , COL_TG_START
                    , COUNT_ROWS = -1
                    , COUNT_ROW_LABELCOMMON;
            protected float SZ_COLUMN_LABEL, SZ_COLUMN_LABEL_VALUE
                , SZ_COLUMN_TG_LABEL, SZ_COLUMN_TG_LABEL_VALUE;

            public System.Windows.Forms.Button btnSetNow;
            public DateTimePicker dtprDate;
            public System.Windows.Forms.Label lblServerTime;

            protected Panel m_panelEmpty;

            public abstract void RestructControl();

            protected abstract TableLayoutPanelCellPosition getPositionCell(int indx);

            public abstract void ShowFactValues();            
            public abstract void ShowTMValues();

            protected virtual PanelTecViewBase m_parent { get { return (PanelTecViewBase)Parent; } }

            public /*static*/ int /*s*/m_indxStartCommonFirstValueSeries
                , /*s*/m_indxStartCommonSecondValueSeries;
            protected int m_iCountCommonLabels;

            public System.Windows.Forms.Label[] m_arLabelCommon;

            protected Dictionary<int, System.Windows.Forms.Label[]> m_tgLabels;
            protected Dictionary<int, System.Windows.Forms.ToolTip[]> m_tgToolTips;

            public HPanelQuickData ()
            {
                InitializeComponent ();
            }

            private void InitializeComponent ()
            {
                /*SZ_COLUMN_LABEL = 48F;*/ SZ_COLUMN_LABEL_VALUE = 88F;
                SZ_COLUMN_TG_LABEL = 40F; SZ_COLUMN_TG_LABEL_VALUE = 75F;
                
                this.btnSetNow = new System.Windows.Forms.Button();
                this.dtprDate = new System.Windows.Forms.DateTimePicker();
                this.lblServerTime = new System.Windows.Forms.Label();

                m_panelEmpty = new Panel();

                this.SuspendLayout();
                
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

                // 
                // dtprDate
                // 
                //this.dtprDate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
                this.dtprDate.Dock = DockStyle.Fill;
                //this.dtprDate.Location = arPlacement[(int)CONTROLS.dtprDate].pt;
                this.dtprDate.Name = "dtprDate";
                //this.dtprDate.Size = arPlacement[(int)CONTROLS.dtprDate].sz;
                this.dtprDate.TabIndex = 4;

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

                this.ResumeLayout(false);
                //this.PerformLayout();
            }

            //public void addTGView(ref string name_shr, /*ref float val,*/ ref int positionXName, ref int positionYName, ref int positionXValue, ref int positionYValue)
            public void AddTGView(TG tg)
            {
                int cnt = -1;
                m_tgLabels.Add(tg.m_id, new Label[(int)TG.INDEX_VALUE.COUNT_INDEX_VALUE]);
                cnt = m_tgLabels.Count;

                m_tgLabels[tg.m_id][(int)TG.INDEX_VALUE.LABEL_DESC] = HLabel.createLabel(tg.name_shr,
                                                                        new HLabelStyles(/*arPlacement[(int)i].pt, sz,*/new Point(-1, -1), new Size(-1, -1),
                                                                        Color.Black, Color.Empty,
                                                                        8F, ContentAlignment.MiddleRight))
                    //, lblValue = null
                    ;
                HLabel hlblValue;

                hlblValue = new HLabel(new HLabelStyles(new Point(-1, -1), new Size(-1, -1), Color.LimeGreen, Color.Black, 13F, ContentAlignment.MiddleCenter));
                hlblValue.Text = @"---.--"; //name_shr + @"_Fact";
                hlblValue.m_type = HLabel.TYPE_HLABEL.TG;
                //m_tgToolTips[tg.m_id][(int)TG.INDEX_VALUE.FACT].SetToolTip(hlblValue, tg.name_shr + @"[" + tg.m_SensorsStrings_ASKUE[0] + @"]: " + (tg.m_TurnOnOff == TG.INDEX_TURNOnOff.ON ? @"вкл." : @"выкл."));
                m_tgLabels[tg.m_id][(int)TG.INDEX_VALUE.FACT] = (Label)hlblValue;

                hlblValue = new HLabel(new HLabelStyles(new Point(-1, -1), new Size(-1, -1), Color.Green, Color.Black, 13F, ContentAlignment.MiddleCenter));
                hlblValue.Text = @"---.--"; //name_shr + @"_TM";
                hlblValue.m_type = HLabel.TYPE_HLABEL.TG;
                m_tgLabels[tg.m_id][(int)TG.INDEX_VALUE.TM] = (Label)hlblValue;
            }

            protected void createLabel(int indx, string strLabelText, Color clrLabelFore, Color clrLabelBackground, float fSzLabelFont, ContentAlignment alignLabel)
            {
                if (strLabelText.Equals(string.Empty) == false)
                    if (m_arLabelCommon[indx - m_indxStartCommonFirstValueSeries] == null)
                        m_arLabelCommon[indx - m_indxStartCommonFirstValueSeries] = HLabel.createLabel(strLabelText,
                                                                                        new HLabelStyles(/*arPlacement[(int)i].pt, sz,*/new Point(-1, -1), new Size(-1, -1),
                                                                                        clrLabelFore, clrLabelBackground,
                                                                                        fSzLabelFont, alignLabel));
                    else ;
                else
                    if (m_arLabelCommon[indx - m_indxStartCommonFirstValueSeries] == null)
                    {
                        m_arLabelCommon[indx - m_indxStartCommonFirstValueSeries] = new HLabel(/*i.ToString(); @"---",*/
                                                                                        new HLabelStyles(/*arPlacement[(int)i].pt, sz,*/new Point(-1, -1), new Size(-1, -1),
                                                                                        clrLabelFore, clrLabelBackground,
                                                                                        fSzLabelFont, alignLabel));
                        m_arLabelCommon[indx - m_indxStartCommonFirstValueSeries].Text = @"---";
                        ((HLabel)m_arLabelCommon[indx - m_indxStartCommonFirstValueSeries]).m_type = HLabel.TYPE_HLABEL.TOTAL;
                    }
                    else ;
            }

            protected void removeFirstCommonLabels(int limit)
            {
                for (int i = m_indxStartCommonFirstValueSeries; i < limit + 1; i++)
                {
                    if (!(this.m_arLabelCommon[(int)i - m_indxStartCommonFirstValueSeries] == null))
                        if (!(this.Controls.IndexOf(this.m_arLabelCommon[(int)i - m_indxStartCommonFirstValueSeries]) < 0))
                            this.Controls.Remove(this.m_arLabelCommon[(int)i - m_indxStartCommonFirstValueSeries]);
                        else ;
                    else
                        ;
                }
            }

            protected void removeSecondCommonLabels (int limit)
            {
                for (int i = m_indxStartCommonSecondValueSeries; i < limit + 1; i++)
                {
                    if (!(this.m_arLabelCommon[(int)i - m_indxStartCommonFirstValueSeries] == null))
                        if (!(this.Controls.IndexOf(this.m_arLabelCommon[(int)i - m_indxStartCommonFirstValueSeries]) < 0))
                            this.Controls.Remove(this.m_arLabelCommon[(int)i - m_indxStartCommonFirstValueSeries]);
                        else ;
                    else
                        ;
                }
            }

            protected void addTGLabels(bool bIsTM)
            {
                int r = -1, c = -1
                    , i = 0;

                foreach (int key in m_tgLabels.Keys)
                {
                    i++;

                    this.Controls.Add(m_tgLabels[key][(int)TG.INDEX_VALUE.LABEL_DESC]);
                    c = (i - 1) / COUNT_TG_IN_COLUMN * COUNT_LABEL + (COL_TG_START + 0); r = (i - 1) % COUNT_TG_IN_COLUMN * (COUNT_ROWS / COUNT_TG_IN_COLUMN);
                    this.SetCellPosition(m_tgLabels[key][(int)TG.INDEX_VALUE.LABEL_DESC], new TableLayoutPanelCellPosition(c, r));
                    this.SetRowSpan(m_tgLabels[key][(int)TG.INDEX_VALUE.LABEL_DESC], (COUNT_ROWS / COUNT_TG_IN_COLUMN));

                    this.Controls.Add(m_tgLabels[key][(int)TG.INDEX_VALUE.FACT]);
                    c = (i - 1) / COUNT_TG_IN_COLUMN * COUNT_LABEL + (COL_TG_START + 1); r = (i - 1) % COUNT_TG_IN_COLUMN * (COUNT_ROWS / COUNT_TG_IN_COLUMN);
                    this.SetCellPosition(m_tgLabels[key][(int)TG.INDEX_VALUE.FACT], new TableLayoutPanelCellPosition(c, r));
                    this.SetRowSpan(m_tgLabels[key][(int)TG.INDEX_VALUE.FACT], (COUNT_ROWS / COUNT_TG_IN_COLUMN));

                    if (bIsTM == true)
                    {
                        this.Controls.Add(m_tgLabels[key][(int)TG.INDEX_VALUE.TM]);
                        c = (i - 1) / COUNT_TG_IN_COLUMN * COUNT_LABEL + (COL_TG_START + 2); r = (i - 1) % COUNT_TG_IN_COLUMN * (COUNT_ROWS / COUNT_TG_IN_COLUMN);
                        this.SetCellPosition(m_tgLabels[key][(int)TG.INDEX_VALUE.TM], new TableLayoutPanelCellPosition(c, r));
                        this.SetRowSpan(m_tgLabels[key][(int)TG.INDEX_VALUE.TM], (COUNT_ROWS / COUNT_TG_IN_COLUMN));
                    }
                    else
                        ;
                }
            }

            protected void removeTGLabels()
            {
                if (m_tgLabels.Count > 0)
                    foreach (int key in m_tgLabels.Keys)
                        for (int j = 0; j < (int)TG.INDEX_VALUE.COUNT_INDEX_VALUE; j++)
                            if ((!(m_tgLabels[key][j] == null)) && (!(this.Controls.IndexOf(m_tgLabels[key][j]) < 0)))
                                this.Controls.Remove(m_tgLabels[key][j]);
                            else
                                ;
                else
                    ;
            }

            /// <summary>
            /// Отобразить значение для элемента управления с условиями
            /// </summary>
            /// <param name="lbl">элемент управления, как ссылка</param>
            /// <param name="val">значение для отображения</param>
            /// <param name="bCheckVal">признак проверки значения</param>
            /// <param name="bPower">признак отображения значения мощности</param>
            /// <param name="adding">дополнительная для отображения строка</param>
            protected static void showValue(ref System.Windows.Forms.Label lbl, double val, bool bCheckVal, bool bPower, string adding)
            {
                if (!(lbl == null))
                    if (val == double.NegativeInfinity)
                        lbl.Text = adding;
                    else
                        if (bPower == true)
                            if ((val > 1)
                                || (bCheckVal == false))
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
            protected static void showValue(System.Windows.Forms.Label lbl, double val, bool bCheckVal)
            {
                if ((val > 1)
                    || (bCheckVal == false))
                    lbl.Text = val.ToString("F2");
                else
                    lbl.Text = 0.ToString("F0");
            }
            /// <summary>
            /// Возвратить цвет для отображения фактических значений
            /// </summary>
            /// <returns>Цвет для фактических значений</returns>
            protected Color getColorFactValues()
            {
                //Определить цвет
                if (m_parent.m_tecView.currHour == true)
                    if (m_parent.m_tecView.m_markWarning.IsMarked((int)TecView.INDEX_WARNING.LAST_MIN) == true)
                        return System.Drawing.Color.OrangeRed;
                    else
                        return System.Drawing.Color.LimeGreen;
                else
                    return System.Drawing.Color.OrangeRed;
            }
        }

        protected partial class PanelQuickDataStandard : HPanelQuickData
        {
            private enum INDEX_CONTEXTMENUITEM { FORECASTEE, TM };

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

            protected override TableLayoutPanelCellPosition getPositionCell(int indx)
            {
                int row = -1,
                    col = -1;

                switch ((CONTROLS)indx)
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

                if ((((ToolStripMenuItem)ContextMenuStrip.Items[(int)INDEX_CONTEXTMENUITEM.TM]).Checked == false) && (col > 2))
                    col--;
                else
                    ;

                //Console.WriteLine(@"PanelQuickData::getPositionCell () - " + indx.ToString () + @"[row=" + row + @", col=" + col + @"]");

                return new TableLayoutPanelCellPosition(col, row);
            }

            /// <summary>
            /// Обязательный метод для поддержки конструктора - не изменяйте
            /// содержимое данного метода при помощи редактора кода.
            /// </summary>
            private void InitializeComponent()
            {
                COUNT_ROWS = 12;
                SZ_COLUMN_LABEL = 40F;

                m_indxStartCommonFirstValueSeries = (int)CONTROLS.lblCommonP;
                m_indxStartCommonSecondValueSeries = (int)CONTROLS.lblCurrentE;
                m_iCountCommonLabels = (int)CONTROLS.lblDevEVal - (int)CONTROLS.lblCommonP + 1;

                m_tgLabels = new Dictionary<int, System.Windows.Forms.Label[]>();

                components = new System.ComponentModel.Container();

                bool bEnabled = true
                    //, bChecked = ! HStatisticUsers.RoleIsOperationPersonal
                    ;

                this.ContextMenuStrip = new ContextMenuStrip();
                this.ContextMenuStrip.Items.AddRange(new ToolStripMenuItem[] {
                    new ToolStripMenuItem (@"Прогноз ЭЭ"),
                    new ToolStripMenuItem (@"Знач. телеметрии") });
                //Checked = bChecked;
                this.ContextMenuStrip.Items[0].Enabled = bEnabled; ((ToolStripMenuItem)this.ContextMenuStrip.Items[(int)INDEX_CONTEXTMENUITEM.FORECASTEE]).Checked = HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.MENUCONTEXTITEM_PANELQUICKDATA_FORECASTEE); this.ContextMenuStrip.Items[(int)INDEX_CONTEXTMENUITEM.FORECASTEE].Click += OnItemClick;
                this.ContextMenuStrip.Items[(int)INDEX_CONTEXTMENUITEM.TM].Enabled = bEnabled; ((ToolStripMenuItem)this.ContextMenuStrip.Items[(int)INDEX_CONTEXTMENUITEM.TM]).Checked = HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.MENUCONTEXTITEM_PANELQUICKDATA_TMVALUES); this.ContextMenuStrip.Items[(int)INDEX_CONTEXTMENUITEM.TM].Click += OnItemClick;

                this.RowCount = COUNT_ROWS;

                for (int i = 0; i < this.RowCount + 1; i++)
                    this.RowStyles.Add(new RowStyle(SizeType.Percent, (float)Math.Round((float)100 / this.RowCount, 1)));

                this.lblPBRNumber = new System.Windows.Forms.Label();

                this.m_arLabelCommon = new System.Windows.Forms.Label[m_iCountCommonLabels];

                //
                // btnSetNow
                //
                this.Controls.Add(this.btnSetNow, 0, 0);
                this.SetRowSpan(this.btnSetNow, 3);
                // 
                // dtprDate
                // 
                this.Controls.Add(this.dtprDate, 0, 3);
                this.SetRowSpan(this.dtprDate, 3);
                // 
                // lblServerTime
                // 
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

                //Создание ОБЩих элементов управления
                Color foreColor, backClolor;
                float szFont;
                ContentAlignment align;
                //Size sz;
                string text = string.Empty;
                //int row = -1, col = -1;

                #region добавить поля для значений МОЩНОСТИ и их подписи
                for (CONTROLS i = (CONTROLS)m_indxStartCommonFirstValueSeries; i < CONTROLS.lblPBRrecVal + 1; i++)
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

                    createLabel((int)i, text, foreColor, backClolor, szFont, align);
                }
                #endregion

                #region добавить поля для значений ЭНЕРГИИ и их подписи
                for (CONTROLS i = (CONTROLS)m_indxStartCommonSecondValueSeries; i < CONTROLS.lblDevEVal + 1; i++)
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
                            foreColor = Color.LimeGreen;
                            backClolor = Color.Black;
                            szFont = 15F;
                            align = ContentAlignment.MiddleCenter;
                            //sz = arPlacement[(int)i].sz;
                            //col = 5;
                            break;
                        case CONTROLS.lblDevEVal:
                            foreColor = Color.Yellow;
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

                    createLabel((int)i, text, foreColor, backClolor, szFont, align);
                }
                #endregion

                //Создание пассивного эл./упр. "надпись" для увеличенного дублирования знач. Pтек
                m_lblPowerFactZoom = new HLabel(new Point(-1, -1), new Size(-1, -1), Color.LimeGreen, SystemColors.Control, 12F, ContentAlignment.MiddleCenter);
                m_lblPowerFactZoom.m_type = HLabel.TYPE_HLABEL.TOTAL_ZOOM;
                m_lblPowerFactZoom.Text = @"Pтек=----.--";                

                //OnSizeChanged(this, EventArgs.Empty);
            }

            #endregion

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

            private HLabel m_lblPowerFactZoom;            
            private System.Windows.Forms.Label lblPBRNumber;            

            public override void RestructControl()
            {
                COUNT_LABEL = 3; COUNT_TG_IN_COLUMN = 4; COL_TG_START = 6;
                COUNT_ROW_LABELCOMMON = 4;

                bool bPowerFactZoom = false;
                int cntCols = -1;                

                //Console.WriteLine(@"PanelQuickData::RestructControl () - вХод...");

                //Удаление ОБЩих элементов управления
                // рекомендация
                removeFirstCommonLabels((int)CONTROLS.lblPBRrecVal);
                // отклонение
                removeSecondCommonLabels((int)CONTROLS.lblDevEVal);

                //Удаление ТГ
                removeTGLabels();

                //Удаление ДУБЛирующей подписи
                if (!(this.Controls.IndexOf(m_lblPowerFactZoom) < 0)) this.Controls.Remove(m_lblPowerFactZoom); else ;

                //Удаление ПУСТой панели
                if (!(this.Controls.IndexOf(m_panelEmpty) < 0)) this.Controls.Remove(m_panelEmpty); else ;

                //Удаление стилей столбцов
                while (this.ColumnStyles.Count > 1)
                    this.ColumnStyles.RemoveAt(this.ColumnStyles.Count - 1);

                if (((ToolStripMenuItem)ContextMenuStrip.Items[(int)INDEX_CONTEXTMENUITEM.FORECASTEE]).Checked == false)
                {
                    COL_TG_START -= 2;
                }
                else
                    ;

                if (((ToolStripMenuItem)ContextMenuStrip.Items[(int)INDEX_CONTEXTMENUITEM.TM]).Checked == false)
                {
                    COUNT_LABEL--;
                    COL_TG_START--;
                }
                else
                    ;

                #region Добавить столбцы группы "Рекомендация"
                for (CONTROLS i = (CONTROLS)m_indxStartCommonFirstValueSeries; i < CONTROLS.lblPBRrecVal + 1; i++)
                {
                    bool bAddItem = false;
                    if (((ToolStripMenuItem)ContextMenuStrip.Items[(int)INDEX_CONTEXTMENUITEM.TM]).Checked == false)
                    {
                        if (i == CONTROLS.lblCommonPVal_TM)
                            ; //continue;
                        else
                        {
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
                        this.Controls.Add(this.m_arLabelCommon[(int)i - m_indxStartCommonFirstValueSeries]);
                        this.SetCellPosition(this.m_arLabelCommon[(int)i - m_indxStartCommonFirstValueSeries], getPositionCell((int)i));
                        this.SetRowSpan(this.m_arLabelCommon[(int)i - m_indxStartCommonFirstValueSeries], COUNT_ROW_LABELCOMMON);
                    }
                    else
                        ;
                }

                //Ширина столбцов группы "Рекомендация"
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SZ_COLUMN_LABEL));
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SZ_COLUMN_LABEL_VALUE));
                if (((ToolStripMenuItem)ContextMenuStrip.Items[(int)INDEX_CONTEXTMENUITEM.TM]).Checked == true)
                    //Телеметрия для объекта отображения
                    this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88F));
                else
                    ;
                #endregion

                #region Добавить столбцы группы "Отклонение" (в ~ от пользовательской настройки)
                if (((ToolStripMenuItem)ContextMenuStrip.Items[(int)INDEX_CONTEXTMENUITEM.FORECASTEE]).Checked == true)
                {
                    for (CONTROLS i = (CONTROLS)m_indxStartCommonSecondValueSeries; i < CONTROLS.lblDevEVal + 1; i++)
                    {
                        //this.Controls.Add(m_arLabelCommon[(int)i - m_indxStartCommonPVal]);
                        this.Controls.Add(this.m_arLabelCommon[(int)i - m_indxStartCommonFirstValueSeries]);
                        this.SetCellPosition(this.m_arLabelCommon[(int)i - m_indxStartCommonFirstValueSeries], getPositionCell((int)i));
                        this.SetRowSpan(this.m_arLabelCommon[(int)i - m_indxStartCommonFirstValueSeries], COUNT_ROW_LABELCOMMON);
                    }

                    //Ширина столбцов группы "Отклонение"
                    this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SZ_COLUMN_LABEL));
                    this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SZ_COLUMN_LABEL_VALUE));
                }
                else
                    ;
                #endregion

                bPowerFactZoom = false;
                cntCols = ((m_tgLabels.Count / COUNT_TG_IN_COLUMN) + ((m_tgLabels.Count % COUNT_TG_IN_COLUMN == 0) ? 0 : 1));                

                for (int i = 0; i < cntCols; i++)
                {
                    this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SZ_COLUMN_TG_LABEL));
                    this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SZ_COLUMN_TG_LABEL_VALUE));
                    if (((ToolStripMenuItem)ContextMenuStrip.Items[(int)INDEX_CONTEXTMENUITEM.TM]).Checked == true)
                        //Телеметрия ТГ
                        this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SZ_COLUMN_TG_LABEL_VALUE));
                    else
                        ;
                }

                if (m_tgLabels.Count > 0)
                    addTGLabels(((ToolStripMenuItem)ContextMenuStrip.Items[(int)INDEX_CONTEXTMENUITEM.TM]).Checked);
                else
                    ;

                //if ((Users.Role == (int)Users.ID_ROLES.NSS) || (Users.Role == (int)Users.ID_ROLES.MAJOR_MASHINIST) || (Users.Role == (int)Users.ID_ROLES.MASHINIST))
                if ((((ToolStripMenuItem)ContextMenuStrip.Items[(int)INDEX_CONTEXTMENUITEM.FORECASTEE]).Checked == false) && (((ToolStripMenuItem)ContextMenuStrip.Items[(int)INDEX_CONTEXTMENUITEM.TM]).Checked == false))
                {
                    this.Controls.Add(m_lblPowerFactZoom, COL_TG_START + cntCols * COUNT_LABEL + 0, 0);
                    this.SetRowSpan(m_lblPowerFactZoom, COUNT_ROWS);
                    this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88 * 3));

                    bPowerFactZoom = true;
                }
                else
                    ;

                this.Controls.Add(m_panelEmpty, COL_TG_START + cntCols * COUNT_LABEL + (bPowerFactZoom == true ? 1 : 0), 0);
                this.SetRowSpan(m_panelEmpty, COUNT_ROWS);
                this.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            }
        }

        partial class PanelQuickDataStandard : HPanelQuickData
        {
            ///// <summary>
            ///// Класс для хранения информации о местоположении элемента управления
            ///// </summary>
            //private class HPlacement
            //{
            //    public Size sz; public Point pt;
            //    public HPlacement(int x, int y, int w, int h)
            //    {
            //        pt.X = x; pt.Y = y; sz.Width = w; sz.Height = h;
            //    }
            //};

            public PanelQuickDataStandard()
            {
                InitializeComponent();
            }

            public PanelQuickDataStandard(IContainer container)
            {
                container.Add(this);

                InitializeComponent();
            }

            private void showTMValue(TG tg, ref double val, int min)
            {
                showTMValue(ref m_tgLabels[tg.m_id][(int)TG.INDEX_VALUE.TM]
                                                , tg.m_strKKS_NAME_TM
                                                , m_parent.m_tecView.m_dictValuesTG[tg.m_id].m_powerMinutes[min]
                                                , m_parent.m_tecView.m_dictValuesTG[tg.m_id].m_powerCurrent_TM
                                                , m_parent.m_tecView.m_dictValuesTG[tg.m_id].m_dtCurrent_TM
                                                , m_parent.m_tecView.serverTime
                                                , ref val);
            }

            private void showTMValue(ref Label lbl, string tg_kksname, double tg_val_fact, double tg_val, DateTime dt_val, DateTime dt_srv, ref double val)
            {
                string text = string.Empty
                    , textNotValue = @"---";
                bool bValidateDateTime = false;

                if (tg_kksname.Length > 0)
                {
                    if (!(tg_val < 0))
                    {// больше ИЛИ равно 0
                        bValidateDateTime = TecView.ValidateDatetimeTMValue(dt_srv, dt_val);

                        if (tg_val > 1)
                        {// больше 1                        
                            text = tg_val.ToString("F2");
                            if (bValidateDateTime == true)
                                if (!(val < 0))
                                    val += tg_val;
                                else
                                    ;
                            else
                                val = -1F;
                        }
                        else
                        {// в диапазоне [0; 1] включительно
                            text = 0.ToString("F0");
                        }
                    }
                    else
                    {// меньше 0
                        // в зависимости от значения АИИС КУЭ
                        if (!(tg_val_fact > 1))
                        {// факт. меньше ИЛИ равно 1
                            text = 0.ToString("F0");
                            bValidateDateTime = true;
                        }
                        else
                            text = textNotValue;

                        //val = -1F;
                    }
                }
                else
                {
                    text = textNotValue;
                    val = -1F;
                }

                //if (text.Equals (textNotValue) == true)
                if (bValidateDateTime == false)
                    lbl.ForeColor = Color.Orange;
                else
                    //if ((dt_srv - dt_val).TotalMinutes > 1)
                    if (m_parent.m_tecView.currHour == false)
                        lbl.ForeColor = Color.Orange;
                    else
                        lbl.ForeColor = Color.Green;

                lbl.Text = text;
            }

            /// <summary>
            /// Отобразить текущие значения
            /// </summary>
            public override void ShowTMValues()
            {
                double value_TM = 0.0;

                if ((!(m_parent == null)) && ((m_parent.m_tecView.serverTime - DateTime.MinValue).TotalSeconds > 1))
                {
                    //if (m_parent.m_tecView.currHour == true)
                    //{
                    //    if (m_parent.m_tecView.currentMinuteTM_GenWarning == true)
                    //    {
                    //        ; //m_parent.m_tecView.WarningReport("Значение телемеханики для одного из ТГ не найдено!");
                    //    }
                    //    else
                    //        ;
                    //}
                    //else
                    //    ;

                    int min = m_parent.m_tecView.lastMin < m_parent.m_tecView.m_valuesMins.Length ? m_parent.m_tecView.lastMin : m_parent.m_tecView.m_valuesMins.Length - 1;

                    if (m_parent.indx_TECComponent < 0) // значит этот view будет суммарным для всех ГТП
                    {
                        foreach (TECComponent g in m_parent.m_tecView.m_tec.list_TECComponents)
                        {
                            if (g.m_id < 500)
                                //Только ГТП
                                foreach (TG tg in g.m_listTG)
                                    showTMValue(tg, ref value_TM, min);
                            else
                                ;
                        }
                    }
                    else
                    {
                        foreach (TG tg in m_parent.m_tecView.m_tec.list_TECComponents[m_parent.indx_TECComponent].m_listTG)
                            showTMValue(tg, ref value_TM, min);
                    }
                }
                else
                    ;

                if (value_TM < 1)
                    value_TM = 0.0;
                else
                    ;

                showValue(ref m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblCommonPVal_TM - m_indxStartCommonFirstValueSeries]
                    , value_TM
                    , true
                    , true
                    , string.Empty);
                Color frCol = Color.Empty;
                if ((!(m_parent == null)) && (m_parent.m_tecView.currHour == true))
                    frCol = Color.Green;
                else
                    frCol = Color.Orange;
                m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblCommonPVal_TM - m_indxStartCommonFirstValueSeries].ForeColor = frCol;
            }

            /// <summary>
            /// Отобразить фактические значение
            /// </summary>
            public override void ShowFactValues()
            {
                if (!(m_parent == null))
                {
                    int indxStartCommonPVal = m_indxStartCommonFirstValueSeries;
                    int i = -1, j = -1
                        , min = m_parent.m_tecView.lastMin;

                    if (!(min == 0)) min--; else ;

                    bool bPrevValueValidate = false
                        , bMinValuesReceived = true;
                    double prevValue = 0F, value = 0F
                        , valueEBefore = 0F
                        , valueECur = 0F
                        , valueEFuture = 0F;

                    bPrevValueValidate = double.TryParse(m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblCommonPVal_Fact - indxStartCommonPVal].Text, out prevValue);

                    value = m_parent.m_tecView.GetSummaFactValues();

                    //14.04.2015 ???
                    if ((bMinValuesReceived == true) && (value < 1F) && (bPrevValueValidate == true) && (!(prevValue < 1F)))
                    {
                        //Logging.Logg().Debug(@"PanelQuickData::ShowFactValues () - value < 1F", Logging.INDEX_MESSAGE.NOT_SET);
                        Logging.Logg().Warning(@"PanelQuickData::ShowFactValues () - value < 1F", Logging.INDEX_MESSAGE.NOT_SET);

                        //return;
                    }
                    else
                        ;

                    for (i = 0; i < m_parent.m_tecView.listTG.Count; i++)
                        if (m_parent.m_tecView.m_dictValuesTG[m_parent.m_tecView.listTG[i].m_id].m_bPowerMinutesRecieved == true)
                            for (j = 0; j < min; j++)
                                if ((!(m_parent.m_tecView.m_dictValuesTG[m_parent.m_tecView.listTG[i].m_id].m_powerMinutes[j] < 0))
                                    && (m_parent.m_tecView.m_dictValuesTG[m_parent.m_tecView.listTG[i].m_id].m_powerMinutes[j] > 1))
                                    valueEBefore += m_parent.m_tecView.m_dictValuesTG[m_parent.m_tecView.listTG[i].m_id].m_powerMinutes[j] / 20;
                                else
                                    ;
                        else
                            ;

                    showValue(ref m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblCommonPVal_Fact - indxStartCommonPVal]
                        , value
                        , true
                        , true
                        , string.Empty);
                    //if (this.ContextMenuStrip.Items [(int)INDEX_CONTEXTMENU_ITEM_FUTURE_EE] = false)
                    //if ((!(Users.Role == (int)Users.ID_ROLES.KOM_DISP)) && (!(Users.Role == (int)Users.ID_ROLES.ADMIN)))
                    m_lblPowerFactZoom.Text = @"Pтек=" + value.ToString(@"F2");
                    //else ;

                    valueECur = value / 20;
                    showValue(ref m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblCurrentEVal - indxStartCommonPVal]
                        , valueECur
                        , true
                        , true
                        , string.Empty);

                    valueEFuture = valueECur * (20 - min - 0);
                    showValue(ref m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblHourEVal - indxStartCommonPVal]
                        , valueEBefore + valueECur + valueEFuture
                        , true
                        , true
                        , string.Empty);

                    if ((m_parent.m_tecView.adminValuesReceived == true) && (m_parent.m_tecView.currHour == true))
                    {
                        showValue(ref m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblPBRrecVal - indxStartCommonPVal]
                            , m_parent.m_tecView.recomendation
                            , true
                            , true
                            , string.Empty);
                    }
                    else
                    {
                        m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblPBRrecVal - indxStartCommonPVal].Text = "---";
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
                        int hour = m_parent.m_tecView.lastHour;
                        if (hour == 24)
                            hour = 23;
                        else
                            ;

                        if (m_parent.m_tecView.currHour == true)
                        {
                            for (i = 1; i < m_parent.m_tecView.lastMin; i++)
                                summ += m_parent.m_tecView.m_valuesMins[i].valuesFact;
                            if (!(min == 0))
                                showValue(ref m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblAverPVal - indxStartCommonPVal]
                                    , summ / min
                                    , true
                                    , true
                                    , string.Empty);
                            else
                                m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblAverPVal - indxStartCommonPVal].Text = 0.ToString("F0");
                        }
                        else
                        {
                            //if ((m_parent.m_tecView.m_valuesHours.addonValues == true) && (hour == m_parent.m_tecView.m_valuesHours.hourAddon))
                            //    summ = m_parent.m_tecView.m_valuesHours.valuesFactAddon;
                            //else
                            summ = m_parent.m_tecView.m_valuesHours[hour].valuesFact;

                            showValue(ref m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblAverPVal - indxStartCommonPVal]
                                , summ
                                , true
                                , true
                                , string.Empty);
                        }

                        //if (! ([lastHour] == 0))
                        double dblDevEVal = -1.0;
                        bool bDevEVal = true;
                        if ((m_parent.m_tecView.lastHour < m_parent.m_tecView.m_valuesHours.Length) &&
                            (!(m_parent.m_tecView.m_valuesHours[m_parent.m_tecView.lastHour].valuesUDGe == 0)))
                        {
                            dblDevEVal = ((((valueEBefore + valueECur + valueEFuture) -
                                        m_parent.m_tecView.m_valuesHours[m_parent.m_tecView.lastHour].valuesUDGe) / m_parent.m_tecView.m_valuesHours[m_parent.m_tecView.lastHour].valuesUDGe) * 100);
                            if (Math.Abs(dblDevEVal) < 100) ; else bDevEVal = false;

                            Logging.Logg().Debug(@"dblDevEVal=" + dblDevEVal.ToString(@"F3")
                                                + @" (valueEBefore=" + valueEBefore.ToString(@"F3")
                                                + @"; valueECur=" + valueECur.ToString(@"F3")
                                                + @"; valueEFuture=" + valueEFuture.ToString(@"F3")
                                                + @"; valuesUDGe=" + m_parent.m_tecView.m_valuesHours[m_parent.m_tecView.lastHour].valuesUDGe
                                                + @") [" + hour + @", " + m_parent.m_tecView.lastMin + @"]"
                                                , Logging.INDEX_MESSAGE.D_004);
                        }
                        else
                            bDevEVal = false;

                        if (bDevEVal == true)
                            showValue(ref m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblDevEVal - indxStartCommonPVal]
                                , dblDevEVal
                                , true
                                , false
                                , @"%");
                        else
                            showValue(ref m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblDevEVal - indxStartCommonPVal]
                                , double.NegativeInfinity
                                , true
                                , false
                                , @"---");
                    }

                    if ((m_parent.m_tecView.currHour == true) && (min == 0))
                        m_parent.m_tecView.recalcAver = bPrevRecalcAver;
                    else
                        ;

                    if (m_parent.m_tecView.currHour == true)
                    {
                        //if (m_parent.m_tecView.lastHourError == true)
                        //{
                        //    m_parent.m_tecView.ErrorReport("По текущему часу значений не найдено!");
                        //}
                        //else
                        //{
                        //    if (m_parent.m_tecView.lastHourHalfError == true)
                        //    {
                        //        m_parent.m_tecView.ErrorReport("За текущий час не получены некоторые получасовые значения!");
                        //    }
                        //    else
                        //    {
                        m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblAverPVal - indxStartCommonPVal].ForeColor =
                        m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblCommonPVal_Fact - indxStartCommonPVal].ForeColor =
                            m_lblPowerFactZoom.ForeColor = getColorFactValues();

                        if (m_parent.m_tecView.m_markWarning.IsMarked((int)TecView.INDEX_WARNING.LAST_MIN) == true)
                        {
                            string strErrMsg = @"По текущему ";
                            if ((m_parent.m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_AISKUE)
                                || (m_parent.m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO)
                                || (m_parent.m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN))
                                strErrMsg += @"3-минутному";
                            else
                                if (m_parent.m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN)
                                    strErrMsg += @"1-минутному";
                                else
                                    ;
                            strErrMsg += @" отрезку значений не найдено!";
                            m_parent.m_tecView.ErrorReport(strErrMsg);
                        }
                        else
                        {
                            if (!(m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblCurrentEVal - indxStartCommonPVal] == null))
                                m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblCurrentEVal - indxStartCommonPVal].ForeColor =
                                    System.Drawing.Color.LimeGreen;
                            else ;

                            if (!(m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblHourEVal - indxStartCommonPVal] == null))
                                m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblHourEVal - indxStartCommonPVal].ForeColor =
                                    System.Drawing.Color.Yellow;
                            else ;
                        }
                        //    }
                        //}
                    }
                    else
                    {
                        m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblAverPVal - indxStartCommonPVal].ForeColor =
                        m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblCommonPVal_Fact - indxStartCommonPVal].ForeColor =
                            m_lblPowerFactZoom.ForeColor = System.Drawing.Color.OrangeRed;

                        if ((!(m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblCurrentEVal - indxStartCommonPVal] == null)) && (!(m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblHourEVal - indxStartCommonPVal] == null)))
                            m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblCurrentEVal - indxStartCommonPVal].ForeColor =
                            m_arLabelCommon[(int)PanelQuickDataStandard.CONTROLS.lblHourEVal - indxStartCommonPVal].ForeColor =
                                System.Drawing.Color.OrangeRed;
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
                            if (g.IsGTP == true)
                                //Только ГТП
                                foreach (TG tg in g.m_listTG)
                                {//Цикл по списку с ТГ
                                    //Отобразить значение
                                    if ((!(m_parent.m_tecView.m_dictValuesTG[tg.m_id].m_powerMinutes == null))
                                        //Ошибка при запуске 'CustomTecView' с оперативной панелью (Исправлено: 13.05.2015)
                                        // не "успевает" new ???
                                        && (!(m_parent.m_tecView.m_dictValuesTG[tg.m_id].m_powerMinutes[min] < 0)))
                                        showValue(m_tgLabels[tg.m_id][(int)TG.INDEX_VALUE.FACT]
                                            , m_parent.m_tecView.m_dictValuesTG[tg.m_id].m_powerMinutes[min]
                                            , true);
                                    else
                                    {
                                        m_tgLabels[tg.m_id][(int)TG.INDEX_VALUE.FACT].Text = "---";
                                        //m_tgToolTips[tg.m_id][(int)TG.INDEX_VALUE.FACT].SetToolTip (m_tgLabels[tg.m_id][(int)TG.INDEX_VALUE.FACT], @"Знач. недостоверно");
                                    }

                                    m_tgLabels[tg.m_id][(int)TG.INDEX_VALUE.FACT].ForeColor = getColorFactValues();

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
                            if ((!(m_parent.m_tecView.m_dictValuesTG[comp.m_listTG[0].m_id].m_powerMinutes == null))
                                && (!(m_parent.m_tecView.m_dictValuesTG[comp.m_listTG[0].m_id].m_powerMinutes[min] < 0)))
                                showValue(m_tgLabels[comp.m_listTG[0].m_id][(int)TG.INDEX_VALUE.FACT]
                                    , m_parent.m_tecView.m_dictValuesTG[comp.m_listTG[0].m_id].m_powerMinutes[min]
                                    , true);
                            else
                                m_tgLabels[comp.m_listTG[0].m_id][(int)TG.INDEX_VALUE.FACT].Text = "---";

                            m_tgLabels[comp.m_listTG[0].m_id][(int)TG.INDEX_VALUE.FACT].ForeColor = getColorFactValues();

                            i++;
                        }
                    }

                    //Logging.Logg().Debug(@"PanelQuickData::ShowFactValues () - вЫход...");
                }
                else
                    ;
            }            

            private void OnItemClick(object obj, EventArgs ev)
            {
                ((ToolStripMenuItem)obj).Checked = !((ToolStripMenuItem)obj).Checked;

                if (ContextMenuStrip.Items.IndexOf((ToolStripMenuItem)obj) == (int)INDEX_CONTEXTMENUITEM.TM)
                    m_parent.m_tecView.m_bLastValue_TM_Gen = ((ToolStripMenuItem)obj).Checked;
                else
                    ;

                RestructControl();
            }
        }
    }
}
