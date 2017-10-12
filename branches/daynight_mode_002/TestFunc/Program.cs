using System;

namespace TestFunc
{
    class Program
    {
        static void Main(string[] args)
        {
            object test = null;

            try {
                //test = new DomainName ();
                //test = new ImportCSV ();

                //test = new HOracleConnection();
                ////test = new HOleDbOracleConnection ();
                //(test as HDbOracle).Open ();
                //(test as HDbOracle).Query();                
                //(test as HDbOracle).Close ();
                //(test as HDbOracle).OutResult ();

                //test = new DbSources();

                //test = new GenericSorted();

                test = new SetQuery ();
            } catch (Exception e) {
                Console.Write(e.Message + Environment.NewLine);
            }

            Console.Write(@"Для завершения работы нажмите любую клавишу..."); Console.ReadKey();
        }
    }
}
