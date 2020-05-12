using Newtonsoft.Json;
using Rocket.Core.Logging;
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
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
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(AcceptAllCertifications);
            BaseUrl = pluginInstance.config.ApiUrl.TrimEnd('/') + "/";
        }

        private bool AcceptAllCertifications(object a, X509Certificate b, X509Chain c, SslPolicyErrors d) => true;

        public async Task<HttpWebResponse> PostAsJsonAsync(string relativeUrl, object obj)
        {
            var content = JsonConvert.SerializeObject(obj);
            var data = Encoding.ASCII.GetBytes(content);

            var targetUrl = BuildUrl(relativeUrl);
            var request = WebRequest.Create(targetUrl) as HttpWebRequest;
            
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;

            request.Headers["x-api-key"] = pluginInstance.config.ApiKey;
            request.Timeout = pluginInstance.config.TimeoutMiliseconds;

            try
            {
                using (var stream = await request.GetRequestStreamAsync())
                {
                    await stream.WriteAsync(data, 0, data.Length);
                }

                using (var response = await request.GetResponseAsync())
                {
                    return (HttpWebResponse)response;
                }
            } catch (WebException e)
            {
                Logger.LogError($"An error occurated during POST request to {targetUrl}: {e.Message}");
                return null;
            }
        }

        public async Task<T> GetFromJsonAsync<T>(string relativeUrl)
        {
            var targetUrl = BuildUrl(relativeUrl);
            var request = WebRequest.Create(targetUrl) as HttpWebRequest;

            request.Method = "GET";

            request.Headers["x-api-key"] = pluginInstance.config.ApiKey;
            request.Timeout = pluginInstance.config.TimeoutMiliseconds;

            string content;
            try
            {
                using (var response = await request.GetResponseAsync())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        var reader = new StreamReader(stream);
                        content = await reader.ReadToEndAsync();
                    }
                }
            } catch (WebException e)
            {
                Logger.LogError($"An error occurated during GET request to {targetUrl}: {e.Message}");
                return default;
            }

            return JsonConvert.DeserializeObject<T>(content);
        }

        private string BuildUrl(string relativeUrl)
        {
            return BaseUrl + relativeUrl;
        }
    }
}
