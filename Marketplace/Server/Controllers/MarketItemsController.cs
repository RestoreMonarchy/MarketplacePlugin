using System.Collections.Generic;
using System.Data.SqlClient;
using Marketplace.Server.Database;
using Marketplace.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Marketplace.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MarketItemsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private SqlConnection connection => new SqlConnection(_configuration["ConnectionString"]);
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
            decimal balance = 10;
            MarketItem item = connection.GetMarketItem(id);
            if (item != null && !item.IsSold && item.Price <= balance)
            {
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
    }
}