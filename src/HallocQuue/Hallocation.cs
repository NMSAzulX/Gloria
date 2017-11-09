using System;
using System.Diagnostics;

namespace Halloc
{
    public static class Hallocation
    {
        public static int AvaliableThreads;
        public static int TotleThreads;
        public static int Processors;

        public static int ProduceCount;
        public static int CustomerCount;
        static Hallocation()
        {
            Processors = Environment.ProcessorCount;
            TotleThreads = Process.GetCurrentProcess().Threads.Count;
            if (TotleThreads<=2)
            {
                AvaliableThreads = 2;
            }
            else
            {
                AvaliableThreads = TotleThreads-2;
            }
            CustomerCount = (int)Math.Floor(AvaliableThreads / 4 * 1.0);
            ProduceCount = AvaliableThreads - CustomerCount;

#if DEBUG
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("------------------------------");
            Console.WriteLine("当前系统核数:\t{0}",Processors);
            Console.WriteLine("系统总线程数:\t{0}", TotleThreads);
            Console.WriteLine("------------------------------");
            Console.WriteLine("推荐使用线程数:\t{0}", AvaliableThreads);
            Console.WriteLine("推荐生产者线程数:{0}", ProduceCount);
            Console.WriteLine("推荐消费者线程数:{0}", CustomerCount);
            Console.WriteLine("------------------------------");
            Console.WriteLine();
#endif
        }
    }
}
