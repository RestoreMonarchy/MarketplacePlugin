using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace RestoreMonarchy.MarketplacePlugin.Models
{
    public class AwaitingCommand
    {
        public AwaitingCommand(string commandText, string playerId, string playerName, bool executeOnPlayerJoin = true, DateTime? executeTime = null)
        {
            CommandText = commandText;
            PlayerId = playerId;
            PlayerName = playerName;
            ExecuteOnPlayerJoin = executeOnPlayerJoin;
            ExecuteTime = executeTime;
        }
        public AwaitingCommand() { }

        public string CommandText { get; set; }
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }
        public bool ExecuteOnPlayerJoin { get; set; }
        public DateTime? ExecuteTime { get; set; }

        [NonSerialized]
        public Timer ExecuteTimer;
    }
}
