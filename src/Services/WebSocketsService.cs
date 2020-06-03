using Marketplace.WebSockets;
using Marketplace.WebSockets.Attributes;
using Marketplace.WebSockets.Models;
using RestoreMonarchy.MarketplacePlugin.Logging;
using Rocket.Core.Utils;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace RestoreMonarchy.MarketplacePlugin.Services
{
    public class WebSocketsService : MonoBehaviour
    {
        private MarketplacePlugin pluginInstance => MarketplacePlugin.Instance;

        private ClientWebSocket client;

        public WebSocketsManager Manager { get; private set; }

        void Awake()
        {
            Manager = new WebSocketsManager(new RocketWebSocketsLogger());
            Manager.Initialize(GetType().Assembly, new object[] { this, pluginInstance.ProductsService });
            client = new ClientWebSocket();
            client.Options.SetRequestHeader("x-api-key", pluginInstance.config.ApiKey);
        }

        void Start()
        {
            Task.Run(StartAsync);
        }

        void OnDestroy()
        {
            if (client.CloseStatus == null)
                client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Service Destroy", CancellationToken.None).Wait();
        }

        private async Task StartAsync()
        {
            try
            {   
                await client.ConnectAsync(new Uri(pluginInstance.config.WebSocketUrl),
                    new CancellationTokenSource(pluginInstance.config.TimeoutMiliseconds).Token);

                await Manager.TellWebSocketAsync(client, "ServerId", null, pluginInstance.Configuration.Instance.ServerId);

                ServiceLogger.LogInformation<WebSocketsService>("Connected to Web!");
                await Manager.ListenWebSocketAsync(client);
                ServiceLogger.LogInformation<WebSocketsService>("Disconnect from Web!");
            } catch (Exception e)
            {
                ServiceLogger.LogError<WebSocketsService>(e);
                await Task.Delay(pluginInstance.Configuration.Instance.WebSocketsReconnectDelayMiliseconds);
                ServiceLogger.LogInformation<WebSocketsService>("Retrying to connect to web...");
                await StartAsync();
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
            TaskDispatcher.QueueOnMainThread(() =>
            {
                var result = pluginInstance.EconomyProvider.IncrementPlayerBalance(playerId, amount);
                Task.Run(() => Manager.TellWebSocketAsync(client, "IncrementPlayerBalance", question.Id, result));
            });
        }

        [WebSocketCall("Pay")]
        private async Task TellPayAsync(WebSocketMessage question)
        {
            var senderId = question.Arguments[0];
            var receiverId = question.Arguments[1];
            var amount = Convert.ToDecimal(question.Arguments[2]);
            TaskDispatcher.QueueOnMainThread(() =>
            {
                var result = pluginInstance.EconomyProvider.Pay(senderId, receiverId, amount);
                Task.Run(() => Manager.TellWebSocketAsync(client, "Pay", question.Id, result));
            });
        }
    }
}
