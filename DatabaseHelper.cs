// Plik: DatabaseHelper.cs

using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;

namespace MP_ModbusApp
{
    public static class DatabaseHelper
    {
        private static readonly string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MP_ModbusMaster.db");
        private static readonly string connectionString = $"Data Source={dbPath}";

        public static void InitializeDatabase()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();

                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Settings (
                        Key TEXT PRIMARY KEY,
                        Value TEXT NOT NULL
                    );";
                command.ExecuteNonQuery();

                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS IPAddresses (
                        Address TEXT PRIMARY KEY,
                        LastUsed DATETIME NOT NULL
                    );";
                command.ExecuteNonQuery();

                command.CommandText = @"
                    INSERT OR IGNORE INTO IPAddresses (Address, LastUsed) 
                    VALUES ('127.0.0.1', datetime('now'));";
                command.ExecuteNonQuery();


            }
        }

        // --- Metody dla tabeli Settings (bez zmian) ---
        public static void SaveSetting(string key, string value)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT OR REPLACE INTO Settings (Key, Value) VALUES ($key, $value);";
                command.Parameters.AddWithValue("$key", key);
                command.Parameters.AddWithValue("$value", value ?? string.Empty);
                command.ExecuteNonQuery();
            }
        }

        public static string LoadSetting(string key, string defaultValue)
        {
            if (!File.Exists(dbPath)) return defaultValue;
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Value FROM Settings WHERE Key = $key;";
                command.Parameters.AddWithValue("$key", key);
                var result = command.ExecuteScalar();
                return result != null ? result.ToString() : defaultValue;
            }
        }

        // --- Metody dla tabeli IPAddresses (bez zmian) ---
        public static void SaveIpAddress(string address)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT OR REPLACE INTO IPAddresses (Address, LastUsed) 
                    VALUES ($address, datetime('now'));";
                command.Parameters.AddWithValue("$address", address);
                command.ExecuteNonQuery();
            }
        }

        public static List<string> LoadIpAddresses()
        {
            var addresses = new List<string>();
            if (!File.Exists(dbPath)) return addresses;

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Address FROM IPAddresses ORDER BY LastUsed DESC;";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        addresses.Add(reader.GetString(0));
                    }
                }
            }
            return addresses;
        }
    }
}