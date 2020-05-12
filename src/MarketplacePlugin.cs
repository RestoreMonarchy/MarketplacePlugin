﻿using Rocket.API.Collections;
using Rocket.Core.Plugins;
using System;
using System.Reflection;
using RestoreMonarchy.MarketplacePlugin.Services;
using Logger = Rocket.Core.Logging.Logger;
using RestoreMonarchy.MarketplacePlugin.Economy;
using Rocket.API;
using Rocket.Core;
using System.Collections.Generic;
using SDG.Unturned;
using Rocket.Core.Utils;

namespace RestoreMonarchy.MarketplacePlugin
{
    public sealed class MarketplacePlugin : RocketPlugin<MarketplaceConfiguration>
    {
        public static MarketplacePlugin Instance { get; private set; }
        public MarketplaceConfiguration config => Configuration.Instance;
        public UnityEngine.Color MessageColor { get; set; }

        public ProductsService ProductsService { get; private set; }
        public MarketItemsService MarketItemsService { get; private set; }
        public WebSocketsService WebSocketsService { get; private set; }

        public static Dictionary<string, Type> SupportedEconomyPlugins = new Dictionary<string, Type>()
        {
            { "Uconomy", typeof(UconomyEconomyProvider) }
        };

        public IRocketPlugin EconomyPlugin { get; private set; }
        public IEconomyProvider EconomyProvider { get; private set; }

        protected override void Load()
        {
            Instance = this;
            MessageColor = Rocket.Unturned.Chat.UnturnedChat.GetColorFromName(config.MessageColor, UnityEngine.Color.green);

            if (!Level.isLoaded)
                Level.onPostLevelLoaded += GetEconomyProvider;
            else
                GetEconomyProvider(0);

            ProductsService = gameObject.AddComponent<ProductsService>();
            Logger.Log("Products service loaded", ConsoleColor.DarkGreen);
            MarketItemsService = gameObject.AddComponent<MarketItemsService>();
            Logger.Log("MarketItems service loaded", ConsoleColor.DarkGreen);
            WebSocketsService = gameObject.AddComponent<WebSocketsService>();
            Logger.Log("WebSocketsService loaded", ConsoleColor.DarkGreen);

			Logger.Log($"{Name} {Assembly.GetName().Version} has been loaded!", ConsoleColor.Yellow);
        }

        private void GetEconomyProvider(int a)
        {
            foreach (var plugin in SupportedEconomyPlugins)
            {
                if ((EconomyPlugin = R.Plugins.GetPlugin(plugin.Key)) != null)
                {                    
                    EconomyProvider = Activator.CreateInstance(plugin.Value) as IEconomyProvider;
                    Logger.Log($"EconomyProvider is set to {plugin.Value.Name}", ConsoleColor.Green);
                    return;
                }
            }
            Logger.LogWarning("No supported Economy plugin found, EconomyProvider is null!");
        }

        protected override void Unload()
        {
            Level.onPostLevelLoaded -= GetEconomyProvider;
            Destroy(ProductsService);
            Destroy(MarketItemsService);
            Destroy(WebSocketsService);
            Logger.Log($"{Name} has been unloaded!", ConsoleColor.Yellow);
        }

        void OnDestroy()
        {
            Console.WriteLine("plugin has been destroyed btw");
        }

        public override TranslationList DefaultTranslations => new TranslationList() 
        {
            { "ClaimInvalid", "Invalid usage. Use: /claim <orderId>" },
            { "ClaimAlready", "You have already claimed this order or it's not yours!" },
            { "ClaimSuccess", "Successfully claimed your {0} of order {1}!" },
            { "SellInvalid", "Invalid usage. Use: /claim <price>" },
            { "SellSuccess", "Successfully put your {0} on sale for {1}!" },
            { "SellTimeout", "Timeout, Try again later. {0} returned" },
            { "SellLimit", "You have reached the limit of maximum active sellings. {0} returned" }
        };
    }
}

