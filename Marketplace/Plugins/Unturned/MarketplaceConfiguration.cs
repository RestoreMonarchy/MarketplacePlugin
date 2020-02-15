using Rocket.API;
using System;

namespace UnturnedMarketplacePlugin
{
    public class MarketplaceConfiguration : IRocketPluginConfiguration
    {
        public string MessageColor { get; set; }
        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }
        public int TimeoutMiliseconds { get; set; }
        public bool Debug { get; set; }
        public void LoadDefaults()
        {
            MessageColor = "yellow";
            ApiUrl = "http://localhost:5046/api";
            ApiKey = "f215ab5d-e761-415e-b2de-f97db5069977";
            TimeoutMiliseconds = 10000;
            Debug = true;
        }
    }
}