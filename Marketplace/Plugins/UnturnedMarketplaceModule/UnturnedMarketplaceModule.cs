using Newtonsoft.Json;
using SDG.Framework.Modules;
using System;
using System.IO;
using UnityEngine;

namespace UnturnedMarketplaceModule
{
    public class UnturnedMarketplaceModule : IModuleNexus
    {
        public static GameObject gameObject;
        public static UnturnedMarketplaceModule Instance { get; private set; }
        public string path = Path.Combine(Directory.GetCurrentDirectory(), "marketplace-config.json");
        public MarketplaceModuleConfig Config { get; set; }
        public void initialize()
        {
            Instance = this;

            Config = new MarketplaceModuleConfig();
            Config.LoadDefaults();
            Config.Reload(this);

            gameObject = new GameObject("UnturnedMarketplaceManager");
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            gameObject.AddComponent<Manager>();  
        }

        public void shutdown()
        {
            UnityEngine.Object.Destroy(gameObject.GetComponent<Manager>());
        }
    }
}
