using Marketplace.Shared;
using Rocket.Core;
using Rocket.Core.Utils;
using Rocket.Unturned;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnturnedMarketplacePlugin.Extensions;
using UnturnedMarketplacePlugin.Models;
using UnturnedMarketplacePlugin.Storage;
using Timer = System.Timers.Timer;
using Logger = Rocket.Core.Logging.Logger;

namespace UnturnedMarketplacePlugin.Services
{
    public class ProductsService : MonoBehaviour
    {
        private MarketplacePlugin pluginInstance => MarketplacePlugin.Instance;
        private WebClient webClient = new WebClient();

        public ProductsStorage<List<AwaitingCommand>> Storage { get; private set; }
        public List<AwaitingCommand> AwaitingCommands { get; set; }
        
        void Awake()
        {
            Logger.Log("Initializing ProductsService");
            Storage = new ProductsStorage<List<AwaitingCommand>>(pluginInstance.Directory, "ProductsData.json");

            if ((AwaitingCommands = Storage.Read()) == null)
            {
                AwaitingCommands = new List<AwaitingCommand>();
                Storage.Save(AwaitingCommands);
            }
            Task.Run(RefreshAwaitingCommands);
            U.Events.OnPlayerConnected += OnPlayerConnected;
        }

        private void OnPlayerConnected(UnturnedPlayer player)
        {
            foreach (var command in AwaitingCommands.Where(x => x.PlayerId == player.Id && x.ExecuteOnPlayerJoin))
            {
                ExecuteCommand(command.CommandText, command.PlayerId, command.PlayerName);
            }
        }

        public void RefreshAwaitingCommands()
        {
            TaskDispatcher.QueueOnMainThread(() => Logger.Log($"RefreshAwaitingCommands called"));
            IEnumerable<ProductTransaction> transactions;
            webClient.Headers["x-api-key"] = pluginInstance.config.ApiKey;
            try
            {
                transactions = webClient.DownloadJson<List<ProductTransaction>>(
                    pluginInstance.config.ApiUrl + $"/products/server?serverId={pluginInstance.config.ServerId}");
            } catch (Exception e)
            {
                TaskDispatcher.QueueOnMainThread(() => Logger.LogException(e));
                return;
            }

            // ends somehwere around here damn it

            TaskDispatcher.QueueOnMainThread(() => Logger.Log($"Transactions count: {transactions.Count()}"));
            

            foreach (var transaction in transactions)
            {
                foreach (var command in transaction.Product.Commands)
                {
                    if (command.ExecuteOnBuyerJoinServer)
                        AwaitingCommands.Add(new AwaitingCommand(command.CommandText, transaction.PlayerId, transaction.PlayerName));
                    else
                        ExecuteCommand(command.CommandText, transaction.PlayerId, transaction.PlayerName);

                    if (command.Expires)
                    {
                        AwaitingCommands.Add(new AwaitingCommand(command.ExpireCommand, transaction.PlayerId,
                            transaction.PlayerName, false, DateTime.Now.AddSeconds(command.ExpireTime)));
                    }
                }
            }
            InitializeAwaitingCommandsTimers();
        }

        public void InitializeAwaitingCommandsTimers()
        {
            lock (AwaitingCommands)
            {
                foreach (var command in AwaitingCommands)
                {
                    if (!command.ExecuteOnPlayerJoin && command.ExecuteTimer == null)
                    {
                        command.ExecuteTimer = new Timer(Math.Max(1, (command.ExecuteTime - DateTime.Now).Value.Milliseconds));
                        command.ExecuteTimer.AutoReset = false;
                        command.ExecuteTimer.Elapsed +=
                            (a, b) => ExecuteCommand(command.CommandText, command.PlayerId, command.PlayerName);
                        command.ExecuteTimer.Start();
                    }
                }
            }
        }

        public void ExecuteCommand(string commandText, string playerId, string playerName)
        {
            Logger.Log($"{commandText} for {playerId}[{playerName}] executes");
            TaskDispatcher.QueueOnMainThread(() => R.Commands.Execute(null, 
                commandText.Replace("{PlayerId}", playerId).Replace("{PlayerName}", playerName)));
        }

        void OnDestroy()
        {
            U.Events.OnPlayerConnected -= OnPlayerConnected;
            lock (AwaitingCommands)
            {
                foreach (var command in AwaitingCommands)
                {
                    command.ExecuteTimer.Dispose();
                }
            }
        }
    }
}
