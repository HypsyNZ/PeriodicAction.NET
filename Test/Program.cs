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

            int count = 1;
            CancellationTokenSource cts = new CancellationTokenSource();
            PeriodicAction Action = new PeriodicAction(() =>
            { 
                Console.WriteLine("Testing" + count++);
            }, 1);

            Action.Run(cts.Token);

            sw.Restart();
            Delay.Wait(500);
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);

            cts.Cancel();

            Console.ReadLine();
        }
    }
}
