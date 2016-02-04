using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StatisticTrans;

namespace StatisticTrans
{
    class ComponentTesting
    {
        /// <summary>
        /// 
        /// </summary>
        private bool bflag;

        /// <summary>
        /// 
        /// </summary>
        private int countIt=0;

        /// <summary>
        /// Текущая иттерация
        /// </summary>
        private int currentIter;

        /// <summary>
        /// Кол-во компонентов
        /// </summary>
        private string[] Iters;

        /// <summary>
        /// Счетчик нового дня
        /// </summary>
        public bool NextDay = false;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="size">размер массива</param>
        public ComponentTesting(int size)
        {
            Iters = new string[size];
            currentIter = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        public void PushIter(string nameElem)
        {
            if (GetNum() == countIt)
            {
                    ClearStck();
            }
            else
                Iters[currentIter] = nameElem;
            currentIter++;
            countIt++;
            CounterSuccessfulDownload();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private int GetNum()
        {
            return Iters.Length;
        }

        /// <summary>
        /// 
        /// </summary>
        public void PopIter()
        {
            currentIter--;
            CounterSuccessfulDownload();
            countIt++;
        }

        /// <summary>
        /// отчет об итерациях
        /// </summary>
        private void CounterSuccessfulDownload()
        {
            FormMainTrans.m_labelTime.Invoke(new Action(() => FormMainTrans.m_labelTime.Text = "Время последнего опроса: " + DateTime.Now.ToString() + ";" + " Успешных итераций: " + currentIter + " из " + GetNum() + ""));
            FormMainTrans.m_labelTime.Invoke(new Action(() => FormMainTrans.m_labelTime.Update()));
        }

        /// <summary>
        /// Очистка итераций
        /// </summary>
        private void ClearStck()
        {
            currentIter = 0;
            countIt = 0;
        }
    }
}
