using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace RestoreMonarchy.MarketplacePlugin.Services
{
    public class EconomyService : MonoBehaviour
    {
        private MarketplacePlugin pluginInstance => MarketplacePlugin.Instance;
        private ClientWebSocket client;
        void Awake()
        {
            Task.Run(StartWebSocketClientAsync);
        }

        async Task StartWebSocketClientAsync()
        {
            client = new ClientWebSocket();
            await client.ConnectAsync(new Uri(pluginInstance.config.WebSocketUrl), CancellationToken.None);

            await Task.WhenAll(ReceiveAsync(), SendAsync());
        }

        async Task ReceiveAsync()
        {
            while (client.State == WebSocketState.Open)
            {

            }
        }

        async Task SendAsync()
        {

        }
    }
}
