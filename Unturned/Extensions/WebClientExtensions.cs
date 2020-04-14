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
        public static async Task<T> DownloadJsonAsync<T>(this WebClient webClient, string address)
        {
            
            string data = await webClient.DownloadStringTaskAsync(new Uri(address));
            return JsonConvert.DeserializeObject<T>(data);
        }

        public static void UploadJson(this WebClient webClient, string address, object obj)
        {
            try
            {
                webClient.ResponseHeaders[HttpRequestHeader.ContentType] = "application/json";
                webClient.UploadString(address, JsonConvert.SerializeObject(obj));
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
