using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Logger = Rocket.Core.Logging.Logger;
using Marketplace.Shared;
using UnturnedMarketplacePlugin.Extensions;
using System.Threading;
using Rocket.Core.Utils;

namespace UnturnedMarketplacePlugin.Commands
{
    public class SellCommand : IRocketCommand
    {
        private MarketplacePlugin pluginInstance => MarketplacePlugin.Instance;

        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "sell";

        public string Help => "Starts a sell process";

        public string Syntax => "<price>";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;

            if (command.Length < 1 || !decimal.TryParse(command[0], out decimal price))
            {
                UnturnedChat.Say(player, "Price is invalid");
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
                    var item = new MarketItem(jar.item.id, price, jar.item.quality, jar.item.amount, jar.item.state, player.Id);
                    ThreadPool.QueueUserWorkItem((a) => 
                    {
                        ItemAsset asset = Assets.find(EAssetType.ITEM, (ushort)item.ItemId) as ItemAsset;
                        if (pluginInstance.TryUploadMarketItem(item))
                        {
                            TaskDispatcher.QueueOnMainThread(() =>
                            {
                                UnturnedChat.Say(player, $"Successfully put your {asset.itemName} on marketplace!", Color.cyan);
                            });
                        } else
                        {
                            TaskDispatcher.QueueOnMainThread(() => 
                            {
                                player.GiveItem(jar.item);
                                UnturnedChat.Say(player, $"Your {asset.itemName} returned. Try again later.");
                            });
                        }
                    });
                    
                    interactableStorage.items.clear();                    
                    player.Inventory.closeStorageAndNotifyClient();
                    Object.Destroy(interactableStorage);

                    player.Inventory.onInventoryAdded -= inventoryAdded;
                    player.Inventory.onInventoryResized -= inventoryResized;
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
    }
}
