using Halloc;
using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace Gloria
{
    public class SpiderTask : AbstractHalloc
    {
        public string Url;
        public string UrlTemplate;
        public Regex MatchTemplete;
        public Stream StreamResult;
        public byte[] ByteResult;
        public string HtmlResult;
        public string ConvertorKey;
        public string FilterKey;
        public string ProcessorKey;
        public string SpiderKey;
        public bool IsCompleted;
        public ISpider Executor;
        public string FileName;
        public string SavePath;
    }
}
