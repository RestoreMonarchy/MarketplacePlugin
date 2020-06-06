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
        public int WebSocketsReconnectDelayMiliseconds { get; set; }
        public int ProductsRefreshMiliseconds { get; set; }
        public int TimeoutMiliseconds { get; set; }
        public string MessageColor { get; set; }
        public bool Debug { get; set; }
        public string SellCommandName { get; set; }
        public string ClaimCommandName { get; set; }

        public void LoadDefaults()
        {            
            ApiUrl = "https://localhost:5001/api";
            ApiKey = "f215ab5d-e761-415e-b2de-f97db5069977";
            WebSocketUrl = "ws://localhost:5001/ws";
            ServerId = 0;
            WebSocketsReconnectDelayMiliseconds = 5000;
            ProductsRefreshMiliseconds = 60000;
            TimeoutMiliseconds = 3000;
            MessageColor = "yellow";            
            Debug = true;
            SellCommandName = "sell";
            ClaimCommandName = "claim";
        }
    }
}