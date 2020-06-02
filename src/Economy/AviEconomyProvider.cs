using com.aviadmini.rocketmod.AviEconomy;

namespace RestoreMonarchy.MarketplacePlugin.Economy
{
    public class AviEconomyProvider : IEconomyProvider
    {        
        public decimal GetPlayerBalance(string steamId)
        {
            return Bank.GetBalance(steamId);
        }

        public bool IncrementPlayerBalance(string steamId, decimal amount)
        {
            const string reason = "Marketplace IncrementPlayerBalance";

            EBankOperationResult result;
            if (amount < 0)
            {
                result = Bank.PayToServer(steamId, -amount, true, out _, reason);
            } else
            {
                Bank.PayAsServer(steamId, amount, true, out _, reason);
                result = EBankOperationResult.SUCCESS;
            }

            return result == EBankOperationResult.SUCCESS;
        }

        public bool Pay(string senderId, string receiverId, decimal amount)
        {            
            const string reason = "Marketplace Pay";
            
            var result = Bank.PayAsPlayer(senderId, receiverId, amount, true, out _, out _, reason);
            return result == EBankOperationResult.SUCCESS;
        }
    }
}
