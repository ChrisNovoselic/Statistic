using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StatisticTrans
{
    class ComponentTesting
    {
        /// <summary>
        /// 
        /// </summary>
        public bool bflag = false;

        /// <summary>
        /// Текущая иттерация
        /// </summary>
        public int currentIter = 0;

        /// <summary>
        /// Кол-во компонентов
        /// </summary>
        public int Iters;

        /// <summary>
        /// Ошибочные иттерации
        /// </summary>
        public string[] amountIter;

        /// <summary>
        /// Имя компонента
        /// </summary>
        public string nameComponent;

        /// <summary>
        /// Строка состояния
        /// </summary>
        //public string Text;

        /// <summary>
        /// Время нач. иттераций
        /// </summary>
        string curentTime;

        /// <summary>
        /// Счетчик нового дня
        /// </summary>
        public bool NextDay = false;

        /// <summary>
        /// Кол-во ошибок
        /// </summary>
        int ErrorIter = 0;

        /// <summary>
        /// Конструктор?
        /// </summary>
        public ComponentTesting()
        {

        }

        /// <summary>
        /// приведение к нулю при ошибке
        /// </summary>
        /// <param name="it"></param>
        /// <param name="curIt"></param>
        public void IsNullItter(int it, int curIt)
        {
            if (bflag == true)
            {
                if (curIt == it)
                {
                    currentIter = 0;
                    bflag = false;
                }
            }
        }

        /// <summary>
        /// Счетчик иттераций
        /// </summary>
        public void CounterIter()
        {
            if (NextDay == true)
            {
                if ((2*(ErrorIter + currentIter)) == Iters)
                    currentIter = 0;

                if ((2*currentIter) == Iters)
                {
                    currentIter = 0;
                    currentIter++;
                }

                else
                    currentIter++;
            }

            else
            {
                if ((ErrorIter + currentIter) == Iters)
                    currentIter = 0;

                if (currentIter == Iters)
                {
                    currentIter = 0;
                    currentIter++;
                }

                else
                    currentIter++; 
            }
        }

        /// <summary>
        /// Время начала опроса
        /// </summary>
        /// <returns></returns>
        public string DateStart()
        {
            return curentTime = DateTime.Now.ToString();
        }

        /// <summary>
        /// Кол-во иттераций
        /// </summary>
        public void SetIter(int i)
        {
            if (NextDay == true)
            {
                Iters = i * 2;
            }
            else
            {
                Iters = i;
            }
        }

        /// <summary>
        /// Имя текущего компонента
        /// </summary>
        /// <param name="x"></param>
        public void NameCurComponent(string x)
        {
            nameComponent = x;
        }

        /// <summary>
        /// Компонент на котором сбой
        /// </summary>
        /// <param name="x">имя компонента</param>
        public void ErrorComp(string name)
        {
           
            //Next(name, ErrorItter);
        }

        /// <summary>
        /// Счетчик ошибок
        /// </summary>
        /// <param name="name">имя компонента</param>
        /// <param name="z">номер массива</param>
        public void Error()
        {
            if (ErrorIter == Iters)
            {
                ErrorIter = 0;
            }
            ErrorIter++;
        }
    }
}
