﻿using Marketplace.Shared;
using Newtonsoft.Json;
using Rocket.Core.Utils;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Logger = Rocket.Core.Logging.Logger;

namespace UnturnedMarketplacePlugin.Extensions
{
    public static class MarketplacePluginExtensions
    {
        // Main thread method
        public static void LoadAssets(this MarketplacePlugin pluginInstance)
        {            
            List<ushort> existingItems = pluginInstance.GetExistingItems();
            int num = 0;
            while (existingItems == null)
            {
                existingItems = pluginInstance.GetExistingItems();
                num++;
                Logger.LogWarning($"Retrying to download existing items [{num}/5]");
                if (num == 5)
                {
                    Logger.LogWarning($"Failed to connect to web API [{pluginInstance.config.ApiUrl}]. Unloading plugin...");
                    pluginInstance.UnloadPlugin();
                    return;
                }
            }

            Logger.Log($"{existingItems.Count} assets are already existing", ConsoleColor.Yellow);

            foreach (ItemAsset asset in Assets.find(EAssetType.ITEM))
            {
                if (!string.IsNullOrEmpty(asset.itemName) && !asset.itemName.Equals("#NAME")
                    && !existingItems.Contains(asset.id))
                {
                    pluginInstance.UploadUnturnedItem(new UnturnedItem(asset.id, asset.itemName, (Marketplace.Shared.EItemType)asset.type, asset.itemDescription, asset.amount));
                }
            }
        }

        // Main Thread
        public static void UploadUnturnedItem(this MarketplacePlugin pluginInstance, UnturnedItem item)
        {
            if (pluginInstance.config.Debug)
                Logger.LogWarning($"Uploading {item.ItemName} [{item.ItemId}]...");

            string content = JsonConvert.SerializeObject(item);
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                    wc.Headers["x-api-key"] = pluginInstance.config.ApiKey;                    
                    wc.UploadString(pluginInstance.config.ApiUrl + $"/unturneditems", content);
                }
            }
            catch (WebException e)
            {
                Logger.LogException(e);
                return;
            }

            Logger.Log($"Successfully uploaded {item.ItemName} [{item.ItemId}]!", ConsoleColor.Yellow);
        }


        public static bool TryUploadMarketItem(this MarketplacePlugin pluginInstance, MarketItem marketItem)
        {
            if (pluginInstance.config.Debug)
                Logger.LogWarning($"Uploading new listing for {marketItem.ItemId} from {marketItem.SellerId}...");

            string content = JsonConvert.SerializeObject(marketItem);
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                    wc.Headers["x-api-key"] = pluginInstance.config.ApiKey;
                    wc.UploadString(pluginInstance.config.ApiUrl + $"/marketitems", content);
                }
            }
            catch (WebException e)
            {
                TaskDispatcher.QueueOnMainThread(() => Logger.LogWarning($"Failed to upload {marketItem.SellerId} listing due to {e.Status}!"));
                return false;
            }

            if (pluginInstance.config.Debug)
                TaskDispatcher.QueueOnMainThread(() => Logger.LogWarning($"Item {marketItem.ItemId} from {marketItem.SellerId} has been uploaded!"));

            return true;
        }

        public static MarketItem ClaimMarketItem(this MarketplacePlugin pluginInstance, int id, string playerId)
        {
            string content = string.Empty;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers["x-api-key"] = pluginInstance.config.ApiKey;
                    content = wc.DownloadString(pluginInstance.config.ApiUrl + $"/marketitems/{id}/claim?playerId={playerId}");
                }
            } catch (WebException e)
            {
                Logger.LogWarning($"Failed to claim market item {id} listing due to {e.Status}!");
            }            

            MarketItem marketItem = null;
            try
            {
                marketItem = JsonConvert.DeserializeObject<MarketItem>(content);
            }
            catch (Exception e)
            {
                Logger.LogException(e);
            }
            return marketItem;
        }

        public static List<ushort> GetExistingItems(this MarketplacePlugin pluginInstance)
        {
            string content = string.Empty;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    content = wc.DownloadString(pluginInstance.config.ApiUrl + "/unturneditems?onlyids=true");
                }
            } catch (WebException e)
            {
                Logger.LogWarning($"Failed to download existing items due to {e.Status}!");
            }

            List<UnturnedItem> items = null;

            try
            {
                items = JsonConvert.DeserializeObject<List<UnturnedItem>>(content);
            }
            catch (Exception e)
            {
                Logger.LogException(e);
            }

            return items == null ? null : items.Select(x => (ushort)x.ItemId).ToList();
        }
    }
}