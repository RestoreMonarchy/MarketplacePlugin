using Dapper;
using Marketplace.Shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Marketplace.Server.Database
{
    public static class UnturnedItemsExtension
    {
        public static void AddUnturnedItem(this IDbConnection conn, UnturnedItem item)
        {
            string sql = "INSERT INTO dbo.UnturnedItems (ItemId, ItemName, ItemType, ItemDescription, Amount) " +
                "VALUES (@ItemId, @ItemName, @ItemType, @ItemDescription, @Amount);";
            using (conn)
            {
                conn.Execute(sql, item);
            }
        }

        public static void AddItemIcon(this IDbConnection conn, ushort itemId, byte[] iconData)
        {
            string sql = "UPDATE dbo.UnturnedItems SET Icon = @iconData WHERE ItemId = @itemId;";
            using (conn)
            {
                conn.Execute(sql, new { iconData, itemId = (int)itemId });
            }
        }

        public static List<UnturnedItem> GetUnturnedItems(this IDbConnection conn)
        {
            string sql = "SELECT ItemId, ItemName, ItemType, ItemDescription, Amount, " +
                "(SELECT COUNT(*) FROM dbo.MarketItems m WHERE m.ItemId = u.ItemId AND m.IsSold = 0) MarketItemsCount FROM dbo.UnturnedItems u;";
            using (conn)
            {
                return conn.Query<UnturnedItem>(sql).ToList();
            }
        }

        public static byte[] GetItemIcon(this IDbConnection conn, ushort itemId)
        {
            string sql = "SELECT Icon FROM dbo.UnturnedItems WHERE ItemId = @itemId;";
            using (conn)
            {
                return conn.QuerySingle<byte[]>(sql, new { itemId = (int)itemId });
            }
        }

        public static List<UnturnedItem> GetUnturnedItemsIds(this IDbConnection conn)
        {
            string sql = "SELECT ItemId FROM dbo.UnturnedItems;";
            using (conn)
            {
                return conn.Query<UnturnedItem>(sql).ToList();
            }
        }

        public static List<UnturnedItem> GetUnturnedItemsIdsNoIcon(this IDbConnection conn)
        {
            string sql = "SELECT ItemId FROM dbo.UnturnedItems WHERE Icon IS NULL;";
            using (conn)
            {
                return conn.Query<UnturnedItem>(sql).ToList();
            }
        }

        public static UnturnedItem GetUnturnedItem(this IDbConnection conn, int itemId)
        {
            string sql = "SELECT u.*, m.Id, m.ItemId, m.Metadata, m.Quality, m.Price, m.SellerId, m.CreateDate FROM dbo.UnturnedItems u " +
                "LEFT JOIN dbo.MarketItems m ON m.ItemId = u.ItemId AND m.IsSold = 0 WHERE u.ItemId = @itemId;";

            UnturnedItem item = null;
            using (conn)
            {
                conn.Query<UnturnedItem, MarketItem, UnturnedItem>(sql, (u, m) =>
                {
                    if (item == null)
                    {
                        item = u;
                        u.MarketItems = new List<MarketItem>();
                    }
                    if (m != null && m.SellerId != null)
                    {
                        item.MarketItems.Add(m);
                    }                    
                    return null;
                }, new { itemId });
            }
            return item;
        }
    }
}
