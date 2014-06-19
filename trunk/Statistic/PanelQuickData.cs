using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

using StatisticCommon;

namespace Statistic
{
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

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }

        #endregion

        PanelTecView m_parent;

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

        const int delimXCommonVal = 115, delimXCommonPair = 35,
            widthPanelDateTime = 85,
            widthLabelName = 30;
        HPlacement[] arPlacement =
                {   new HPlacement (112, 6, widthLabelName, 13), new HPlacement (143, 0, 79, 27), //lblCommonP, lblCommonPVal
                                                                new HPlacement (224, 0, 79, 27),
                    new HPlacement (112, 36, widthLabelName, 13), new HPlacement (143, 30, 79, 27), //lblAverP, lblAverPVal
                    new HPlacement (112, 66, widthLabelName, 13), new HPlacement (143, 60, 79, 27),
                    new HPlacement (330, 6, widthLabelName, 13), new HPlacement (360, 0, 79, 27), //lblCurrentE, lblCurrentEVal
                    new HPlacement (330, 36, widthLabelName, 13), new HPlacement (360, 30, 79, 27), //lblDevE, lblDevEVal
                    new HPlacement (330, 66, widthLabelName, 13), new HPlacement (360, 60, 79, 27),
                    new HPlacement (3, 3, widthPanelDateTime, 20), //dtprDate
                    new HPlacement (/*0, 1, 67, 20*/3, 26, widthPanelDateTime, 20), //lblServerTime
                    new HPlacement (/*6, 643, 93, 23*/3, 49, widthPanelDateTime, 23), //btnSetNow
                    new HPlacement (/*0, 22, 67, 20*/3, 75, widthPanelDateTime, 20) //lblPBRNumber
                };

        public const int m_indxStartCommonPVal = (int)CONTROLS.lblCommonP,
                    m_indxStartCommonEVal = (int)CONTROLS.lblCurrentE;
        private const int iCountLabels = (int)CONTROLS.lblDevEVal - (int)CONTROLS.lblCommonP + 1;

        public System.Windows.Forms.Label[] m_arLabelCommon;

        private List<System.Windows.Forms.Label> tgsName;
        private List<System.Windows.Forms.Label>[] tgsValues = { new List<System.Windows.Forms.Label>(), new List<System.Windows.Forms.Label>() };

        public System.Windows.Forms.Button btnSetNow;
        public DateTimePicker dtprDate;
        public System.Windows.Forms.Label lblServerTime;
        private System.Windows.Forms.Label lblPBRNumber;

        public void Initialize()
        {
            m_parent = (PanelTecView)Parent;

            this.btnSetNow = new System.Windows.Forms.Button();
            this.dtprDate = new System.Windows.Forms.DateTimePicker();
            this.lblServerTime = new System.Windows.Forms.Label();
            this.lblPBRNumber = new System.Windows.Forms.Label();

            this.m_arLabelCommon = new System.Windows.Forms.Label[iCountLabels];

            this.Controls.AddRange(new Control[] {
                    this.btnSetNow,
                    this.dtprDate,
                    this.lblServerTime,
                    this.lblPBRNumber});

            //
            // btnSetNow
            //
            this.btnSetNow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSetNow.Location = arPlacement[(int)CONTROLS.btnSetNow].pt;
            this.btnSetNow.Name = "btnSetNow";
            this.btnSetNow.Size = arPlacement[(int)CONTROLS.btnSetNow].sz;
            this.btnSetNow.TabIndex = 2;
            this.btnSetNow.Text = "Текущий час";
            this.btnSetNow.UseVisualStyleBackColor = true;
            // 
            // dtprDate
            // 
            this.dtprDate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.dtprDate.Location = arPlacement[(int)CONTROLS.dtprDate].pt;
            this.dtprDate.Name = "dtprDate";
            this.dtprDate.Size = arPlacement[(int)CONTROLS.dtprDate].sz;
            this.dtprDate.TabIndex = 4;
            // 
            // lblServerTime
            // 
            this.lblServerTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblServerTime.AutoSize = false;
            this.lblServerTime.Location = arPlacement[(int)CONTROLS.lblServerTime].pt;
            this.lblServerTime.Name = "lblServerTime";
            this.lblServerTime.Size = arPlacement[(int)CONTROLS.lblServerTime].sz;
            this.lblServerTime.TabIndex = 5;
            this.lblServerTime.Text = "--:--:--";
            this.lblServerTime.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblServerTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblServerTime.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblPBRNumber
            // 
            this.lblPBRNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblPBRNumber.AutoSize = false;
            this.lblPBRNumber.Location = arPlacement[(int)CONTROLS.lblPBRNumber].pt;
            this.lblPBRNumber.Name = "lblPBRNumber";
            this.lblPBRNumber.Size = arPlacement[(int)CONTROLS.lblPBRNumber].sz;
            this.lblPBRNumber.TabIndex = 5;
            this.lblPBRNumber.Text = "---";
            this.lblPBRNumber.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblPBRNumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblPBRNumber.TextAlign = ContentAlignment.MiddleCenter;

            Color foreColor, backClolor;
            float szFont;
            ContentAlignment align;
            Size sz;
            string text = string.Empty;
            for (CONTROLS i = (CONTROLS)m_indxStartCommonPVal; i < CONTROLS.lblPBRrecVal + 1; i++)
            {
                switch (i)
                {
                    case CONTROLS.lblCommonP:
                    case CONTROLS.lblPBRrec:
                    case CONTROLS.lblAverP:
                        foreColor = Color.Black;
                        backClolor = Color.Empty;
                        szFont = 8F;
                        align = ContentAlignment.MiddleLeft;
                        sz = new Size(-1, -1);
                        break;
                    case CONTROLS.lblCommonPVal_Fact:
                    case CONTROLS.lblPBRrecVal:
                    case CONTROLS.lblAverPVal:
                        foreColor = Color.LimeGreen;
                        backClolor = Color.Black;
                        szFont = 15F;
                        align = ContentAlignment.MiddleCenter;
                        sz = arPlacement[(int)i].sz;
                        break;
                    case CONTROLS.lblCommonPVal_TM:
                        foreColor = Color.Green;
                        backClolor = Color.Black;
                        szFont = 15F;
                        align = ContentAlignment.MiddleCenter;
                        sz = arPlacement[(int)i].sz;
                        break;
                    default:
                        foreColor = Color.Yellow;
                        backClolor = Color.Red;
                        szFont = 6F;
                        align = ContentAlignment.MiddleCenter;
                        sz = new Size(-1, -1);
                        break;
                }

                m_arLabelCommon[(int)i - m_indxStartCommonPVal] = HLabel.createLabel(/*i.ToString()*/@"---",
                                                                                    new HLabelStyles(arPlacement[(int)i].pt, sz,
                                                                                    foreColor, backClolor,
                                                                                    szFont, align));
                switch (i)
                {
                    case CONTROLS.lblCommonP:
                        text = @"P1"; //@"P тек";
                        break;
                    case CONTROLS.lblPBRrec:
                        text = @"P рек";
                        break;
                    case CONTROLS.lblAverP:
                        text = @"P ср";
                        break;
                    default:
                        text = string.Empty;
                        break;
                }
                if (text.Equals(string.Empty) == false) m_arLabelCommon[(int)i - m_indxStartCommonPVal].Text = text; else ;
                this.Controls.Add(m_arLabelCommon[(int)i - m_indxStartCommonPVal]);
            }

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
                        sz = new Size(-1, -1);
                        break;
                    case CONTROLS.lblCurrentEVal:
                    case CONTROLS.lblHourEVal:
                    case CONTROLS.lblDevEVal:
                        foreColor = Color.LimeGreen;
                        backClolor = Color.Black;
                        szFont = 15F;
                        align = ContentAlignment.MiddleCenter;
                        sz = arPlacement[(int)i].sz;
                        break;
                    default:
                        foreColor = Color.Red;
                        backClolor = Color.Yellow;
                        szFont = 6F;
                        align = ContentAlignment.MiddleCenter;
                        sz = new Size(-1, -1);
                        break;
                }

                m_arLabelCommon[(int)i - m_indxStartCommonPVal] = HLabel.createLabel(/*i.ToString()*/@"---",
                                                                                    new HLabelStyles(arPlacement[(int)i].pt, sz,
                                                                                    foreColor, backClolor,
                                                                                    szFont, align));
                this.Controls.Add(m_arLabelCommon[(int)i - m_indxStartCommonPVal]);
            }

            m_parent.m_list_TECComponents = new List<TECComponentBase>();
            tgsName = new List<System.Windows.Forms.Label>();

            int positionXName = 515, positionXValue = 504, positionYName = 6, positionYValue = 19;
            //countTG = 0;
            List<int> tg_ids = new List<int>(); //Временный список идентификаторов ТГ

            if (m_parent.num_TECComponent < 0) // значит этот view будет суммарным для всех ГТП
            {
                foreach (TECComponent g in m_parent.tec.list_TECComponents)
                {
                    if ((g.m_id > 100) && (g.m_id < 500))
                        m_parent.m_list_TECComponents.Add(g);
                    else
                        ;

                    foreach (TG tg in g.TG)
                    {
                        //Проверка обработки текущего ТГ
                        if (tg_ids.IndexOf(tg.m_id) == -1)
                        {
                            tg_ids.Add(tg.m_id); //Запомнить, что ТГ обработан

                            positionYValue = 19;
                            addTGView(ref tg.name_shr, ref positionXName, ref positionYName, ref positionXValue, ref positionYValue);
                        }
                        else
                            ;
                    }
                }
            }
            else
            {
                foreach (TG tg in m_parent.tec.list_TECComponents[m_parent.num_TECComponent].TG)
                {
                    tg_ids.Add(tg.m_id); //Добавить без проверки

                    positionYValue = 19;
                    addTGView(ref tg.name_shr, ref positionXName, ref positionYName, ref positionXValue, ref positionYValue);

                    m_parent.m_list_TECComponents.Add(tg);
                }
            }

            m_parent.sensorId2TG = new TG[tg_ids.Count];
        }

        public void addTGView(ref string name_shr, /*ref float val,*/ ref int positionXName, ref int positionYName, ref int positionXValue, ref int positionYValue)
        {
            System.Windows.Forms.Label lblName = new System.Windows.Forms.Label();

            lblName.AutoSize = true;
            lblName.Location = new System.Drawing.Point(positionXName, positionYName);
            lblName.Name = "lblName" + name_shr;
            //lblName.AutoSize = false;
            lblName.Size = new System.Drawing.Size(32, 13);
            //lblName.TabIndex = 4 + countTG;
            lblName.Text = name_shr;

            tgsName.Add(lblName);

            System.Windows.Forms.Label lblValue = null;

            //lblValue = new System.Windows.Forms.Label();
            //createTGLabelValue(ref lblValue, name_shr + TG.INDEX_VALUE.FACT.ToString(), val, System.Drawing.Color.LimeGreen, positionXValue, positionYValue);
            //lblValue.TextAlign = ContentAlignment.MiddleCenter;
            lblValue = HLabel.createLabel(name_shr + "_Fact", new HLabelStyles(new Point(positionXValue, positionYValue), new Size(63, 27), Color.LimeGreen, Color.Black, 15F, ContentAlignment.MiddleCenter));
            tgsValues[(int)TG.INDEX_VALUE.FACT].Add(lblValue);

            positionYValue += 29;

            //lblValue = new System.Windows.Forms.Label();
            //createTGLabelValue(ref lblValue, name_shr + TG.INDEX_VALUE.TM.ToString(), val, Color.Green, positionXValue, positionYValue);
            //lblValue.TextAlign = ContentAlignment.MiddleCenter;
            lblValue = HLabel.createLabel(name_shr + "_Fact", new HLabelStyles(new Point(positionXValue, positionYValue), new Size(63, 27), Color.Green, Color.Black, 15F, ContentAlignment.MiddleCenter));
            tgsValues[(int)TG.INDEX_VALUE.TM].Add(lblValue);

            positionXName += 69;
            positionXValue += 69;

            this.Controls.Add(lblName);
            this.Controls.Add(tgsValues[(int)TG.INDEX_VALUE.FACT][tgsValues[(int)TG.INDEX_VALUE.FACT].Count - 1]);
            this.Controls.Add(tgsValues[(int)TG.INDEX_VALUE.TM][tgsValues[(int)TG.INDEX_VALUE.TM].Count - 1]);
        }
    }

    public partial class PanelQuickData : Panel
    {
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

        public void ShowTMValues()
        {
            double value_TM = 0.0;
            int i = 0;
            if (m_parent.num_TECComponent < 0) // значит этот view будет суммарным для всех ГТП
            {
                foreach (TECComponent g in m_parent.tec.list_TECComponents)
                {
                    if (g.m_id < 500)
                        //Только ГТП
                        foreach (TG tg in g.TG)
                        {
                            if (tg.id_tm > 0)
                            {
                                if (tg.power_TM > 1)
                                {
                                    tgsValues[(int)TG.INDEX_VALUE.TM][i].Text = tg.power_TM.ToString("F2");
                                    value_TM += tg.power_TM;
                                }
                                else
                                    tgsValues[(int)TG.INDEX_VALUE.TM][i].Text = 0.ToString("F0");

                            }
                            else
                            {
                                tgsValues[(int)TG.INDEX_VALUE.TM][i].Text = "---";
                            }
                            i++;
                        }
                    else
                        ;
                }
            }
            else
            {
                foreach (TG tg in m_parent.tec.list_TECComponents[m_parent.num_TECComponent].TG)
                {
                    if (tg.id_tm > 0)
                    {
                        if (tg.power_TM > 1)
                        {
                            tgsValues[(int)TG.INDEX_VALUE.TM][i].Text = tg.power_TM.ToString("F2");
                            value_TM += tg.power_TM;
                        }
                        else
                            tgsValues[(int)TG.INDEX_VALUE.TM][i].Text = 0.ToString("F0");
                    }
                    else
                    {
                        tgsValues[(int)TG.INDEX_VALUE.TM][i].Text = "---";
                    }
                    i++;
                }
            }

            if (value_TM < 1)
                value_TM = 0.0;
            else
                ;

            showValue(ref m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblCommonPVal_TM - m_indxStartCommonPVal], value_TM);
        }

        private void showValue(ref System.Windows.Forms.Label lbl, double val)
        {
            if (val > 1)
                lbl.Text = val.ToString("F2");
            else
                lbl.Text = 0.ToString("F0");
        }

        private void showValue(System.Windows.Forms.Label lbl, double val)
        {
            if (val > 1)
                lbl.Text = val.ToString("F2");
            else
                lbl.Text = 0.ToString("F0");
        }

        public void ShowFactValues()
        {
            int indxStartCommonPVal = PanelQuickData.m_indxStartCommonPVal;
            int i = -1, j = -1,
                min = m_parent.lastMin;

            if (!(min == 0)) min--; else ;

            double valueEBefore = 0.0,
                    valueECur = 0.0,
                    valueEFuture = 0.0;
            for (i = 0; i < m_parent.sensorId2TG.Length; i++)
                for (j = 0; j < min; j++)
                    valueEBefore += m_parent.sensorId2TG[i].power[j] / 20;

            double value = 0;
            for (i = 0; i < m_parent.sensorId2TG.Length; i++)
                if (m_parent.sensorId2TG[i].power[min] > 1) value += m_parent.sensorId2TG[i].power[min]; else ;

            showValue(ref m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblCommonPVal_Fact - indxStartCommonPVal], value);

            valueECur = value / 20;
            showValue(ref m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblCurrentEVal - indxStartCommonPVal], valueECur);

            valueEFuture = valueECur * (20 - min - 1);
            showValue(ref m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblHourEVal - indxStartCommonPVal], valueEBefore + valueECur + valueEFuture);

            if ((m_parent.adminValuesReceived == true) && (m_parent.currHour == true))
            {
                showValue(ref m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblPBRrecVal - indxStartCommonPVal], m_parent.recomendation);
            }
            else
            {
                m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblPBRrecVal - indxStartCommonPVal].Text = "---";
            }

            double summ = 0;
            bool bPrevRecalcAver = m_parent.recalcAver;

            if ((m_parent.currHour == true) && (min == 0))
                m_parent.recalcAver = false;

            if (m_parent.recalcAver == true)
            {
                if (m_parent.currHour == true)
                {
                    for (i = 1; i < m_parent.lastMin; i++)
                        summ += m_parent.m_valuesMins.valuesFact[i];
                    if (!(min == 0))
                        showValue(ref m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblAverPVal - indxStartCommonPVal], summ / min);
                    else
                        m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblAverPVal - indxStartCommonPVal].Text = 0.ToString("F0");
                }
                else
                {
                    int hour = m_parent.lastHour;
                    if (hour == 24)
                        hour = 23;

                    if ((m_parent.m_valuesHours.addonValues == true) && (hour == m_parent.m_valuesHours.hourAddon))
                        summ = m_parent.m_valuesHours.valuesFactAddon;
                    else
                        summ = m_parent.m_valuesHours.valuesFact[hour];

                    showValue(ref m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblAverPVal - indxStartCommonPVal], summ);
                }

                //if (! ([lastHour] == 0))
                if ((m_parent.lastHour < m_parent.m_valuesHours.valuesUDGe.Length) &&
                    (!(m_parent.m_valuesHours.valuesUDGe[m_parent.lastHour] == 0)))
                {
                    m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblDevEVal - indxStartCommonPVal].Text = ((((valueEBefore + valueECur + valueEFuture) -
                                                m_parent.m_valuesHours.valuesUDGe[m_parent.lastHour]) / m_parent.m_valuesHours.valuesUDGe[m_parent.lastHour]) * 100).ToString("F2") + "%";
                }
                else
                    m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblDevEVal - indxStartCommonPVal].Text = "---";
            }

            if ((m_parent.currHour == true) && (min == 0))
                m_parent.recalcAver = bPrevRecalcAver;
            else
                ;

            if (m_parent.currHour == true)
            {
                if (m_parent.lastHourError == true)
                {
                    m_parent.ErrorReport("По текущему часу значений не найдено!");
                }
                else
                {
                    if (m_parent.lastHourHalfError == true)
                    {
                        m_parent.ErrorReport("За текущий час не получены некоторые получасовые значения!");
                    }
                    else
                    {
                        if (m_parent.lastMinError == true)
                        {
                            m_parent.ErrorReport("По текущему трёхминутному отрезку значений не найдено!");
                            m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblAverPVal - indxStartCommonPVal].ForeColor = System.Drawing.Color.OrangeRed;
                            m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblCommonPVal_Fact - indxStartCommonPVal].ForeColor = System.Drawing.Color.OrangeRed;
                        }
                        else
                        {
                            m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblAverPVal - indxStartCommonPVal].ForeColor = System.Drawing.Color.LimeGreen;
                            m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblCommonPVal_Fact - indxStartCommonPVal].ForeColor = System.Drawing.Color.LimeGreen;

                            m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblCurrentEVal - indxStartCommonPVal].ForeColor = System.Drawing.Color.LimeGreen;
                            m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblHourEVal - indxStartCommonPVal].ForeColor = System.Drawing.Color.Yellow;
                        }
                    }
                }
            }
            else
            {
                m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblAverPVal - indxStartCommonPVal].ForeColor =
                m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblCommonPVal_Fact - indxStartCommonPVal].ForeColor =

                m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblCurrentEVal - indxStartCommonPVal].ForeColor =
                m_arLabelCommon[(int)PanelQuickData.CONTROLS.lblHourEVal - indxStartCommonPVal].ForeColor = System.Drawing.Color.OrangeRed;
            }

            lblPBRNumber.Text = m_parent.lastLayout;

            //ShowTGValue
            i = 0;
            if (m_parent.num_TECComponent < 0) // значит этот view будет суммарным для всех ГТП
            {
                foreach (TECComponent g in m_parent.m_list_TECComponents)
                {
                    if (g.m_id < 500)
                        //Только ГТП
                        foreach (TG tg in g.TG)
                        {
                            if (tg.receivedMin[min] == true)
                            {
                                showValue(tgsValues[(int)TG.INDEX_VALUE.FACT][i], tg.power[min]);
                                if (m_parent.currHour == true)
                                    tgsValues[(int)TG.INDEX_VALUE.FACT][i].ForeColor = System.Drawing.Color.LimeGreen;
                                else
                                    tgsValues[(int)TG.INDEX_VALUE.FACT][i].ForeColor = System.Drawing.Color.OrangeRed;
                            }
                            else
                            {
                                tgsValues[(int)TG.INDEX_VALUE.FACT][i].Text = "---";
                                tgsValues[(int)TG.INDEX_VALUE.FACT][i].ForeColor = System.Drawing.Color.OrangeRed;
                            }
                            i++;
                        }
                    else
                        ;
                }
            }
            else
            {
                foreach (TG t in m_parent.m_list_TECComponents)
                {
                    if (t.receivedMin[min] == true)
                    {
                        showValue(tgsValues[(int)TG.INDEX_VALUE.FACT][i], t.power[min]);
                        if (m_parent.currHour == true)
                            tgsValues[(int)TG.INDEX_VALUE.FACT][i].ForeColor = System.Drawing.Color.LimeGreen;
                        else
                            tgsValues[(int)TG.INDEX_VALUE.FACT][i].ForeColor = System.Drawing.Color.OrangeRed;
                    }
                    else
                    {
                        tgsValues[(int)TG.INDEX_VALUE.FACT][i].Text = "---";
                        tgsValues[(int)TG.INDEX_VALUE.FACT][i].ForeColor = System.Drawing.Color.OrangeRed;
                    }
                    i++;
                }
            }
        }
    }
}
