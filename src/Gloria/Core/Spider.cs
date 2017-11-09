using Halloc;
using System.Net.Http;
using System.Threading;

namespace Gloria
{
    public class Spider : AbstractHalloc
    {
        private HttpClient _client;
        public bool IsRunning;
        internal bool IsAlive;
        public Spider(int i =0 )
        {
            Index = i;
            IsAlive = true;
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Connection.Add("keep-alive");
        }


        public void Start()
        {
            SpiderTask task;
            while (IsAlive)
            {
                task = SpiderRunner.GetTask();
                if (task == null)
                {
                    IsRunning = false;
                    Thread.Sleep(300);
                    continue;
                }
#if DEBUG
                System.Console.ForegroundColor = System.ConsoleColor.DarkYellow;
                System.Console.WriteLine("[Spider]\t{0}号爬虫开始工作,目标:{1}\r\n", Index, task.Url);
#endif
                task.Index = Index;
                IsRunning = true;
                task.Executor?.Run(_client,task);
                SpiderRunner.PostResult(task);
            }
            if (_client != null)
            {
                _client.CancelPendingRequests();
                _client.Dispose();
                _client = null;
            }
        }
    }
}
