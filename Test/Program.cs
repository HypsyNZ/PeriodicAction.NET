using System;
using System.Diagnostics;
using System.Threading;

namespace Test
{
    internal class Program
    {
        static void Main()
        {
            Stopwatch sw = Stopwatch.StartNew();

            for (int i = 0; i < 20; i++)
            {
                sw.Restart();
                Delay.Wait(1);
                sw.Stop();
                Console.WriteLine(sw.ElapsedMilliseconds);
            }
            Console.ReadLine();

            //2 -- First call for application might be slower
            //1
            //1
            //1
            //1
            //1
            //1
            //1
            //1
            //1
            //1
            //1
            //1
            //1
            //1
            //1
            //1
            //1
            //1
            //1
        }
    }
}
