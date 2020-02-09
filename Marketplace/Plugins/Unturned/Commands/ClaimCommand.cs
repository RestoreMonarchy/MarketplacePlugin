using Marketplace.Shared;
using Rocket.API;
using Rocket.Core.Utils;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 1 || !int.TryParse(command[0], out int id))
            {
                UnturnedChat.Say(caller, "Invalid Syntax");
                return;
            }
            UnturnedPlayer player = (UnturnedPlayer)caller;

            ThreadPool.QueueUserWorkItem(ProcessClaim);

            void ProcessClaim(object a)
            {
                MarketItem item = pluginInstance.ClaimMarketItem(id, player.Id);
                if (item == null || item.BuyerId != player.Id || item.IsClaimed)
                {
                    TaskDispatcher.QueueOnMainThread(() => UnturnedChat.Say(caller, "You already claimed this item or you are not owner!"));
                    return;
                }

                TaskDispatcher.QueueOnMainThread(() => 
                {                    
                    player.GiveItem(new Item((ushort)item.ItemId, 1, item.Quality, item.Metadata));
                    UnturnedChat.Say(caller, $"You successfully claimed your order {id}!");
                });
            }
        }
    }
}
