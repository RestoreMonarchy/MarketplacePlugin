using Dapper;
using Marketplace.Shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Marketplace.Server.Database
{
    public static class MarketItemsExtension
    {
        public static List<MarketItem> GetMarketItems(this IDbConnection conn)
        {
            string sql = "SELECT * FROM dbo.MarketItems;";
            using (conn)
            {
                return conn.Query<MarketItem>(sql).ToList();
            }
        }

        public static MarketItem GetMarketItem(this IDbConnection conn, int id)
        {
            string sql = "SELECT * FROM dbo.MarketItems WHERE Id = @id;";
            using (conn)
            {
                return conn.Query<MarketItem>(sql, new { id }).FirstOrDefault();
            }
        }

        public static int AddMarketItem(this IDbConnection conn, MarketItem marketItem)
        {
            string sql = "INSERT INTO dbo.MarketItems (ItemId, Quality, Metadata, Price, SellerId) " +
                "VALUES (@ItemId, @Quality, @Metadata, @Price, @SellerId);";
            using (conn)
            {                
                return conn.ExecuteScalar<int>(sql, marketItem);
            }
        }

        public static void BuyMarketItem(this IDbConnection conn, int id, string buyerId)
        {
            string sql = "UPDATE dbo.MarketItems SET IsSold = 1, BuyerId = @buyerId, SoldDate = SYSDATETIME() WHERE Id = @id;";
            using (conn)
            {
                conn.Execute(sql, new { id, buyerId });
            }
        }

        public static List<MarketItem> GetPlayerMarketItems(this IDbConnection conn, string playerId)
        {
            string sql = "SELECT m.*, u.ItemName, u.ItemType, u.ItemDescription, u.Amount, u.Icon FROM dbo.MarketItems m " +
                "LEFT JOIN dbo.UnturnedItems u ON m.ItemId = u.ItemId WHERE BuyerId = @playerId OR SellerId = @playerId;";

            using (conn)
            {
                return conn.Query<MarketItem, UnturnedItem, MarketItem>(sql, (m, u) => 
                {
                    m.Item = u;
                    m.Item.ItemId = m.ItemId;
                    return m;
                }, new { playerId }, splitOn: "ItemName").ToList();
            }
        }

        public static void ClaimMarketItem (this IDbConnection conn, int id)
        {
            string sql = "UPDATE dbo.MarketItems SET IsClaimed = 1, ClaimDate = SYSDATETIME() WHERE Id = @id;";

            using (conn)
            {
                conn.Execute(sql, new { id });
            }
        }
    }
}
