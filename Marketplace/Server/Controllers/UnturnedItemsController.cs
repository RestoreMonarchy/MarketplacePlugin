using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Marketplace.Server.Database;
using Marketplace.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Marketplace.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UnturnedItemsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private SqlConnection connection => new SqlConnection(_configuration.GetConnectionString("WebDatabase"));

        public UnturnedItemsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public List<UnturnedItem> GetUnturnedItems([FromQuery] bool onlyIds = false, [FromQuery] bool withNoIcons = false)
        {
            if (onlyIds)
            {
                if (withNoIcons)
                {
                    return connection.GetUnturnedItemsIdsNoIcon();  
                } else
                {
                    return connection.GetUnturnedItemsIds();
                }                
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

        [HttpPost("{itemId}/icon")]
        public void AddIcon(ushort itemId, [FromBody] UnturnedItem item)
        {
            connection.AddItemIcon(itemId, item.Icon);
        }

        [HttpGet("{itemId}/icon")]
        public IActionResult GetIcon(ushort itemId)
        {
            byte[] data = connection.GetItemIcon(itemId);
            if (data != null)
            {
                return File(data, "image/png");
            } else
            {
                return File(new byte[0] { }, "image/png");
            }            
        }

        [HttpPost]
        public void AddUnturnedItems([FromBody] UnturnedItem item)
        {
            connection.AddUnturnedItem(item);
        }
    }
}