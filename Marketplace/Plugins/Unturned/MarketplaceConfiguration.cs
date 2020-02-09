using Rocket.API;
using System;

namespace UnturnedMarketplacePlugin
{
    public class MarketplaceConfiguration : IRocketPluginConfiguration
    {
        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }
        public int TimeoutRetryTimeMiliseconds { get; set; }
        public bool Debug { get; set; }
        public int IconSize { get; set; }
        public void LoadDefaults()
        {
            ApiUrl = "http://localhost:5000/api";
            ApiKey = Guid.NewGuid().ToString();
            TimeoutRetryTimeMiliseconds = 10000;
            Debug = true;
            IconSize = 100;
        }
    }
}