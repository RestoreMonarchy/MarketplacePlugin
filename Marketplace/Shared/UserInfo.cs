using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Shared
{
    public class UserInfo
    {
        public string SteamId { get; set; }
        public string Role { get; set; }
        public decimal Balance { get; set; }
        public bool IsAuthenticated { get; set; }
    }
}
