using Marketplace.Shared;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;
using RestoreMonarchy.MarketplacePlugin.Utilities;
using Rocket.Core.Logging;

namespace RestoreMonarchy.MarketplacePlugin.Services
{
    public class MarketItemsService : MonoBehaviour
    {
        private MarketplacePlugin pluginInstance => MarketplacePlugin.Instance;
        private MarketplaceHttpClient httpClient { get; } = new MarketplaceHttpClient();

        void Awake()
        {
            Task.Run(AwakeAsync).Wait();       
        }

        private async Task AwakeAsync()
        {
            try
            {
                var existingItems = await httpClient.GetFromJsonAsync<IEnumerable<UnturnedItem>>("unturneditems");
                if (existingItems == null)
                {
                    Logger.LogWarning("Failed to download existing items from Web API");
                    return;
                }   

                var existingItemsId = existingItems.Select(x => x.ItemId);                
                Logger.Log($"There are {existingItems.Count()} already existing items", ConsoleColor.Green);
                
                int num = 0;
                foreach (ItemAsset asset in Assets.find(EAssetType.ITEM))
                {
                    if (!string.IsNullOrEmpty(asset.itemName) && !asset.itemName.Equals("#NAME") && !existingItemsId.Contains(asset.id))
                    {
                        await UploadUnturnedItemAsync(new UnturnedItem(asset.id, asset.itemName, (Marketplace.Shared.EItemType)asset.type,
                            asset.itemDescription, asset.amount));
                        num++;
                    }
                }
                Logger.Log($"{num} items have been uploaded!", ConsoleColor.Green);
            } catch (Exception e)
            {
                Logger.LogException(e);
            }
        }

        public async Task<HttpStatusCode> UploadMarketItemAsync(MarketItem marketItem)
        {
            if (pluginInstance.config.Debug)
                Logger.LogWarning($"Uploading new listing for {marketItem.ItemId} from {marketItem.SellerId}...");

            var response = await httpClient.PostAsJsonAsync("marketitems", marketItem);
            return response.StatusCode;
        }

        public async Task<MarketItem> ClaimMarketItem(int id, string playerId)
        {
            return await httpClient.GetFromJsonAsync<MarketItem>($"marketitems/{id}/claim?playerId={playerId}");
        }

        public async Task<HttpStatusCode> UploadUnturnedItemAsync(UnturnedItem unturnedItem)
        {
            if (pluginInstance.config.Debug)
                Logger.LogWarning($"Uploading UnturnedItem {unturnedItem.ItemName}[{unturnedItem.ItemId}]...");

            var response = await httpClient.PostAsJsonAsync("unturneditems", unturnedItem);
            return response.StatusCode;
        }

        void OnDestroy()
        {

        }
    }
}
