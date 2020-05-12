using Marketplace.Shared;
using Rocket.Core;
using Rocket.Core.Utils;
using Rocket.Unturned;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RestoreMonarchy.MarketplacePlugin.Models;
using RestoreMonarchy.MarketplacePlugin.Storage;
using Timer = System.Timers.Timer;
using Logger = Rocket.Core.Logging.Logger;
using SDG.Unturned;
using Math = System.Math;
using RestoreMonarchy.MarketplacePlugin.Utilities;
using System.Threading.Tasks;

namespace RestoreMonarchy.MarketplacePlugin.Services
{
    public class ProductsService : MonoBehaviour
    {
        private MarketplacePlugin pluginInstance => MarketplacePlugin.Instance;
        private MarketplaceHttpClient httpClient = new MarketplaceHttpClient();

        public DataStorage<List<AwaitingCommand>> Storage { get; private set; }
        public List<AwaitingCommand> AwaitingCommands { get; set; }

        void Awake()
        {
            if (pluginInstance.config.Debug)
                Logger.Log($"Initializing {nameof(ProductsService)}", ConsoleColor.Yellow);
            
            Storage = new DataStorage<List<AwaitingCommand>>(pluginInstance.Directory, "ProductsData.json");

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
            foreach (var command in AwaitingCommands.Where(x => x.PlayerId == player.Id && x.ExecuteOnPlayerJoin).ToList())
            {
                ExecuteCommand(command.CommandText, command.PlayerId, command.PlayerName);
                AwaitingCommands.Remove(command);
                Storage.Save(AwaitingCommands);
            }
        }

        public async Task RefreshAwaitingCommands()
        {
            IEnumerable<ServerTransaction> transactions;
            while (pluginInstance.ProductsService != null)
            {
                transactions = await httpClient.GetFromJsonAsync<IEnumerable<ServerTransaction>>(
                    $"/products/server?serverId={pluginInstance.config.ServerId}");

                TaskDispatcher.QueueOnMainThread(() => ProcessTransactions(transactions));
                if (pluginInstance.config.Debug)
                    Logger.Log($"Transactions Processed", ConsoleColor.Yellow);

                await Task.Delay(Math.Max(1000, pluginInstance.config.ProductsRefreshMiliseconds));
            }
        }

        private void ProcessTransactions(IEnumerable<ServerTransaction> transactions)
        {
            foreach (var transaction in transactions)
            {
                Logger.Log($"[Received Transaction] {transaction.PlayerName} bought {transaction.ProductTitle}!");
                foreach (var command in transaction.Commands)
                {
                    if (command.ExecuteOnBuyerJoinServer)
                    {
                        if (PlayerTool.getSteamPlayer(ulong.Parse(transaction.PlayerId)) != null)
                            ExecuteCommand(command.CommandText, transaction.PlayerId, transaction.PlayerName);
                        else
                            AwaitingCommands.Add(new AwaitingCommand(command.CommandText, transaction.PlayerId, transaction.PlayerName));
                    }
                    else
                        ExecuteCommand(command.CommandText, transaction.PlayerId, transaction.PlayerName);

                    if (command.Expires)
                    {
                        AwaitingCommands.Add(new AwaitingCommand(command.ExpireCommand, transaction.PlayerId,
                            transaction.PlayerName, false, DateTime.Now.AddSeconds(command.ExpireTime)));
                    }
                }
            }

            Storage.Save(AwaitingCommands);
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
                            (a, b) => 
                            {
                                ExecuteCommand(command.CommandText, command.PlayerId, command.PlayerName);
                                AwaitingCommands.Remove(command);
                                Storage.Save(AwaitingCommands);
                            };
                        command.ExecuteTimer.Start();
                    }
                }
            }
        }

        public void ExecuteCommand(string commandText, string playerId, string playerName)
        {
            Logger.Log($"Executing {commandText} for {playerName}[{playerId}]");
            R.Commands.Execute(null, commandText.Replace("{PlayerId}", playerId).Replace("{PlayerName}", playerName));
        }

        void OnDestroy()
        {
            Storage.Save(AwaitingCommands);
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
