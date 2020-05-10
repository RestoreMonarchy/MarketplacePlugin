using Rocket.API;
using System;

namespace RestoreMonarchy.MarketplacePlugin
{
    public class MarketplaceConfiguration : IRocketPluginConfiguration
    {
        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }
        public string WebSocketUrl { get; set; }
        public int ServerId { get; set; }
        public double ProductsRefreshMiliseconds { get; set; }
        public int TimeoutMiliseconds { get; set; }
        public string MessageColor { get; set; }
        public bool Debug { get; set; }
        public void LoadDefaults()
        {
            
            ApiUrl = "https://localhost:5001/api";
            ApiKey = "f215ab5d-e761-415e-b2de-f97db5069977";
            WebSocketUrl = "https://localhost:5001/ws";
            ServerId = 0;
            ProductsRefreshMiliseconds = 10000;
            TimeoutMiliseconds = 10000;
            MessageColor = "yellow";            
            Debug = true;
        }
    }
}