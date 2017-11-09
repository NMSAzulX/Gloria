using Gloria;
using System.Net.Http;

namespace System
{
    public interface ISpider
    {
        void Run(HttpClient client,SpiderTask task);
    }
}
