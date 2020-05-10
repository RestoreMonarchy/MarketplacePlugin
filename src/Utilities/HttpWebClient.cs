using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RestoreMonarchy.MarketplacePlugin.Utilities
{
    public class MarketplaceHttpClient
    {
        private MarketplacePlugin pluginInstance => MarketplacePlugin.Instance;
        public string BaseUrl { get; set; }

        public MarketplaceHttpClient()
        {
            BaseUrl = pluginInstance.config.ApiUrl.TrimEnd('/') + "/";
        }

        public async Task<HttpWebResponse> PostAsJsonAsync(string relativeUrl, object obj)
        {
            var content = JsonConvert.SerializeObject(obj);
            var data = Encoding.ASCII.GetBytes(content);

            var request = WebRequest.Create(BuildUrl(relativeUrl)) as HttpWebRequest;
            
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;

            request.Headers["x-api-key"] = pluginInstance.config.ApiKey;
            request.Timeout = pluginInstance.config.TimeoutMiliseconds;

            using (var stream = await request.GetRequestStreamAsync())
            {
                await stream.WriteAsync(data, 0, data.Length);
            }

            using (var response = await request.GetResponseAsync())
            {
                return (HttpWebResponse)response;
            }
        }

        public async Task<T> GetFromJsonAsync<T>(string relativeUrl)
        {
            var request = WebRequest.Create(BuildUrl(relativeUrl)) as HttpWebRequest;

            request.Method = "GET";

            request.Headers["x-api-key"] = pluginInstance.config.ApiKey;
            request.Timeout = pluginInstance.config.TimeoutMiliseconds;

            string content;
            using (var response = await request.GetResponseAsync())
            {
                using (var stream = response.GetResponseStream())
                {
                    var reader = new StreamReader(stream);
                    content = await reader.ReadToEndAsync();
                }
            }

            return JsonConvert.DeserializeObject<T>(content);
        }

        private string BuildUrl(string relativeUrl)
        {
            return BaseUrl + relativeUrl;
        }
    }
}
