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
using UnityEngine;
using System.IO;

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
            //ThreadPool.QueueUserWorkItem((a) => this.LoadAssets());           





            //ItemAsset itemAsset = (ItemAsset)Assets.find(EAssetType.ITEM, 363);
            //Console.WriteLine("is pro " + itemAsset.isPro);
            //Console.WriteLine("pro path" + itemAsset.proPath);

            //var defIcon = IconUtils.getItemDefIcon(363, 0, 0);
            //Console.WriteLine($"Extras Path: {defIcon.extraPath}");

            IconUtils.captureItemIcon(itemAsset);
            string path = string.Concat(new object[]
            {
                ReadWrite.PATH,
                "/Extras/Icons/",
                itemAsset.name,
                "_",
                itemAsset.id
            });

            Console.WriteLine(path);

            ThreadPool.QueueUserWorkItem(async (a) => 
            {
                await Task.Delay(4000);
                try
                {
                    byte[] arr = File.ReadAllBytes(path + ".png");
                    File.WriteAllBytes(Directory + "/test.png", arr);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);

                }
                Console.WriteLine();
                try
                {
                    byte[] arr2 = ReadWrite.readBytes(path, false, false);
                    File.WriteAllBytes(Directory + "/test2.png", arr2);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });



//ItemAsset itemAsset = (ItemAsset)Assets.find(EAssetType.ITEM, 363);
//var ready = new ItemIconReady((icon) =>
//{
//    UploadItem(new UnturnedItem(itemAsset.id, itemAsset.itemName, (Marketplace.Shared.EItemType)itemAsset.type,
//                                itemAsset.itemDescription, itemAsset.amount, icon.EncodeToPNG()));
//});
//ItemTool.getIcon(itemAsset.id, itemAsset.quality, itemAsset.getState(), ready);




            //ItemTool.getIcon(363, 100, itemAsset.getState(), ready);


            //void ready(Texture2D text)
            //{
            //    RenderTexture tmp = RenderTexture.GetTemporary(
            //        text.width,
            //        text.height,
            //        0,
            //        RenderTextureFormat.Default,
            //        RenderTextureReadWrite.Linear);

            //    // Blit the pixels on texture to the RenderTexture
            //    Graphics.Blit(text, tmp);

            //    RenderTexture previous = RenderTexture.active;
            //    RenderTexture.active = tmp;
            //    Texture2D myTexture2D = new Texture2D(text.width, text.height);
            //    myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            //    myTexture2D.Apply();
            //    RenderTexture.active = previous;
            //    RenderTexture.ReleaseTemporary(tmp);

            //    Console.WriteLine("I'm ready !!!");
            //    byte[] bytes = myTexture2D.EncodeToPNG();
            //    File.WriteAllBytes(Directory + "/test.png", bytes);
            //}


            var cache = typeof(ItemTool).GetField("cache", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as Dictionary<ushort, Texture2D>;

            Console.WriteLine(cache.Count + $" count");
            
            Logger.Log($"{Name} {Assembly.GetName().Version} has been loaded!", ConsoleColor.Yellow);
        }

        protected override void Unload()
        {
            Logger.Log($"{Name} has been unloaded!", ConsoleColor.Yellow);
        }
    }
}

