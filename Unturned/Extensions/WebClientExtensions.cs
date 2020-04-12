using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UnturnedMarketplacePlugin.Extensions
{
    public static class WebClientExtensions
    {
        public static T DownloadJson<T>(this WebClient webclient, string address)
        {
            string data = webclient.DownloadString(address);
            Console.WriteLine(data);
            return JsonConvert.DeserializeObject<T>(data);
        }
    }
}
