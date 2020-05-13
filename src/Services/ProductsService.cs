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
using Marketplace.WebSockets.Attributes;
using Marketplace.WebSockets.Models;
using Rocket.Core.Logging;
using RestoreMonarchy.MarketplacePlugin.Logging;

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

            InitializeAwaitingCommandsTimers();

            if (!Level.isLoaded)
                Level.onLevelLoaded += (i) => Task.Run(ProcessAwaitingTransactions);
            else
                Task.Run(ProcessAwaitingTransactions);            

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

        [WebSocketCall("ProductTransaction")]
        public async Task TellPlayerProductBuyAsync(WebSocketMessage question)
        {
            var transactionId = question.Arguments[0];
            var transaction = await httpClient.GetFromJsonAsync<ServerTransaction>($"products/server/{transactionId}");
            ServiceLogger.LogInformation<ProductsService>($"[Received Transaction] {transaction.PlayerName} purchased {transaction.ProductTitle}!");
            ProcessTransaction(transaction);
        }


        private async Task ProcessAwaitingTransactions()
        {
            var transactions = await httpClient.GetFromJsonAsync<IEnumerable<ServerTransaction>>($"products/server?serverId={pluginInstance.config.ServerId}");
            ServiceLogger.LogDebug<ProductsService>($"Downloaded {transactions.Count()} uncompleted transactions!");
            foreach (var transaction in transactions)
            {
                ProcessTransaction(transaction);
            }
        }

        private void ProcessTransaction(ServerTransaction transaction)
        {
            ServiceLogger.LogInformation<ProductsService>($"Processing {transaction.TransactionId} transaction!");
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
            ServiceLogger.LogInformation<ProductsService>($"Executing {commandText} for {playerName}[{playerId}]");
            R.Commands.Execute(null, commandText.Replace("{PlayerId}", playerId).Replace("{PlayerName}", playerName));
        }

        void OnDestroy()
        {
            Storage.Save(AwaitingCommands);
            U.Events.OnPlayerConnected -= OnPlayerConnected;
            Level.onLevelLoaded -= (i) => Task.Run(ProcessAwaitingTransactions); 
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
