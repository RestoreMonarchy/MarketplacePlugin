using Marketplace.Shared;
using Newtonsoft.Json;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;
using UnturnedMarketplacePlugin.Extensions;
using Logger = Rocket.Core.Logging.Logger;

namespace UnturnedMarketplacePlugin.Services
{
    public class MarketItemsService : MonoBehaviour
    {
        private MarketplacePlugin pluginInstance => MarketplacePlugin.Instance;
        private WebClient webClient = new WebClient();

        void Awake()
        {
            var existingItems = webClient.DownloadJson<List<UnturnedItem>>(pluginInstance.config.ApiUrl + "/unturneditems").Select(x => x.ItemId);
            Logger.Log($"There are {existingItems.Count()} already existing items!");
            foreach (ItemAsset asset in Assets.find(EAssetType.ITEM))
            {
                if (!string.IsNullOrEmpty(asset.itemName) && !asset.itemName.Equals("#NAME") && !existingItems.Contains(asset.id))
                {
                    try
                    {
                        UploadUnturnedItem(new UnturnedItem(asset.id, asset.itemName, (Marketplace.Shared.EItemType)asset.type,
                            asset.itemDescription, asset.amount));
                    } catch (Exception e)
                    {
                        Logger.LogException(e);
                    }                    
                }
            }
        }

        public HttpStatusCode UploadMarketItem(MarketItem marketItem)
        {
            if (pluginInstance.config.Debug)
                Logger.LogWarning($"Uploading new listing for {marketItem.ItemId} from {marketItem.SellerId}...");

            string content = JsonConvert.SerializeObject(marketItem);
            var data = Encoding.ASCII.GetBytes(content);

            var request = WebRequest.Create(pluginInstance.config.ApiUrl + "/marketitems") as HttpWebRequest;
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;
            request.Headers["x-api-key"] = pluginInstance.config.ApiKey;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                return response.StatusCode;
            }
        }

        public MarketItem ClaimMarketItem(int id, string playerId)
        {
            webClient.Headers["x-api-key"] = pluginInstance.config.ApiKey;
            return webClient.DownloadJson<MarketItem>(pluginInstance.config.ApiUrl + $"/marketitems/{id}/claim?playerId={playerId}");
        }

        public HttpStatusCode UploadUnturnedItem(UnturnedItem unturnedItem)
        {
            if (pluginInstance.config.Debug)
                Logger.LogWarning($"Uploading UnturnedItem {unturnedItem.ItemName}[{unturnedItem.ItemId}]...");

            string content = JsonConvert.SerializeObject(unturnedItem);
            var data = Encoding.ASCII.GetBytes(content);
            
            var request = WebRequest.Create(pluginInstance.config.ApiUrl + "/unturneditems") as HttpWebRequest;
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;
            request.Headers["x-api-key"] = pluginInstance.config.ApiKey;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                return response.StatusCode;
            }
        }

        void OnDestroy()
        {

        }
    }
}
