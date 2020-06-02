using Marketplace.WebSockets.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestoreMonarchy.MarketplacePlugin.Logging
{
    public class RocketWebSocketsLogger : IWebSocketsLogger
    {
        public Task Log(string msg)
        {
            ServiceLogger.LogInformation<IWebSocketsLogger>(msg);
            return Task.CompletedTask;
        }

        public Task LogDebug(string msg)
        {
            ServiceLogger.LogDebug<IWebSocketsLogger>(msg);
            return Task.CompletedTask;
        }

        public Task LogError(Exception e, string msg)
        {
            ServiceLogger.LogError<IWebSocketsLogger>(e, msg);
            return Task.CompletedTask;
        }
    }
}
