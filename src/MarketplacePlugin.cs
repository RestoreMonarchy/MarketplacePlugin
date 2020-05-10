using Rocket.API.Collections;
using Rocket.Core.Plugins;
using System;
using System.Reflection;
using RestoreMonarchy.MarketplacePlugin.Services;
using Logger = Rocket.Core.Logging.Logger;

namespace RestoreMonarchy.MarketplacePlugin
{
    public sealed class MarketplacePlugin : RocketPlugin<MarketplaceConfiguration>
    {
        public const string SecretUserID = "{{U}}";

        public static MarketplacePlugin Instance { get; private set; }
        public MarketplaceConfiguration config => Configuration.Instance;
        public UnityEngine.Color MessageColor { get; set; }

        public ProductsService ProductsService { get; private set; }
        public MarketItemsService MarketItemsService { get; private set; }

        protected override void Load()
        {
            Instance = this;
            MessageColor = Rocket.Unturned.Chat.UnturnedChat.GetColorFromName(config.MessageColor, UnityEngine.Color.green);

            ProductsService = gameObject.AddComponent<ProductsService>();
            MarketItemsService = gameObject.AddComponent<MarketItemsService>();

			Logger.Log($"{Name} {Assembly.GetName().Version} has been loaded!", ConsoleColor.Yellow);
        }

        protected override void Unload()
        {
            Destroy(ProductsService);
            Destroy(MarketItemsService);
            Logger.Log($"{Name} has been unloaded!", ConsoleColor.Yellow);
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

