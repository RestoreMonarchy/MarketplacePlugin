using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnturnedMarketplaceModule
{
    public class MarketplaceModuleConfig
    {
        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }
        public int IconSize { get; set; }

        public void LoadDefaults()
        {
            ApiUrl = "http://localhost:5046/api";
            ApiKey = "f215ab5d-e761-415e-b2de-f97db5069977";
            IconSize = 100;
        }

        public void Reload(UnturnedMarketplaceModule module)
        {
            try
            {
                if (File.Exists(module.path))
                {
                    module.Config = JsonConvert.DeserializeObject<MarketplaceModuleConfig>(File.ReadAllText(module.path));
                }
                else
                {
                    using (var stream = File.CreateText(module.path))
                    {
                        stream.Write(JsonConvert.SerializeObject(module.Config, Formatting.Indented));
                    }
                    Debug.Log($"Successfully created config file in {Directory.GetCurrentDirectory()}");
                }
            } catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
