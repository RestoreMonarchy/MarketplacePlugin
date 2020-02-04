using Marketplace.Shared;
using Newtonsoft.Json;
using Rocket.Core.Logging;
using Rocket.Core.Utils;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnturnedMarketplacePlugin.Extensions
{
    public static class MarketplacePluginExtensions
    {
        public static void LoadAssets(this MarketplacePlugin pluginInstance)
        {            
            List<ushort> existingItems = pluginInstance.GetExistingItems();
            int num = 0;
            while (existingItems == null)
            {
                existingItems = pluginInstance.GetExistingItems();
                num++;
                TaskDispatcher.QueueOnMainThread(() => Logger.LogWarning($"Retrying to download existing items [{num}/5]"));
                if (num == 5)
                {
                    TaskDispatcher.QueueOnMainThread(() => 
                    {
                        Logger.LogWarning($"Failed to connect to web API [{pluginInstance.config.ApiUrl}]. Unloading plugin...");
                        pluginInstance.UnloadPlugin();
                    });
                    
                    return;
                }
            }

            Logger.Log($"{existingItems.Count} assets are already existing", ConsoleColor.Yellow);
            int num1 = 0;
            foreach (ItemAsset asset in Assets.find(EAssetType.ITEM))
            {
                if (!string.IsNullOrEmpty(asset.itemName) && !asset.itemName.Equals("#NAME")
                    && !existingItems.Contains(asset.id))
                {
                    pluginInstance.UploadUnturnedItem(new UnturnedItem(asset.id, asset.itemName, 
                        (Marketplace.Shared.EItemType)asset.type, asset.itemDescription, asset.amount, new byte[0] { }));
                    num1++;
                }
            }
            Logger.Log($"Successfully uploaded {num1} new assets!", ConsoleColor.Yellow);
        }

        public static void UploadUnturnedItem(this MarketplacePlugin pluginInstance, UnturnedItem item)
        {
            if (pluginInstance.config.Debug)
                TaskDispatcher.QueueOnMainThread(() => Logger.LogWarning($"Uploading {item.ItemName} [{item.ItemId}]..."));

            string content = JsonConvert.SerializeObject(item);
            try
            {
                using (pluginInstance.WebClient)
                {
                    pluginInstance.WebClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                    pluginInstance.WebClient.UploadString(pluginInstance.config.ApiUrl + "/unturneditems", content);
                }
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.Timeout)
                {
                    TaskDispatcher.QueueOnMainThread(() => Logger.LogWarning($"Web API [{pluginInstance.config.ApiUrl}] timeout. Retrying in {pluginInstance.config.TimeoutRetryTimeMiliseconds} miliseconds..."));
                    Task.Delay(pluginInstance.config.TimeoutRetryTimeMiliseconds);
                    pluginInstance.UploadUnturnedItem(item);
                }
            }
        }

        public static bool TryUploadMarketItem(this MarketplacePlugin pluginInstance, MarketItem marketItem)
        {
            if (pluginInstance.config.Debug)
                TaskDispatcher.QueueOnMainThread(() => Logger.LogWarning($"Uploading new listing for {marketItem.ItemId} from {marketItem.SellerId}..."));

            string content = JsonConvert.SerializeObject(marketItem);
            try
            {
                using (pluginInstance.WebClient)
                {
                    pluginInstance.WebClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                    pluginInstance.WebClient.UploadString(pluginInstance.config.ApiUrl + "/marketitems", content);
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
                using (pluginInstance.WebClient)
                {
                    content = pluginInstance.WebClient.DownloadString(pluginInstance.config.ApiUrl + $"/marketitems/{id}/claim?playerId={playerId}");
                }
            } catch (WebException e)
            {
                TaskDispatcher.QueueOnMainThread(() => Logger.LogWarning($"Failed to claim market item {id} listing due to {e.Status}!"));
            }            

            MarketItem marketItem = null;
            try
            {
                marketItem = JsonConvert.DeserializeObject<MarketItem>(content);
            }
            catch (Exception e)
            {
                TaskDispatcher.QueueOnMainThread(() => Logger.LogException(e));
            }
            return marketItem;
        }

        public static List<ushort> GetExistingItems(this MarketplacePlugin pluginInstance)
        {
            string content = string.Empty;
            try
            {
                using (pluginInstance.WebClient)
                {
                    content = pluginInstance.WebClient.DownloadString(pluginInstance.config.ApiUrl + "/unturneditems?onlyids=true");
                }
            } catch (WebException e)
            {
                TaskDispatcher.QueueOnMainThread(() => Logger.LogWarning($"Failed to download existing items due to {e.Status}!"));
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
