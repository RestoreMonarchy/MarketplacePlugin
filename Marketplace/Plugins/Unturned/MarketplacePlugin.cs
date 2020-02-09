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
            WebClient.Headers.Add("x-api-key", config.ApiKey);
			this.LoadAssets();



			//var arr = ItemTool.captureIcon(itemAsset.id, 0, itemAsset.item.transform, itemAsset.item.GetComponent("Icon").transform, itemAsset.size_x, itemAsset.size_y, itemAsset.size_z, true);



			//IconUtils.captureItemIcon(itemAsset);
			//string path = string.Concat(new object[]
			//{
			//	ReadWrite.PATH,
			//	"/Extras/Icons/",
			//	itemAsset.name,
			//	"_",
			//	itemAsset.id
			//});
			//Console.WriteLine(path);
			//ThreadPool.QueueUserWorkItem(async delegate (object a)
			//{
			//	await Task.Delay(4000);
			//	try
			//	{
			//		byte[] arr = File.ReadAllBytes(path + ".png");
			//		File.WriteAllBytes(this.Directory + "/test.png", arr);
			//		arr = null;
			//	}
			//	catch (Exception e)
			//	{
			//		Console.WriteLine(e);
			//	}
			//});

			Logger.Log($"{Name} {Assembly.GetName().Version} has been loaded!", ConsoleColor.Yellow);
        }

		[RocketCommand("getIcon", "idk")]
		public void GetIconCommand(Rocket.API.IRocketPlayer caller, string[] args)
		{			
			ItemAsset itemAsset = (ItemAsset)Assets.find(EAssetType.ITEM, ushort.Parse(args[0]));
			
		}

        protected override void Unload()
        {
            Logger.Log($"{Name} has been unloaded!", ConsoleColor.Yellow);
        }
    }
}

