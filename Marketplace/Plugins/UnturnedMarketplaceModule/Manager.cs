using Marketplace.Shared;
using Newtonsoft.Json;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;

namespace UnturnedMarketplaceModule
{
    public class Manager : MonoBehaviour
    {
        void Start()
        {
            DontDestroyOnLoad(this);
            Player.onPlayerCreated += (p) => LoadItems();
        }

        public void LoadItems()
        {
            UnturnedMarketplaceModule.Instance.Config.Reload(UnturnedMarketplaceModule.Instance);
            List<ushort> noIconItems = GetItemsWithNoIcons();
            Debug.Log($"{noIconItems.Count} items do not have icons");

            int num1 = 0;
            foreach (ushort itemId in noIconItems)
            {
                ItemAsset asset = Assets.find(EAssetType.ITEM, itemId) as ItemAsset;
                var ready = new ItemIconReady((icon) =>
                {
                    UploadUnturnedItemIcon(asset.id, icon.EncodeToPNG());
                });

                ItemTool.getIcon(asset.id, 0, asset.quality, asset.getState(), asset, null, string.Empty, string.Empty,
                    asset.size_x * UnturnedMarketplaceModule.Instance.Config.IconSize, asset.size_y * UnturnedMarketplaceModule.Instance.Config.IconSize, false, true, ready);
                num1++;
            }
        }

        public void UploadUnturnedItemIcon(ushort itemId, byte[] icon)
        {
            Debug.Log($"Uploading icon for {itemId}...");
            string content = JsonConvert.SerializeObject(new UnturnedItem() { Icon = icon });

            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                    wc.Headers["x-api-key"] = UnturnedMarketplaceModule.Instance.Config.ApiKey;
                    wc.UploadString(UnturnedMarketplaceModule.Instance.Config.ApiUrl + $"/unturneditems/{itemId}/icon", content);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            Debug.Log($"Successfully uploaded icon for item {itemId}!");
        }

        public List<ushort> GetItemsWithNoIcons()
        {
            string content = string.Empty;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    content = wc.DownloadString(UnturnedMarketplaceModule.Instance.Config.ApiUrl + "/unturneditems?onlyIds=true&withNoIcons=true");
                }
            }
            catch (WebException e)
            {
                Debug.Log($"Failed to download items with no icon due to {e.Status}!");
            }

            List<UnturnedItem> items = null;

            try
            {
                items = JsonConvert.DeserializeObject<List<UnturnedItem>>(content);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

            return items == null ? null : items.Select(x => (ushort)x.ItemId).ToList();
        }

        void OnDestroy()
        {
            Player.onPlayerCreated -= (p) => LoadItems();
        }
    }
}
