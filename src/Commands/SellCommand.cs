﻿using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using Marketplace.Shared;
using Rocket.Core.Utils;
using System.Net;
using System.Threading.Tasks;

namespace RestoreMonarchy.MarketplacePlugin.Commands
{
    public class SellCommand : IRocketCommand
    {
        public SellCommand(string name)
        {
            Name = name;
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;

            if (command.Length < 1 || !decimal.TryParse(command[0], out decimal price))
            {
                UnturnedChat.Say(player, pluginInstance.Translate("SellInvalid"), pluginInstance.MessageColor, true);
                return;
            }

            var interactableStorage = new InteractableStorage();
            typeof(InteractableStorage).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(interactableStorage, new Items(PlayerInventory.STORAGE));
            interactableStorage.isOpen = true;
            interactableStorage.items.resize(8, 8);

            player.Inventory.isStoring = true;
            player.Inventory.storage = interactableStorage;
            player.Inventory.updateItems(PlayerInventory.STORAGE, interactableStorage.items);
            player.Inventory.sendStorage();

            InventoryAdded inventoryAdded = null;
            InventoryResized inventoryResized = null;

            inventoryAdded = delegate (byte page, byte index, ItemJar jar)
            {
                if (page == PlayerInventory.STORAGE && player.Inventory.storage == interactableStorage)
                {                    
                    var item = new MarketItem(jar.item.id, price, jar.item.quality, jar.item.amount, jar.item.state, player.Id) 
                    { 
                        SellerName = player.SteamName
                    };

                    Task.Run(ProcessSell).ContinueWith(c => 
                    {
                        TaskDispatcher.QueueOnMainThread(c.Result);
                    });

                    async Task<System.Action> ProcessSell()
                    {
                        var responseStatus = await pluginInstance.MarketItemsService.UploadMarketItemAsync(item);
                        void CloseInventory()
                        {
                            interactableStorage.items.clear();
                            player.Inventory.closeStorageAndNotifyClient();
                            Object.Destroy(interactableStorage);

                            player.Inventory.onInventoryAdded -= inventoryAdded;
                            player.Inventory.onInventoryResized -= inventoryResized;
                        };
                            

                        switch (responseStatus)
                        {
                            case HttpStatusCode.OK:
                                return () =>
                                {
                                    CloseInventory();
                                    ItemAsset asset = Assets.find(EAssetType.ITEM, (ushort)item.ItemId) as ItemAsset;
                                    UnturnedChat.Say(player, pluginInstance.Translate("SellSuccess", asset.itemName, price), pluginInstance.MessageColor, true);
                                };
                            case HttpStatusCode.Conflict:
                                return () =>
                                {
                                    CloseInventory();
                                    ItemAsset asset = Assets.find(EAssetType.ITEM, (ushort)item.ItemId) as ItemAsset;
                                    player.Inventory.forceAddItem(jar.item, true);
                                    UnturnedChat.Say(player, pluginInstance.Translate("SellLimit", asset.itemName), pluginInstance.MessageColor, true);
                                };
                            default:
                                return () =>
                                {
                                    CloseInventory();
                                    ItemAsset asset = Assets.find(EAssetType.ITEM, (ushort)item.ItemId) as ItemAsset;
                                    player.Inventory.forceAddItem(jar.item, true);
                                    UnturnedChat.Say(player, pluginInstance.Translate("SellTimeout", asset.itemName), pluginInstance.MessageColor, true);
                                };
                        }
                    }
                        
                }
            };
            
            inventoryResized = delegate (byte page, byte newWidth, byte newHeight)
            {
                Object.Destroy(interactableStorage);
                player.Inventory.onInventoryResized -= inventoryResized;
                player.Inventory.onInventoryAdded -= inventoryAdded;
            };

            player.Inventory.onInventoryAdded += inventoryAdded;
            player.Inventory.onInventoryResized += inventoryResized;
        }

        private MarketplacePlugin pluginInstance => MarketplacePlugin.Instance;

        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name { get; }

        public string Help => "Starts a sell process";

        public string Syntax => "<price>";

        public List<string> Aliases => new List<string>() { "marketsell" };

        public List<string> Permissions => new List<string>();
    }
}
