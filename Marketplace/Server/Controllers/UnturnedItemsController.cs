using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Marketplace.Server.Database;
using Marketplace.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Marketplace.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UnturnedItemsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private SqlConnection connection => new SqlConnection(_configuration["ConnectionString"]);

        public UnturnedItemsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public List<UnturnedItem> GetUnturnedItems([FromQuery] bool onlyIds = false)
        {
            if (onlyIds)
            {
                return connection.GetUnturnedItemsIds();
            }
            else
            {
                return connection.GetUnturnedItems();
            }
        }

        [HttpGet("{itemId}")]
        public UnturnedItem GetUnturnedItem(ushort itemId)
        {
            return connection.GetUnturnedItem(itemId);
        }

        [HttpGet("{itemId}/icon")]
        public IActionResult GetIcon(ushort itemId)
        {
            return File(connection.GetItemIcon(itemId), "image/png");
        }

        [HttpPost]
        public void AddUnturnedItems([FromBody] UnturnedItem item)
        {
            connection.AddUnturnedItems(item);
        }
    }
}