using Marketplace.Shared;
using Rocket.API;
using Rocket.Core.Utils;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;
using System.Threading;
using UnturnedMarketplacePlugin.Extensions;

namespace UnturnedMarketplacePlugin.Commands
{
    public class ClaimCommand : IRocketCommand
    {
        private MarketplacePlugin pluginInstance => MarketplacePlugin.Instance;

        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "claim";

        public string Help => "Claims your bought market item";

        public string Syntax => "<id>";

        public List<string> Aliases => new List<string>() { "marketclaim" };

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 1 || !int.TryParse(command[0], out int id))
            {
                UnturnedChat.Say(caller, pluginInstance.Translate("ClaimInvalid"), pluginInstance.MessageColor);
                return;
            }
            UnturnedPlayer player = (UnturnedPlayer)caller;

            ThreadPool.QueueUserWorkItem(ProcessClaim);

            void ProcessClaim(object a)
            {
                MarketItem item = pluginInstance.ClaimMarketItem(id, player.Id);
                if (item == null || item.BuyerId != player.Id || item.IsClaimed)
                {
                    TaskDispatcher.QueueOnMainThread(() => UnturnedChat.Say(caller, pluginInstance.Translate("ClaimedAlready"), pluginInstance.MessageColor));
                    return;
                }

                TaskDispatcher.QueueOnMainThread(() => 
                {
                    var asset = Assets.find(EAssetType.ITEM, (ushort)item.ItemId) as ItemAsset;
                    if (asset != null)
                    {
                        player.Inventory.forceAddItem(new Item(asset.id, item.Amount, item.Quality, item.Metadata), true);
                        UnturnedChat.Say(caller, pluginInstance.Translate("ClaimSuccess", asset.itemName, id), pluginInstance.MessageColor);
                    }
                });
            }
        }
    }
}
