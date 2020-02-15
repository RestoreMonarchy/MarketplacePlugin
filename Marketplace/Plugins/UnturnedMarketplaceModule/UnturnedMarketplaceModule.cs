using SDG.Framework.Modules;
using SDG.Unturned;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnturnedMarketplaceModule
{
    public class UnturnedMarketplaceModule : IModuleNexus
    {
        public static GameObject gameObject;
        public static UnturnedMarketplaceModule Instance { get; private set; }
        public void initialize()
        {
            Instance = this;
            Debug.Log("Initialized is called right");

            gameObject = new GameObject("UnturnedMarketplaceManager");
            Object.DontDestroyOnLoad(gameObject);
            gameObject.AddComponent<Manager>();  
        }

        public void shutdown()
        {

        }
    }
}
