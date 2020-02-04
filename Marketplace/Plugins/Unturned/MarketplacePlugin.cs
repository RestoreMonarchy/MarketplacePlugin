using Marketplace.Shared;
using Newtonsoft.Json;
using Rocket.Core.Plugins;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Logger = Rocket.Core.Logging.Logger;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnturnedMarketplacePlugin.Extensions;

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
            WebClient.Headers.Add("x-api-key", config.ApiKey);
            ThreadPool.QueueUserWorkItem((a) => this.LoadAssets());
            
            Logger.Log($"{Name} {Assembly.GetName().Version} has been loaded!", ConsoleColor.Yellow);
        }

        protected override void Unload()
        {
            Logger.Log($"{Name} has been unloaded!", ConsoleColor.Yellow);
        }
    }
}

