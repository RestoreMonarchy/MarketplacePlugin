using System.Collections.Generic;
using System.Data.SqlClient;
using Marketplace.Server.Database;
using Marketplace.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace Marketplace.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MarketItemsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private SqlConnection connection => new SqlConnection(_configuration.GetConnectionString("WebDatabase"));
        private MySqlConnection serversConnection => new MySqlConnection(_configuration.GetConnectionString("ServersDatabase")); 
        public MarketItemsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public List<MarketItem> GetMarketItems()
        {
            var marketitems = connection.GetMarketItems();
            return marketitems;
        }

        [HttpGet("{id}")]
        public MarketItem GetMarketItem(int id)
        {
            return connection.GetMarketItem(id);
        }

        [HttpPost]
        public int PostMarketItem([FromBody] MarketItem marketItem)
        {
            return connection.AddMarketItem(marketItem);
        }

        [Authorize]
        [HttpPost("{id}/buy")]
        public bool TryBuyMarketItem(int id)
        {
            decimal balance = serversConnection.UconomyGetBalance(User.Identity.Name);
            MarketItem item = connection.GetMarketItem(id);
            if (item != null && !item.IsSold && item.Price <= balance)
            {
                serversConnection.UconomyPay(User.Identity.Name, item.Price * -1);
                serversConnection.UconomyPay(item.SellerId, item.Price);
                connection.BuyMarketItem(id, User.Identity.Name);
                return true;
            }
            return false;
        }

        [Authorize]
        [HttpGet("my")]
        public List<MarketItem> GetMyMarketItems()
        {
            return connection.GetPlayerMarketItems(User.Identity.Name);
        }

        [HttpGet("{id}/claim")]
        public MarketItem ClaimMarketItem(int id, [FromQuery] string playerId)
        {
            MarketItem marketItem = connection.GetMarketItem(id);
            
            if (marketItem == null)
            {
                return new MarketItem();
            }

            if (playerId == marketItem.BuyerId && !marketItem.IsClaimed)
            {
                connection.ClaimMarketItem(id);                
            }
            return marketItem;
        }

        [Authorize]
        [HttpGet("~/mybalance")]
        public decimal GetMyBalance()
        {
            return serversConnection.UconomyGetBalance(User.Identity.Name);
        }
    }
}