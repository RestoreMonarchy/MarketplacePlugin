using Rocket.API;
using System;

namespace RestoreMonarchy.MarketplacePlugin
{
    public class MarketplaceConfiguration : IRocketPluginConfiguration
    {
        public string MessageColor { get; set; }
        public int ServerId { get; set; }
        public string WebSocketUrl { get; set; }
        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }
        public double ProductsRefreshMiliseconds { get; set; }
        public int TimeoutMiliseconds { get; set; }
        public bool Debug { get; set; }
        public void LoadDefaults()
        {
            MessageColor = "yellow";
            ServerId = 0;
            WebSocketUrl = "http://localhost:5000/ws";
            ApiUrl = "http://localhost:5046/api";
            ApiKey = "f215ab5d-e761-415e-b2de-f97db5069977";
            ProductsRefreshMiliseconds = 10000;
            TimeoutMiliseconds = 10000;
            Debug = true;
        }
    }
}