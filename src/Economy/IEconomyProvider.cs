using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestoreMonarchy.MarketplacePlugin.Economy
{
    public interface IEconomyProvider
    {
        decimal GetPlayerBalance(string steamId);
        bool IncrementPlayerBalance(string steamId, decimal amount);
        bool Pay(string senderId, string receiverId, decimal amount);
    }
}
