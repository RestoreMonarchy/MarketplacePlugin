using Rocket.Core.Plugins;
using System;
using System.Net;
using Logger = Rocket.Core.Logging.Logger;
using System.Reflection;
using System.Threading;
using UnturnedMarketplacePlugin.Extensions;
using SDG.Unturned;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Rocket.Core.Commands;

namespace UnturnedMarketplacePlugin
{
    public class MarketplacePlugin : RocketPlugin<MarketplaceConfiguration>
    {
        public static MarketplacePlugin Instance { get; private set; }

        public WebClient WebClient { get; private set; }

        public MarketplaceConfiguration config => Configuration.Instance;


        protected override void Load()
        {
            Instance = this;
            WebClient = new WebClient();
            WebClient.Headers["x-api-key"] = config.ApiKey;
            this.LoadAssets();

			Logger.Log($"{Name} {Assembly.GetName().Version} has been loaded!", ConsoleColor.Yellow);
        }

        protected override void Unload()
        {
            Logger.Log($"{Name} has been unloaded!", ConsoleColor.Yellow);
        }
    }
}

