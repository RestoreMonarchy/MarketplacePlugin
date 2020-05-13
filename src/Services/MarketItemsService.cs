using Marketplace.Shared;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using RestoreMonarchy.MarketplacePlugin.Utilities;
using RestoreMonarchy.MarketplacePlugin.Logging;

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
                    return;
                }   

                var existingItemsId = existingItems.Select(x => x.ItemId);
                ServiceLogger.LogInformation<MarketItemsService>($"There are {existingItems.Count()} already existing items");
                
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
                ServiceLogger.LogInformation<MarketItemsService>($"{num} items have been uploaded!");
            } catch (Exception e)
            {
                ServiceLogger.LogError<MarketItemsService>(e);
            }
        }

        public async Task<HttpStatusCode> UploadMarketItemAsync(MarketItem marketItem)
        {
            ServiceLogger.LogDebug<MarketItemsService>($"Uploading new listing for {marketItem.ItemId} from {marketItem.SellerId}...");
            var response = await httpClient.PostAsJsonAsync("marketitems", marketItem);
            ServiceLogger.LogInformation<MarketItemsService>($"{marketItem.SellerName}[{marketItem.SellerId}] listing for {marketItem.ItemId} has been uploaded!");
            return response.StatusCode;
        }

        public async Task<MarketItem> ClaimMarketItem(int id, string playerId)
        {
            return await httpClient.GetFromJsonAsync<MarketItem>($"marketitems/{id}/claim?playerId={playerId}");
        }

        public async Task<HttpStatusCode> UploadUnturnedItemAsync(UnturnedItem unturnedItem)
        {
            var response = await httpClient.PostAsJsonAsync("unturneditems", unturnedItem);
            ServiceLogger.LogDebug<MarketItemsService>($"{unturnedItem.ItemName}[{unturnedItem.ItemId}] has been uploaded!");
            return response.StatusCode;
        }

        void OnDestroy()
        {

        }
    }
}
