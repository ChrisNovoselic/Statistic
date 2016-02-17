using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StatisticTrans;

namespace StatisticTrans
{
    /// <summary>
    /// 
    /// </summary>
    class ComponentTesting
    {
        /// <summary>
        /// Кол-во пройденных итераций
        /// </summary>
        private int countIt = 0;

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
        /// Успешные итерации
        /// </summary>
        /// <param name="nameElem">имя опрашеваемого компонента</param>
        public void PushIter(string nameElem)
        {
            if (GetNum() == countIt)
                ClearStck();

            Iters[currentIter] = nameElem;
            currentIter++;
            countIt++;
            CounterSuccessfulDownload();
        }

        /// <summary>
        /// Кол-во итераций
        /// </summary>
        /// <returns>кол-во итераций</returns>
        private int GetNum()
        {
            return Iters.Length;
        }

        /// <summary>
        /// Уменьшение счетчика итераций
        /// </summary>
        public void PopIter()
        {
            ErrorIter();
            currentIter--;
            CounterSuccessfulDownload();
            countIt++;
        }

        /// <summary>
        /// отчет об итерациях
        /// </summary>
        private void CounterSuccessfulDownload()
        {
            FormMainTrans.m_labelTime.Invoke(new Action(() => FormMainTrans.m_labelTime.Text = "Время крайнего опроса: "
                + DateTime.Now.ToString() + ";" + " Успешных итераций: " + currentIter + " из " + GetNum() + ";"));
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

        /// <summary>
        /// 
        /// </summary>
        private void ErrorIter()
        {
            FormMainTrans.m_labelTime.Invoke(new Action(() => FormMainTrans.m_labelTime.Text = "Время крайнего опроса: "
               + DateTime.Now.ToString() + ";" + " Успешных итераций: " + currentIter + " из " + GetNum() + ";"
               + "Ошибка на компоненте: " + Iters[currentIter].ToString() + "."));
            FormMainTrans.m_labelTime.Invoke(new Action(() => FormMainTrans.m_labelTime.Update()));
        }
    }
}
