using System.Collections.Generic;
using ApiKeyAuthentication;
using DatabaseManager;
using Marketplace.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UnturnedItemsController : ControllerBase
    {
        private readonly IDatabaseManager _databaseManager;

        public UnturnedItemsController(IDatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        [HttpGet]
        public List<UnturnedItem> GetUnturnedItems([FromQuery] bool onlyIds = false, [FromQuery] bool withNoIcons = false)
        {
            if (onlyIds)
            {
                if (withNoIcons)
                {
                    return _databaseManager.GetUnturnedItemsIdsNoIcon();  
                } else
                {
                    return _databaseManager.GetUnturnedItemsIds();
                }                
            }
            else
            {
                return _databaseManager.GetUnturnedItems();
            }
        }

        [HttpGet("{itemId}")]
        public UnturnedItem GetUnturnedItem(ushort itemId)
        {
            return _databaseManager.GetUnturnedItem(itemId);
        }

        [ApiKeyAuth]
        [HttpPost("{itemId}/icon")]
        public void AddIcon(ushort itemId, [FromBody] UnturnedItem item)
        {
            _databaseManager.AddItemIcon(itemId, item.Icon);
        }

        [HttpGet("{itemId}/icon")]
        public IActionResult GetIcon(ushort itemId)
        {
            byte[] data = _databaseManager.GetItemIcon(itemId);
            if (data != null)
            {
                return File(data, "image/png");
            } else
            {
                return File(new byte[0] { }, "image/png");
            }            
        }

        [ApiKeyAuth]
        [HttpPost]        
        public void AddUnturnedItems([FromBody] UnturnedItem item)
        {
            _databaseManager.AddUnturnedItem(item);
        }
    }
}