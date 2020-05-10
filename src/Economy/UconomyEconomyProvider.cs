using fr34kyn01535.Uconomy;
using SDG.Unturned;

namespace RestoreMonarchy.MarketplacePlugin.Economy
{
    public class UconomyEconomyProvider : IEconomyProvider
    {
        private MarketplacePlugin pluginInstance => MarketplacePlugin.Instance;
        private Uconomy uconomyPlugin => pluginInstance.EconomyPlugin as Uconomy;

        public decimal GetPlayerBalance(string steamId)
        {
            return uconomyPlugin.Database.GetBalance(steamId);
        }

        public bool IncrementPlayerBalance(string steamId, decimal amount)
        {
            var balance = GetPlayerBalance(steamId);
            if (balance + amount < 0)
                return false;

            uconomyPlugin.Database.IncreaseBalance(steamId, amount);
            return true;
        }

        public bool Pay(string senderId, string receiverId, decimal amount)
        {
            var balance = GetPlayerBalance(senderId);
            if (balance - amount < 0)
                return false;

            uconomyPlugin.Database.IncreaseBalance(senderId, -amount);
            uconomyPlugin.Database.IncreaseBalance(receiverId, amount);
            return true;
        }
    }
}
