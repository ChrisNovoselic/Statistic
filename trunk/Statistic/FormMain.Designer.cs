﻿using System.Collections.Generic; //Dictionary

using HClassLibrary;
using StatisticCommon;

namespace Statistic
{
    partial class FormMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            m_dictAddingTabs = new Dictionary<int, ADDING_TAB>();

            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.MainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            this.файлToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.файлПрофильToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.файлПрофильЗагрузитьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.файлПрофильСохранитьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.файлПрофильАвтоЗагрузитьСохранитьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.выходToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.видToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.панельГрафическихToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            m_dictAddingTabs.Add((int)ID_ADDING_TAB.CUSTOM_2X2_1, new ADDING_TAB("выборОбъектывыборОбъекты22-1-ToolStripMenuItem", "Окно 1", HStatisticTabCtrlEx.TYPE_TAB.FLOAT));
            m_dictAddingTabs.Add((int)ID_ADDING_TAB.CUSTOM_2X2_2, new ADDING_TAB("выборОбъектывыборОбъекты22-2-ToolStripMenuItem", "Окно 2", HStatisticTabCtrlEx.TYPE_TAB.FLOAT));
            m_dictAddingTabs.Add((int)ID_ADDING_TAB.CUSTOM_2X2_3, new ADDING_TAB("выборОбъектывыборОбъекты22-3-ToolStripMenuItem", "Окно 3", HStatisticTabCtrlEx.TYPE_TAB.FLOAT));
            m_dictAddingTabs.Add((int)ID_ADDING_TAB.CUSTOM_2X2_4, new ADDING_TAB("выборОбъектывыборОбъекты22-4-ToolStripMenuItem", "Окно 4", HStatisticTabCtrlEx.TYPE_TAB.FLOAT));
            m_dictAddingTabs.Add((int)ID_ADDING_TAB.CUSTOM_2X3_1, new ADDING_TAB("выборОбъектывыборОбъекты23-1-ToolStripMenuItem", "Окно 1", HStatisticTabCtrlEx.TYPE_TAB.FLOAT));            
            m_dictAddingTabs.Add((int)ID_ADDING_TAB.CUSTOM_2X3_2, new ADDING_TAB("выборОбъектывыборОбъекты23-2-ToolStripMenuItem", "Окно 2", HStatisticTabCtrlEx.TYPE_TAB.FLOAT));
            m_dictAddingTabs.Add((int)ID_ADDING_TAB.CUSTOM_2X3_3, new ADDING_TAB("выборОбъектывыборОбъекты23-3-ToolStripMenuItem", "Окно 3", HStatisticTabCtrlEx.TYPE_TAB.FLOAT));
            m_dictAddingTabs.Add((int)ID_ADDING_TAB.CUSTOM_2X3_4, new ADDING_TAB("выборОбъектывыборОбъекты23-4-ToolStripMenuItem", "Окно 4", HStatisticTabCtrlEx.TYPE_TAB.FLOAT));
            m_dictAddingTabs.Add((int)ID_ADDING_TAB.CUR_POWER, new ADDING_TAB("значенияТекущаяМощностьГТПгТЭЦснToolStripMenuItem", "Значения текущей мощности ГТПг, ТЭЦсн", HClassLibrary.HTabCtrlEx.TYPE_TAB.FLOAT));
            m_dictAddingTabs.Add((int)ID_ADDING_TAB.TM_SN_POWER, new ADDING_TAB("значенияТекущаяМощностьТЭЦгТЭЦснToolStripMenuItem", "Значения текущей мощности ТЭЦг, ТЭЦсн", HClassLibrary.HTabCtrlEx.TYPE_TAB.FLOAT));
            m_dictAddingTabs.Add((int)ID_ADDING_TAB.MONITOR_LAST_MINUTES, new ADDING_TAB("мониторингПоследняяМинутаЧасToolStripMenuItem", "Мониторинг - последняя минута часа", HClassLibrary.HTabCtrlEx.TYPE_TAB.FLOAT));
            m_dictAddingTabs.Add((int)ID_ADDING_TAB.SOBSTV_NYZHDY, new ADDING_TAB("собственныеНуждыToolStripMenuItem", "Собственные нужды СОТИАССО", HClassLibrary.HTabCtrlEx.TYPE_TAB.FLOAT));
            m_dictAddingTabs.Add((int)ID_ADDING_TAB.SOBSTV_NYZHDY_NEW, new ADDING_TAB("собственныеНуждыНовToolStripMenuItem", "Собственные нужды АИСКУЭ", HClassLibrary.HTabCtrlEx.TYPE_TAB.FLOAT));
            m_dictAddingTabs.Add((int)ID_ADDING_TAB.DATETIMESYNC_SOURCE_DATA, new ADDING_TAB("рассинхронизацияДатаВремяСерверБДToolStripMenuItem", "Рассинхронизация даты/времени серверов БД", HClassLibrary.HTabCtrlEx.TYPE_TAB.FIXED));
            m_dictAddingTabs.Add((int)ID_ADDING_TAB.DIAGNOSTIC, new ADDING_TAB("ДиагностикаToolStripMenuItem", "Диагностика", HClassLibrary.HTabCtrlEx.TYPE_TAB.FIXED));
            m_dictAddingTabs.Add((int)ID_ADDING_TAB.ANALYZER, new ADDING_TAB("ПросмотрЖурналаToolStripMenuItem", "Журнал событий", HClassLibrary.HTabCtrlEx.TYPE_TAB.FIXED));
            #region KhryapinAN 2017-06
            m_dictAddingTabs.Add((int)ID_ADDING_TAB.SOTIASSO_HOUR, new ADDING_TAB("значенияСОТИАССОЧасToolStripMenuItem", "Значения СОТИАССО-час", HClassLibrary.HTabCtrlEx.TYPE_TAB.FIXED));            
            m_dictAddingTabs.Add((int)ID_ADDING_TAB.AIISKUE_SOTIASSO_DAY, new ADDING_TAB("значенияАИИСКУЭСОТИАССОСуткиToolStripMenuItem", "Значения АИСКУЭ+СОТИАССО-сутки", HClassLibrary.HTabCtrlEx.TYPE_TAB.FIXED));
            #endregion
            m_dictAddingTabs.Add((int)ID_ADDING_TAB.VZLET_TDIRECT, new ADDING_TAB("значенияВзлетТпрямаяToolStripMenuItem", "Расчет теплосети", HClassLibrary.HTabCtrlEx.TYPE_TAB.FLOAT));
            m_dictAddingTabs.Add((int)ID_ADDING_TAB.TEC_Component, new ADDING_TAB("СоставТЭЦToolStripMenuItem", "Изменить состав ТЭЦ", HClassLibrary.HTabCtrlEx.TYPE_TAB.FIXED));
            m_dictAddingTabs.Add((int)ID_ADDING_TAB.USERS, new ADDING_TAB("изментьСоставПользовательToolStripMenuItem", "Изменить состав пользователей", HClassLibrary.HTabCtrlEx.TYPE_TAB.FIXED));

            this.настройкиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.сменитьРежимToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.настройкиСоединенияБДКонфToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.настройкиСоединенияБДИсточникToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.администрированиеToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();


            this.изменитьПарольДиспетчераToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.изменитьПарольАдминистратораToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            //this.рассинхронизацияДатаВремяСерверБДToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            //this.ДиагностикаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.изменитьПарольНССToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.изменитьПарольЛКДиспетчераToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            //this.изментьСоставТЭЦГТПЩУToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.параметрыToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.параметрыПриложенияToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.параметрыТГБийскToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.оПрограммеToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tclTecViews = new HStatisticTabCtrlEx (); //System.Windows.Forms.TabControl();
            this.lblLabel = new System.Windows.Forms.Label();
            this.MainMenuStrip.SuspendLayout();
            //this.m_ContextMenuStripListTecViews.SuspendLayout ();
            this.SuspendLayout();
            // 
            // MainMenuStrip
            // 
            this.MainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.файлToolStripMenuItem
                , this.видToolStripMenuItem
                , this.настройкиToolStripMenuItem
                , this.оПрограммеToolStripMenuItem
            });
            this.MainMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.MainMenuStrip.Name = "MainMenuStrip";
            this.MainMenuStrip.Size = new System.Drawing.Size(982, 24);
            this.MainMenuStrip.TabIndex = 2;
            this.MainMenuStrip.Text = "Главное меню";
            this.MainMenuStrip.MenuActivate += new System.EventHandler(menuStrip_MenuActivate);
            this.MainMenuStrip.MenuDeactivate += new System.EventHandler(menuStrip_MenuDeactivate);
            //this.MainMenuStrip.BackColorChanged += mainMenuStrip_BackColorChanged;
            // 
            // ContextMenuStrip
            // 
            this.ContextMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.ContextMenuStrip.Name = "ContextMenuStripListTecViews";
            this.ContextMenuStrip.Text = "Контекстное меню";
            // 
            // файлToolStripMenuItem
            // 
            this.файлToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.файлПрофильToolStripMenuItem
                , new System.Windows.Forms.ToolStripSeparator()
                , this.выходToolStripMenuItem});
            this.файлToolStripMenuItem.Name = "файлToolStripMenuItem";
            this.файлToolStripMenuItem.Size = new System.Drawing.Size(45, 20);
            this.файлToolStripMenuItem.Text = "Файл";
            // 
            // файлПрофильЗагрузитьToolStripMenuItem
            // 
            this.файлПрофильЗагрузитьToolStripMenuItem.Name = "файлПрофильЗагрузитьToolStripMenuItem";
            this.файлПрофильЗагрузитьToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.файлПрофильЗагрузитьToolStripMenuItem.Text = "Загрузить";
            this.файлПрофильЗагрузитьToolStripMenuItem.Click += new System.EventHandler(this.файлПрофильЗагрузитьToolStripMenuItem_Click);
            this.файлПрофильЗагрузитьToolStripMenuItem.Enabled = false;
            // 
            // файлПрофильСохранитьToolStripMenuItem
            // 
            this.файлПрофильСохранитьToolStripMenuItem.Name = "сохранитьПрофильToolStripMenuItem";
            this.файлПрофильСохранитьToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.файлПрофильСохранитьToolStripMenuItem.Text = "Сохранить";
            this.файлПрофильСохранитьToolStripMenuItem.Click += new System.EventHandler(this.файлПрофильСохранитьToolStripMenuItem_Click);
            this.файлПрофильСохранитьToolStripMenuItem.Enabled = false;
            // 
            // файлПрофильАвтоЗагрузитьСохранитьToolStripMenuItem
            // 
            this.файлПрофильАвтоЗагрузитьСохранитьToolStripMenuItem.Name = "файлПрофильАвтоЗагрузитьСохранитьToolStripMenuItem";
            this.файлПрофильАвтоЗагрузитьСохранитьToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.файлПрофильАвтоЗагрузитьСохранитьToolStripMenuItem.Text = "Авто Загружать/Сохранять";
            this.файлПрофильАвтоЗагрузитьСохранитьToolStripMenuItem.CheckOnClick = true;
            this.файлПрофильАвтоЗагрузитьСохранитьToolStripMenuItem.Checked = false;
            this.файлПрофильАвтоЗагрузитьСохранитьToolStripMenuItem.CheckedChanged += new System.EventHandler(this.файлПрофильАвтоЗагрузитьСохранить_CheckedChanged);
            this.файлПрофильАвтоЗагрузитьСохранитьToolStripMenuItem.Enabled = false;

            this.файлПрофильToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.файлПрофильЗагрузитьToolStripMenuItem
                , this.файлПрофильСохранитьToolStripMenuItem
                , new System.Windows.Forms.ToolStripSeparator()
                , this.файлПрофильАвтоЗагрузитьСохранитьToolStripMenuItem});
            this.файлПрофильToolStripMenuItem.Name = "файлПрофильToolStripMenuItem";
            this.файлПрофильToolStripMenuItem.Size = new System.Drawing.Size(45, 20);
            this.файлПрофильToolStripMenuItem.Text = "Профиль";

            // 
            // выходToolStripMenuItem
            // 
            this.выходToolStripMenuItem.Name = "выходToolStripMenuItem";
            this.выходToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.выходToolStripMenuItem.Text = "Выход";
            this.выходToolStripMenuItem.Click += new System.EventHandler(this.выходToolStripMenuItem_Click);
            // 
            // видToolStripMenuItem
            // 
            this.видToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.панельГрафическихToolStripMenuItem
                , new System.Windows.Forms.ToolStripSeparator()
                , new System.Windows.Forms.ToolStripMenuItem (@"Объекты по выбору (2X2)", null, new System.Windows.Forms.ToolStripItem [] {
                    m_dictAddingTabs[(int)m_arIdCustomTabs[(int)INDEX_CUSTOM_TAB.TAB_2X2][0]].menuItem
                    , m_dictAddingTabs[(int)m_arIdCustomTabs[(int)INDEX_CUSTOM_TAB.TAB_2X2][1]].menuItem
                    , m_dictAddingTabs[(int)m_arIdCustomTabs[(int)INDEX_CUSTOM_TAB.TAB_2X2][2]].menuItem
                    , m_dictAddingTabs[(int)m_arIdCustomTabs[(int)INDEX_CUSTOM_TAB.TAB_2X2][3]].menuItem
                }) 
                , new System.Windows.Forms.ToolStripMenuItem (@"Объекты по выбору (2X3)", null, new System.Windows.Forms.ToolStripItem [] {
                    m_dictAddingTabs[(int)m_arIdCustomTabs[(int)INDEX_CUSTOM_TAB.TAB_2X3][0]].menuItem
                    , m_dictAddingTabs[(int)m_arIdCustomTabs[(int)INDEX_CUSTOM_TAB.TAB_2X3][1]].menuItem
                    , m_dictAddingTabs[(int)m_arIdCustomTabs[(int)INDEX_CUSTOM_TAB.TAB_2X3][2]].menuItem
                    , m_dictAddingTabs[(int)m_arIdCustomTabs[(int)INDEX_CUSTOM_TAB.TAB_2X3][3]].menuItem
                })
                , new System.Windows.Forms.ToolStripSeparator()
                , m_dictAddingTabs[(int)ID_ADDING_TAB.CUR_POWER].menuItem
                , m_dictAddingTabs[(int)ID_ADDING_TAB.TM_SN_POWER].menuItem
                , m_dictAddingTabs[(int)ID_ADDING_TAB.MONITOR_LAST_MINUTES].menuItem
                , new System.Windows.Forms.ToolStripSeparator()
                , this.m_dictAddingTabs[(int)ID_ADDING_TAB.SOBSTV_NYZHDY_NEW].menuItem
                , this.m_dictAddingTabs[(int)ID_ADDING_TAB.SOBSTV_NYZHDY].menuItem

            //    this.администрированиеToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            //    this.настройкиСоединенияБДКонфToolStripMenuItem,
            //    this.настройкиСоединенияБДИсточникToolStripMenuItem,
            //    new System.Windows.Forms.ToolStripSeparator(),
            //    this.m_dictAddingTabs[(int)ID_ADDING_TAB.DIAGNOSTIC].menuItem,
            //    this.m_dictAddingTabs[(int)ID_ADDING_TAB.DATETIMESYNC_SOURCE_DATA].menuItem,
            //    this.m_dictAddingTabs[(int)ID_ADDING_TAB.ANALYZER].menuItem,
            //    new System.Windows.Forms.ToolStripSeparator(),
            //    this.изменитьПарольДиспетчераToolStripMenuItem,
            //    this.изменитьПарольАдминистратораToolStripMenuItem,
            //    this.изменитьПарольНССToolStripMenuItem,
            //    this.изменитьПарольЛКДиспетчераToolStripMenuItem,
            //    new System.Windows.Forms.ToolStripSeparator(),
            //    this.m_dictAddingTabs[(int)ID_ADDING_TAB.TEC_Component].menuItem,
            //    this.m_dictAddingTabs[(int)ID_ADDING_TAB.USERS].menuItem
            //});

            //this.администрированиеToolStripMenuItem.Name = "администрированиеToolStripMenuItem";
            //this.администрированиеToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            //this.администрированиеToolStripMenuItem.Text = "Администрирование";


                //, m_dictAddingTabs[(int)ID_ADDING_TAB.SOBSTV_NYZHDY].menuItem
                #region KhryapinAN 2017-06
                , new System.Windows.Forms.ToolStripSeparator()
                , m_dictAddingTabs[(int)ID_ADDING_TAB.SOTIASSO_HOUR].menuItem                
                , m_dictAddingTabs[(int)ID_ADDING_TAB.AIISKUE_SOTIASSO_DAY].menuItem
                , new System.Windows.Forms.ToolStripSeparator()
                #endregion
                , m_dictAddingTabs[(int)ID_ADDING_TAB.VZLET_TDIRECT].menuItem
            });
            this.видToolStripMenuItem.Name = "видToolStripMenuItem";
            this.видToolStripMenuItem.Size = new System.Drawing.Size(38, 20);
            this.видToolStripMenuItem.Text = "Вид";
            this.видToolStripMenuItem.Enabled = false;
            // 
            // панельГрафическихToolStripMenuItem
            // 
            this.панельГрафическихToolStripMenuItem.CheckOnClick = true;
            this.панельГрафическихToolStripMenuItem.Name = "панельГрафическихToolStripMenuItem";
            this.панельГрафическихToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.панельГрафическихToolStripMenuItem.Text = "Панель графических параметров...";
            this.панельГрафическихToolStripMenuItem.CheckedChanged += new System.EventHandler(this.панельГрафическихToolStripMenuItem_CheckedChanged);
            // 
            // выборОбъекты22ToolStripMenuItem
            // 
            this.m_dictAddingTabs[(int)m_arIdCustomTabs[(int)INDEX_CUSTOM_TAB.TAB_2X2][0]].menuItem.CheckedChanged += new System.EventHandler(this.выборОбъекты22ToolStripMenuItem_CheckedChanged);
            this.m_dictAddingTabs[(int)m_arIdCustomTabs[(int)INDEX_CUSTOM_TAB.TAB_2X2][1]].menuItem.CheckedChanged += new System.EventHandler(this.выборОбъекты22ToolStripMenuItem_CheckedChanged);
            this.m_dictAddingTabs[(int)m_arIdCustomTabs[(int)INDEX_CUSTOM_TAB.TAB_2X2][2]].menuItem.CheckedChanged += new System.EventHandler(this.выборОбъекты22ToolStripMenuItem_CheckedChanged);
            this.m_dictAddingTabs[(int)m_arIdCustomTabs[(int)INDEX_CUSTOM_TAB.TAB_2X2][3]].menuItem.CheckedChanged += new System.EventHandler(this.выборОбъекты22ToolStripMenuItem_CheckedChanged);
            // 
            // выборОбъекты23ToolStripMenuItem
            // 
            this.m_dictAddingTabs[(int)m_arIdCustomTabs[(int)INDEX_CUSTOM_TAB.TAB_2X3][0]].menuItem.CheckedChanged += new System.EventHandler(this.выборОбъекты23ToolStripMenuItem_CheckedChanged);
            this.m_dictAddingTabs[(int)m_arIdCustomTabs[(int)INDEX_CUSTOM_TAB.TAB_2X3][1]].menuItem.CheckedChanged += new System.EventHandler(this.выборОбъекты23ToolStripMenuItem_CheckedChanged);
            this.m_dictAddingTabs[(int)m_arIdCustomTabs[(int)INDEX_CUSTOM_TAB.TAB_2X3][2]].menuItem.CheckedChanged += new System.EventHandler(this.выборОбъекты23ToolStripMenuItem_CheckedChanged);
            this.m_dictAddingTabs[(int)m_arIdCustomTabs[(int)INDEX_CUSTOM_TAB.TAB_2X3][3]].menuItem.CheckedChanged += new System.EventHandler(this.выборОбъекты23ToolStripMenuItem_CheckedChanged);
            // 
            // значенияТекущаяМощностьГТПгТЭЦснToolStripMenuItem
            // 
            this.m_dictAddingTabs[(int)ID_ADDING_TAB.CUR_POWER].menuItem.CheckedChanged += new System.EventHandler(this.значенияТекущаяМощностьГТПгТЭЦснToolStripMenuItem_CheckedChanged);
            // 
            // значенияТекущаяМощностьТЭЦгТЭЦснToolStripMenuItem
            // 
            this.m_dictAddingTabs[(int)ID_ADDING_TAB.TM_SN_POWER].menuItem.CheckedChanged += new System.EventHandler(this.значенияТекущаяМощностьТЭЦгТЭЦснToolStripMenuItem_CheckedChanged);
            // 
            // мониторингПоследняяМинутаЧасToolStripMenuItem
            // 
            this.m_dictAddingTabs[(int)ID_ADDING_TAB.MONITOR_LAST_MINUTES].menuItem.CheckedChanged += new System.EventHandler(this.мониторингПоследняяМинутаЧасToolStripMenuItem_CheckedChanged);
            // 
            // собственныеНуждыToolStripMenuItem
            // 
            this.m_dictAddingTabs[(int)ID_ADDING_TAB.SOBSTV_NYZHDY].menuItem.CheckedChanged += new System.EventHandler(this.собственныеНуждыToolStripMenuItem_CheckedChanged);
            // 
            // собственныеНуждыНовToolStripMenuItem
            // 
            this.m_dictAddingTabs[(int)ID_ADDING_TAB.SOBSTV_NYZHDY_NEW].menuItem.CheckedChanged += new System.EventHandler(this.собственныеНуждыНовToolStripMenuItem_CheckedChanged);
            // 
            // рассинхронизацияДатаВремяСерверБДToolStripMenuItem
            // 
            this.m_dictAddingTabs[(int)ID_ADDING_TAB.DATETIMESYNC_SOURCE_DATA].menuItem.CheckedChanged += new System.EventHandler(this.рассинхронизацияДатаВремяСерверБДToolStripMenuItem_CheckedChanged);
            //
            //ДиагностикаToolStripMenuItem_CheckedChanged
            //
            this.m_dictAddingTabs[(int)ID_ADDING_TAB.DIAGNOSTIC].menuItem.CheckedChanged += new System.EventHandler(this.ДиагностикаToolStripMenuItem_CheckedChanged);
            //
            //ПросмотрЖурналаToolStripMenuItem_CheckedChanged
            //
            this.m_dictAddingTabs[(int)ID_ADDING_TAB.ANALYZER].menuItem.CheckedChanged += new System.EventHandler(this.ПросмотрЖурналаToolStripMenuItem_CheckedChanged);
            //
            //СоставТЭЦToolStripMenuItem_CheckedChanged
            //
            this.m_dictAddingTabs[(int)ID_ADDING_TAB.TEC_Component].menuItem.CheckedChanged += new System.EventHandler(this.СоставТЭЦToolStripMenuItem_CheckedChanged);
            //
            //изментьСоставПользовательStripMenuItem_CheckedChanged
            //
            this.m_dictAddingTabs[(int)ID_ADDING_TAB.USERS].menuItem.CheckedChanged += new System.EventHandler(this.изментьСоставПользовательToolStripMenuItem_CheckedChanged);

            #region KhryapinAN, 2017-06
            // 
            // значенияСОТИАССОЧасToolStripMenuItem
            // 
            this.m_dictAddingTabs[(int)ID_ADDING_TAB.SOTIASSO_HOUR].menuItem.CheckedChanged += new System.EventHandler(this.значенияСОТИАССОЧасToolStripMenuItem_CheckedChanged);            
            // 
            // значенияСОТИАССОСуткиToolStripMenuItem
            // 
            this.m_dictAddingTabs[(int)ID_ADDING_TAB.AIISKUE_SOTIASSO_DAY].menuItem.CheckedChanged += new System.EventHandler(this.значенияАИИСКУЭСОТИАССОСуткиToolStripMenuItem_CheckedChanged);
            #endregion
            // 
            // значенияВзлетТпрямаяToolStripMenuItem
            // 
            this.m_dictAddingTabs[(int)ID_ADDING_TAB.VZLET_TDIRECT].menuItem.CheckedChanged += new System.EventHandler(this.значенияВзлетТпрямаяToolStripMenuItem_CheckedChanged);            

            // 
            // настройкиToolStripMenuItem
            // 
            this.настройкиToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.сменитьРежимToolStripMenuItem,
            this.администрированиеToolStripMenuItem,
            this.параметрыToolStripMenuItem});
            this.настройкиToolStripMenuItem.Name = "настройкиToolStripMenuItem";
            this.настройкиToolStripMenuItem.Size = new System.Drawing.Size(73, 20);
            this.настройкиToolStripMenuItem.Text = "Настройки";
            // 
            // сменитьРежимToolStripMenuItem
            // 
            this.сменитьРежимToolStripMenuItem.Name = "сменитьРежимToolStripMenuItem";
            this.сменитьРежимToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.сменитьРежимToolStripMenuItem.Text = "Сменить режим...";
            this.сменитьРежимToolStripMenuItem.Click += new System.EventHandler(this.сменитьРежимToolStripMenuItem_Click);
            this.сменитьРежимToolStripMenuItem.Enabled = false;
            this.сменитьРежимToolStripMenuItem.EnabledChanged += new System.EventHandler(сменитьРежимToolStripMenuItem_EnabledChanged);
            // 
            // настройкиСоединенияБДКонфToolStripMenuItem
            // 
            this.настройкиСоединенияБДКонфToolStripMenuItem.Name = "настройкиСоединенияБДКонфToolStripMenuItem";
            this.настройкиСоединенияБДКонфToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.настройкиСоединенияБДКонфToolStripMenuItem.Text = "Настройки соединения БД конфигурации...";
            this.настройкиСоединенияБДКонфToolStripMenuItem.Click += new System.EventHandler(this.настройкиСоединенияБДКонфToolStripMenuItem_Click);
            this.настройкиСоединенияБДКонфToolStripMenuItem.Enabled = false;
            // 
            // настройкиСоединенияБДИсточникToolStripMenuItem
            // 
            this.настройкиСоединенияБДИсточникToolStripMenuItem.Name = "настройкиСоединенияБДИсточникToolStripMenuItem";
            this.настройкиСоединенияБДИсточникToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.настройкиСоединенияБДИсточникToolStripMenuItem.Text = "Настройки соединения БД источников данных...";
            this.настройкиСоединенияБДИсточникToolStripMenuItem.Click += new System.EventHandler(this.настройкиСоединенияБДИсточникToolStripMenuItem_Click);
            настройкиСоединенияБДИсточникToolStripMenuItem.Enabled = false;
            // 
            // администрированиеToolStripMenuItem
            // 
            this.администрированиеToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.настройкиСоединенияБДКонфToolStripMenuItem,            
                this.настройкиСоединенияБДИсточникToolStripMenuItem,
                new System.Windows.Forms.ToolStripSeparator(),
                this.m_dictAddingTabs[(int)ID_ADDING_TAB.DIAGNOSTIC].menuItem,
                this.m_dictAddingTabs[(int)ID_ADDING_TAB.DATETIMESYNC_SOURCE_DATA].menuItem,
                this.m_dictAddingTabs[(int)ID_ADDING_TAB.ANALYZER].menuItem,
                new System.Windows.Forms.ToolStripSeparator(),
                this.изменитьПарольДиспетчераToolStripMenuItem,
                this.изменитьПарольАдминистратораToolStripMenuItem,
                this.изменитьПарольНССToolStripMenuItem,
                this.изменитьПарольЛКДиспетчераToolStripMenuItem,
                new System.Windows.Forms.ToolStripSeparator(),
                this.m_dictAddingTabs[(int)ID_ADDING_TAB.TEC_Component].menuItem,
                this.m_dictAddingTabs[(int)ID_ADDING_TAB.USERS].menuItem
            });

            this.администрированиеToolStripMenuItem.Name = "администрированиеToolStripMenuItem";
            this.администрированиеToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.администрированиеToolStripMenuItem.Text = "Администрирование";
            // 
            // изменитьПарольДиспетчераToolStripMenuItem
            // 
            this.изменитьПарольДиспетчераToolStripMenuItem.Name = "изменитьПарольДиспетчераToolStripMenuItem";
            this.изменитьПарольДиспетчераToolStripMenuItem.Size = new System.Drawing.Size(303, 22);
            this.изменитьПарольДиспетчераToolStripMenuItem.Text = "Изменить пароль коммерческого диспетчера...";
            this.изменитьПарольДиспетчераToolStripMenuItem.Click += new System.EventHandler(this.изменитьПарольКоммерческогоДиспетчераToolStripMenuItem_Click);
            изменитьПарольДиспетчераToolStripMenuItem.Enabled = false;
            // 
            // изменитьПарольАдминистратораToolStripMenuItem
            // 
            this.изменитьПарольАдминистратораToolStripMenuItem.Name = "изменитьПарольАдминистратораToolStripMenuItem";
            this.изменитьПарольАдминистратораToolStripMenuItem.Size = new System.Drawing.Size(303, 22);
            this.изменитьПарольАдминистратораToolStripMenuItem.Text = "Изменить пароль администратора...";
            this.изменитьПарольАдминистратораToolStripMenuItem.Click += new System.EventHandler(this.изменитьПарольАдминистратораToolStripMenuItem_Click);
            изменитьПарольАдминистратораToolStripMenuItem.Enabled = false;
            // 
            // toolStripMenuItemИзменитьПарольНСС
            // 
            this.изменитьПарольНССToolStripMenuItem.Name = "изменитьПарольНССToolStripMenuItem";
            this.изменитьПарольНССToolStripMenuItem.Size = new System.Drawing.Size(303, 22);
            this.изменитьПарольНССToolStripMenuItem.Text = "Изменить пароль начальника смены станции...";
            this.изменитьПарольНССToolStripMenuItem.Click += new System.EventHandler(this.изменитьПарольНССToolStripMenuItem_Click);
            изменитьПарольНССToolStripMenuItem.Enabled = false;
            // 
            // toolStripMenuItemИзменитьПарольЛКДиспетчера
            // 
            this.изменитьПарольЛКДиспетчераToolStripMenuItem.Name = "изменитьПарольЛКДиспетчераToolStripMenuItem";
            this.изменитьПарольЛКДиспетчераToolStripMenuItem.Size = new System.Drawing.Size(303, 22);
            this.изменитьПарольЛКДиспетчераToolStripMenuItem.Text = "Изменить пароль ЛК-диспетчера...";
            this.изменитьПарольЛКДиспетчераToolStripMenuItem.Click += new System.EventHandler(this.изменитьПарольЛКДиспетчераToolStripMenuItem_Click);
            изменитьПарольЛКДиспетчераToolStripMenuItem.Enabled = false;            
            
            // 
            // изментьСоставТЭЦГТПЩУToolStripMenuItem
            // 
            //this.изментьСоставТЭЦГТПЩУToolStripMenuItem.Name = "изментьСоставТЭЦГТПЩУToolStripMenuItem";
            //this.изментьСоставТЭЦГТПЩУToolStripMenuItem.Size = new System.Drawing.Size(303, 22);
            //this.изментьСоставТЭЦГТПЩУToolStripMenuItem.Text = "Изменть состав ТЭЦ (ГТП, ЩУ)...";
            //this.изментьСоставТЭЦГТПЩУToolStripMenuItem.Click += new System.EventHandler(this.изментьСоставТЭЦГТПЩУToolStripMenuItem_Click);
            //изментьСоставТЭЦГТПЩУToolStripMenuItem.Enabled = false;
            
            // 
            // параметрыToolStripMenuItem
            // 
            this.параметрыToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.параметрыПриложенияToolStripMenuItem,
            this.параметрыТГБийскToolStripMenuItem});
            this.параметрыToolStripMenuItem.Name = "параметрыToolStripMenuItem";
            this.параметрыToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.параметрыToolStripMenuItem.Text = "Параметры";
            //параметрыToolStripMenuItem.Enabled = false;
            // 
            // параметрыПриложенияToolStripMenuItem
            // 
            this.параметрыПриложенияToolStripMenuItem.Name = "параметрыПриложенияToolStripMenuItem";
            this.параметрыПриложенияToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.параметрыПриложенияToolStripMenuItem.Text = "Параметры приложения...";
            this.параметрыПриложенияToolStripMenuItem.Click += new System.EventHandler(this.параметрыПриложенияToolStripMenuItem_Click);
            this.параметрыПриложенияToolStripMenuItem.Enabled = false;
            // 
            // параметрыТГБийскToolStripMenuItem
            // 
            this.параметрыТГБийскToolStripMenuItem.Name = "параметрыТГБийскToolStripMenuItem";
            this.параметрыТГБийскToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.параметрыТГБийскToolStripMenuItem.Text = "Параметры ТГ Бийск...";
            this.параметрыТГБийскToolStripMenuItem.Visible = false;
            this.параметрыТГБийскToolStripMenuItem.Click += new System.EventHandler(this.параметрыТГБийскToolStripMenuItem_Click);
            // 
            // оПрограммеToolStripMenuItem
            // 
            this.оПрограммеToolStripMenuItem.Name = "оПрограммеToolStripMenuItem";
            this.оПрограммеToolStripMenuItem.Size = new System.Drawing.Size(83, 20);
            this.оПрограммеToolStripMenuItem.Text = "О программе";
            this.оПрограммеToolStripMenuItem.Click += new System.EventHandler(this.оПрограммеToolStripMenuItem_Click);
            // 
            // tclTecViews
            // 
            this.tclTecViews.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tclTecViews.Location = new System.Drawing.Point(0, 24);
            this.tclTecViews.Name = "tclTecViews";
            //this.tclTecViews.SelectedIndex = 0;
            this.tclTecViews.Size = new System.Drawing.Size(982, 735);
            this.tclTecViews.TabIndex = 3;
            this.tclTecViews.SelectedIndexChanged += new System.EventHandler(this.tclTecViews_SelectedIndexChanged);
            // 
            // lblLabel
            // 
            this.lblLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLabel.AutoSize = true;
            this.lblLabel.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblLabel.Location = new System.Drawing.Point(772, 4);
            this.lblLabel.Name = "lblLabel";
            this.lblLabel.Size = new System.Drawing.Size(206, 17);
            this.lblLabel.TabIndex = 5;
            this.lblLabel.Text = "ОА \"СибЭко\"";
            this.lblLabel.Visible = false;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(982, 784);
            this.Controls.Add(this.lblLabel);
            this.Controls.Add(this.tclTecViews);
            this.Controls.Add(this.MainMenuStrip);
            //this.Controls.Add(this.m_ContextMenuStripListTecViews);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.MainMenuStrip;
            //this.ContextMenuStrip = this.m_ContextMenuStripListTecViews;
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Статистика";
            this.Activated += new System.EventHandler(this.FormMain_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.Load += new System.EventHandler(FormMain_FormLoad);
            this.ContextMenuStrip.Opened += new System.EventHandler(menuStrip_MenuActivate);
            this.ContextMenuStrip.Closed += new System.Windows.Forms.ToolStripDropDownClosedEventHandler(contextMenuStrip_Closed);
            this.MainMenuStrip.ResumeLayout(false);
            this.MainMenuStrip.PerformLayout();
            this.ContextMenuStrip.ResumeLayout(false);
            this.ContextMenuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        ///// <summary>
        ///// Обработчик события - изменение цвета фона
        ///// </summary>
        ///// <param name="sender">Объект инициировавший событие</param>
        ///// <param name="e">Аргумент события</param>
        //private void mainMenuStrip_BackColorChanged (object sender, System.EventArgs e)
        //{
        //    for (int i = 0; i < (sender as MenuStrip).Items.Count; i++)                
        //        changeColorToolStripMenuItem ((sender as MenuStrip).Items [i] as ToolStripMenuItem);
        //}


        //private void changeColorToolStripMenuItem (ToolStripMenuItem item)
        //{
        //    item.BackColor =
        //        //item.GetCurrentParent().BackColor ???изменение произошло, но не применено
        //        PanelStatistic._BackColor
        //        ;

        //    if (item.HasDropDownItems == true) {
        //        for (int i = 0; i < item.DropDownItems.Count; i++)
        //            if (item.DropDownItems [i] is ToolStripMenuItem)
        //                changeColorToolStripMenuItem (item.DropDownItems [i] as ToolStripMenuItem);
        //            else if (item.DropDownItems [i] is ToolStripSeparator)
        //                (item.DropDownItems [i] as ToolStripSeparator).BackColor = PanelStatistic._BackColor;
        //            else
        //                ;
        //    } else
        //        ;
        //}
        #endregion

        //private System.Windows.Forms.ContextMenuStrip m_ContextMenuStripListTecViews;
        private System.Windows.Forms.ToolStripMenuItem файлToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem файлПрофильToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem файлПрофильЗагрузитьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem файлПрофильСохранитьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem файлПрофильАвтоЗагрузитьСохранитьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem выходToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem настройкиToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem сменитьРежимToolStripMenuItem;
        //private System.Windows.Forms.ToolStripMenuItem ДиагностикаToolStripMenuItem;
        //private System.Windows.Forms.ToolStripMenuItem рассинхронизацияДатаВремяСерверБДToolStripMenuItem;
        private HStatisticTabCtrlEx /*System.Windows.Forms.TabControl*/ tclTecViews;
        private System.Windows.Forms.ToolStripMenuItem настройкиСоединенияБДКонфToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem настройкиСоединенияБДИсточникToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem администрированиеToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem изменитьПарольДиспетчераToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem изменитьПарольАдминистратораToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem изменитьПарольНССToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem изменитьПарольЛКДиспетчераToolStripMenuItem;
        //private System.Windows.Forms.ToolStripMenuItem изментьСоставТЭЦГТПЩУToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem оПрограммеToolStripMenuItem;
        private System.Windows.Forms.Label lblLabel;
        private System.Windows.Forms.ToolStripMenuItem видToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem панельГрафическихToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem параметрыToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem параметрыПриложенияToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem параметрыТГБийскToolStripMenuItem;
    }
}

