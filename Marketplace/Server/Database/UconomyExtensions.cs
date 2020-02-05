﻿using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Marketplace.Server.Database
{
    public static class UconomyExtensions
    {
        public static void UconomyPay(this MySqlConnection conn, string playerId, decimal amount)
        {
            string sql = "UPDATE Uconomy SET balance = balance + @amount, lastUpdated = @date WHERE steamId = @playerId;";
            using (conn)
            {
                conn.Execute(sql, new { amount, date = DateTime.Now, playerId });
            }
        }

        public static decimal UconomyGetBalance(this MySqlConnection conn, string playerId)
        {
            string sql = "SELECT balance FROM Uconomy WHERE steamId = @playerId;";
            using (conn)
            {
                return conn.ExecuteScalar<decimal>(sql, new { playerId });
            }
        }


    }
}
