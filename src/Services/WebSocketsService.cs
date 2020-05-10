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
            AwakeAsync()?.GetAwaiter().GetResult();

            async Task AwakeAsync()
            {
                Logger.Log("Intializing WebSockets...", ConsoleColor.Green);
                Manager = new WebSocketsManager(new WebSocketsConsoleLogger(true));
                Manager.Initialize(GetType().Assembly, new object[] { this });

                client.Options.SetRequestHeader("x-api-key", pluginInstance.config.ApiKey);
                try
                {                    
                    await client.ConnectAsync(new Uri(pluginInstance.config.WebSocketUrl), 
                        new CancellationTokenSource(pluginInstance.config.TimeoutMiliseconds).Token);
                }
                catch (TaskCanceledException)
                {
                    Logger.LogWarning("WebSocketsServer timeout");
                    throw new TimeoutException();
                }

                Logger.Log("Successfully connected to WebSockets server", ConsoleColor.Green);
                await Manager.TellWebSocketAsync(client, "ServerId", null, pluginInstance.Configuration.Instance);
                await Task.Run(() => Manager.ListenWebSocketAsync(client));
                Logger.Log("WebSocket Listener started", ConsoleColor.Green);
                            
            }
        }

        [WebSocketCall("PlayerBalance")]
        private async Task TellPlayerBalanceAsync(WebSocketMessage question)
        {
            var playerId = (string)question.Arguments[0];
            var balance = pluginInstance.EconomyProvider.GetPlayerBalance(playerId);
            await Manager.TellWebSocketAsync(client, "PlayerBalance", question.Id, balance);
        }

        [WebSocketCall("IncrementPlayerBalance")]
        private async Task TellIncrementBalanceAsync(WebSocketMessage question)
        {
            var playerId = Convert.ToString(question.Arguments[0]);
            var amount = Convert.ToDecimal(question.Arguments[1]);
            var result = pluginInstance.EconomyProvider.IncrementPlayerBalance(playerId, amount);
            await Manager.TellWebSocketAsync(client, "IncrementPlayerBalance", question.Id, result);
        }

        [WebSocketCall("Pay")]
        private async Task TellPayAsync(WebSocketMessage question)
        {
            var senderId = Convert.ToString(question.Arguments[0]);
            var receiverId = Convert.ToString(question.Arguments[1]);
            var amount = Convert.ToDecimal(question.Arguments[2]);
            var result = pluginInstance.EconomyProvider.Pay(senderId, receiverId, amount);
            await Manager.TellWebSocketAsync(client, "Pay", question.Id, result);
        }
    }
}
