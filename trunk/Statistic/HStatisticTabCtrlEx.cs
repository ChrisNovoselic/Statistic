﻿
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Statistic
{
    partial class FormMain
    {
        //??? требуется перенос кода в HClassLibrary
        /// <summary>
        /// Закрытый класс HTabCtrlEx наследуется от HClassLibrary.HTabCtrlEx
        /// </summary>
        private class HStatisticTabCtrlEx : ASUTP.Control.HTabCtrlEx {
            private Color _backColor;

            private SolidBrush _backBrush;

            public HStatisticTabCtrlEx ()
                : base ()
            {
                this.DrawMode = TabDrawMode.OwnerDrawFixed;

                _backBrush = new SolidBrush (SystemColors.Control);
            }

            /// <summary>
            /// Открытый метод IndexOfName (индекс имени) принимает строковый аргумент, возвращает значение типа int
            /// </summary>
            /// <param name="name">Имя</param>
            /// <returns>Число</returns>
            public int IndexOfName (string name)
            {
                int iRes = -1;

                int i = -1;
                //?? что-то с вкладками связано
                for (i = 0; i < TabCount; i++)
                    if (TabPages [i].Name.Trim ().Equals (name.Trim ()) == true) {
                        iRes = i;

                        break;
                    } else
                        ;

                return iRes;
            }

            public void FormMain_BackColorChanged (object sender, EventArgs ev)
            {
                _backColor = (sender as Form).BackColor;

                _backBrush = new SolidBrush (_backColor);

                foreach (TabPage tab in TabPages)
                    tab.BackColor = _backColor;
            }

            protected override void OnDrawItem (DrawItemEventArgs e)
            {
                // фон заголовка вкладки
                e.Graphics.FillRectangle (_backBrush, e.Bounds);
                // свободное место рядом с заголовками вкладок 
                RectangleF rectLastTab = this.GetTabRect (this.TabPages.Count - 1)
                    , rectSpacing = new RectangleF (rectLastTab.X + rectLastTab.Width
                        , rectLastTab.Y - 5
                        , this.Width - (rectLastTab.X + rectLastTab.Width) + 5
                        , rectLastTab.Height + 5);
                e.Graphics.FillRectangle (_backBrush, rectSpacing);

                base.OnDrawItem (e);
            }
        }
    }
}