using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StatisticTrans
{
    class ComponentTesting
    {
        /// <summary>
        /// Текущая иттерация
        /// </summary>
        public int currentItter = 0;

        /// <summary>
        /// Кол-во компонентов
        /// </summary>
        public int Itters;

        /// <summary>
        /// Ошибочные иттерации
        /// </summary>
        public string[] amountItter;

        /// <summary>
        /// Имя компонента
        /// </summary>
       public string nameComponent;

        /// <summary>
        /// Строка состояния
        /// </summary>
        public string Text;

        /// <summary>
        /// Время нач. иттераций
        /// </summary>
        string curentTime;

        /// <summary>
        /// Кол-во ошибок
        /// </summary>
        int ErrorItter = 0;

        /// <summary>
        /// Конструктор?
        /// </summary>
        public ComponentTesting()
        {

        }

        /// <summary>
        /// Счетчик иттераций
        /// </summary>
        public void CounterItter()
        {
            if (currentItter == 10)
            {
                currentItter = 0;
                currentItter++;
            }

            else
                currentItter++;
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
        public void SetItter(int i)
        {
            Itters = i;
        }

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
            ErrorItter++;
            Next(name, ErrorItter);
        }

        /// <summary>
        /// Счетчик ошибок
        /// </summary>
        /// <param name="name">имя компонента</param>
        /// <param name="z">номер массива</param>
        public void Next(string name, int z)
        {
            amountItter.SetValue(name, z);
        }
    }
}
