using Marketplace.WebSockets;
using Marketplace.WebSockets.Attributes;
using Marketplace.WebSockets.Logger;
using Marketplace.WebSockets.Models;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace RestoreMonarchy.MarketplacePlugin.Services
{
    public class WebSocketsService : MonoBehaviour
    {
        private MarketplacePlugin pluginInstance => MarketplacePlugin.Instance;

        private ClientWebSocket client = new ClientWebSocket();

        public WebSocketsManager Manager { get; private set; }

        void Awake()
        {
            Task.Run(AwakeAsync);
        }

        void OnDestroy()
        {
            if (client.CloseStatus == null)
                client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Service Destroy", CancellationToken.None).Wait();
        }
        
        private async Task AwakeAsync()
        {
            try
            {
                if (pluginInstance.config.Debug)
                    Logger.Log("Loading WebSocketsService", ConsoleColor.DarkGreen);

                Manager = new WebSocketsManager(new WebSocketsConsoleLogger(true));
                Manager.Initialize(GetType().Assembly, new object[] { this });

                client.Options.SetRequestHeader("x-api-key", pluginInstance.config.ApiKey);

                await client.ConnectAsync(new Uri(pluginInstance.config.WebSocketUrl),
                    new CancellationTokenSource(pluginInstance.config.TimeoutMiliseconds).Token);
                await Manager.TellWebSocketAsync(client, "ServerId", null, pluginInstance.Configuration.Instance.ServerId);
                Logger.Log("Successfully connected to WebSockets server!", ConsoleColor.Green);

                await Manager.ListenWebSocketAsync(client);
            } catch (Exception e)
            {
                Logger.LogException(e);
            }
        }

        [WebSocketCall("PlayerBalance")]
        private async Task TellPlayerBalanceAsync(WebSocketMessage question)
        {
            var playerId = question.Arguments[0];
            var balance = pluginInstance.EconomyProvider.GetPlayerBalance(playerId);
            await Manager.TellWebSocketAsync(client, "PlayerBalance", question.Id, balance);
        }

        [WebSocketCall("IncrementPlayerBalance")]
        private async Task TellIncrementBalanceAsync(WebSocketMessage question)
        {
            var playerId = question.Arguments[0];
            var amount = Convert.ToDecimal(question.Arguments[1]);
            var result = pluginInstance.EconomyProvider.IncrementPlayerBalance(playerId, amount);
            await Manager.TellWebSocketAsync(client, "IncrementPlayerBalance", question.Id, result);
        }

        [WebSocketCall("Pay")]
        private async Task TellPayAsync(WebSocketMessage question)
        {
            var senderId = question.Arguments[0];
            var receiverId = question.Arguments[1];
            var amount = Convert.ToDecimal(question.Arguments[2]);
            var result = pluginInstance.EconomyProvider.Pay(senderId, receiverId, amount);
            await Manager.TellWebSocketAsync(client, "Pay", question.Id, result);
        }
    }
}
