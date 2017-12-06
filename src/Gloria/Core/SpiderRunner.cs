using System;
using System.Collections.Generic;
using Halloc;
using System.Threading.Tasks;
using System.Threading;

namespace Gloria
{
    public static class SpiderRunner
    {
        public static HallocQueue<SpiderTask> TaskQueue;
        internal static Dictionary<string, ITaskConvert> Converts;
        internal static Dictionary<string, ITaskFilter> Filters;
        internal static Dictionary<string, ITaskProcessor> Processors;
        internal static Dictionary<string, ISpider> SpiderActions;
        internal static List<Spider> Spiders;

        private static HashSet<SpiderTask> TasksCompleted;
        public static Action SpiderCompleted;

        public static bool IsCompleted
        {
            get
            {
                foreach (var item in TasksCompleted)
                {
                    if (!item.IsCompleted)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        static SpiderRunner()
        {
            Spiders = new List<Spider>();
            TaskQueue = new HallocQueue<SpiderTask>();
            TasksCompleted = new HashSet<SpiderTask>();
            Filters = new Dictionary<string, ITaskFilter>();
            SpiderActions = new Dictionary<string, ISpider>();
            Converts = new Dictionary<string, ITaskConvert>();
            Processors = new Dictionary<string, ITaskProcessor>();
        }

        public static void Equip<T>(string key = null)
        {

            Type type = typeof(T);
            object[] attrs = type.GetCustomAttributes(typeof(NameAttribute), true);
            if (attrs != null && attrs.Length != 0)
            {
                key = ((NameAttribute)attrs[0]).Name;
            }

            object _instance = Activator.CreateInstance(typeof(T));

            if (type.GetInterface("ISpider") != null)
            {
                SpiderActions[key] = (ISpider)_instance;
#if DEBUG
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("[Runner]\t装载爬行器：{0}\r\n", _instance.ToString());
#endif
            }
            else if (type.GetInterface("ITaskProcessor") != null)
            {
                Processors[key] = (ITaskProcessor)_instance;
#if DEBUG
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("[Runner]\t装载处理器：{0}\r\n", _instance.ToString());
#endif
            }
            else if (type.GetInterface("ITaskConvert") != null)
            {
#if DEBUG
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("[Runner]\t装载转换器：{0}\r\n", _instance.ToString());
#endif
                Converts[key] = (ITaskConvert)_instance;

            }
            else if (type.GetInterface("ITaskFilter") != null)
            {
                Filters[key] = (ITaskFilter)_instance;
#if DEBUG
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("[Runner]\t装载过滤器：{0}\r\n", _instance.ToString());
#endif
            }



        }
        public static SpiderTask GetTask()
        {
            SpiderTask task = TaskQueue.GetTask();

            if (task == null)
            {
                return null;
            }

            if (!String.IsNullOrEmpty(task.ConvertorKey))
            {
                ITaskConvert convert = Converts[task.ConvertorKey];
                convert.Deal(task);
            }
            if (task.IsCompleted)
            {
                return null;
            }
            if (!String.IsNullOrEmpty(task.SpiderKey))
            {
                task.Executor = SpiderActions[task.SpiderKey];
            }

            return task;
        }

        public static void PostTask(SpiderTask task)
        {
            TasksCompleted.Add(task);
            if (!task.IsCompleted)
            {
#if DEBUG
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("[TaskQueue]\t接受任务:{0}\r\n", task.Url);
#endif
                TaskQueue.PostTask(task);
            }
            else
            {
#if DEBUG
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("[TaskQueue]\t驳回任务:{0}\r\n", task.Url);
#endif
            }
        }
        public static void PostResult(SpiderTask task)
        {
            TaskQueue.PostResult(task);
        }
        public static SpiderTask GetResult()
        {
            return TaskQueue.GetResult();
        }
        public static void Run()
        {
            for (int i = 0; i < Hallocation.ProduceCount; i += 1)
            {
                Spider spider = new Spider(i);
                Spiders.Add(spider);
            }
            for (int i = 0; i < Hallocation.ProduceCount; i += 1)
            {

                Spider spider = Spiders[i];
                ThreadPool.QueueUserWorkItem((obj =>
                {
#if DEBUG
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("[Runner]\t{0}号蜘蛛被唤醒\r\n", spider.Index);
#endif
                    spider.Start();
                }));
            }
            ThreadPool.QueueUserWorkItem((obj) =>
            {
                CheckEnd();
            });

            for (int i = 0; i < Hallocation.CustomerCount; i += 1)
            {
                int j = i;
                ThreadPool.QueueUserWorkItem((obj) =>
                {
                    while (true)
                    {
                        SpiderTask task = GetResult();
                        if (task == null)
                        {
                            Thread.Sleep(400);
                            continue;
                        }
#if DEBUG
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine("[Executor]\t拿到结果并交给{0}来处理\r\n", task.ProcessorKey);
#endif
                            if (!String.IsNullOrEmpty(task.FilterKey))
                            {
                                ITaskFilter filter = Filters[task.FilterKey];
                                filter.Run(task);
                            }
                            if (!String.IsNullOrEmpty(task.ProcessorKey))
                            {
                                ITaskProcessor processor = Processors[task.ProcessorKey];
                                processor.Run(task);
                            }
                        PostTask(task);
                    }
                });
            }
        }

        public static void CheckEnd()
        {
            while (true)
            {
                if (IsCompleted)
                {
                    SpiderCompleted?.Invoke();
                    return;
                }
                else
                {
                    Thread.Sleep(3000);
                }
            }
        }
    }
}
