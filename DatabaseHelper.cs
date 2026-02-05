using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;

namespace MP_ModbusApp
{
    /// <summary>
    /// Static helper class for all database interactions using SQLite.
    /// Manages application settings, IP history, and device configurations.
    /// </summary>
    public static class DatabaseHelper
    {
        private static readonly string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string dbPath = Path.Combine(appData, "JuniorSoft", "MP_ModbusApp", "MP_ModbusMaster.db");
        //private static readonly string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MP_ModbusMaster.db");
        private static readonly string connectionString = $"Data Source={dbPath}";

        /// <summary>
        /// Gets the absolute path to the database file.
        /// </summary>
        public static string GetDbPath()
        {
            return dbPath;
        }

        /// <summary>
        /// Creates the initial database file and essential tables (Settings, IPAddresses)
        /// if they don't exist. Also seeds the IPAddresses table with localhost.
        /// </summary>
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

                // Ensure localhost is always available in the IP list
                command.CommandText = @"
                    INSERT OR IGNORE INTO IPAddresses (Address, LastUsed) 
                    VALUES ('127.0.0.1', datetime('now'));";
                command.ExecuteNonQuery();
            }
        }

        // --- Settings Table Methods ---

        /// <summary>
        /// Saves a key-value pair to the Settings table (uses INSERT OR REPLACE).
        /// </summary>
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

        /// <summary>
        /// Loads a setting value by key.
        /// Returns the specified default value if the key is not found or the DB file doesn't exist.
        /// </summary>
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

        // --- IPAddresses Table Methods ---

        /// <summary>
        /// Saves an IP address to the history, updating its LastUsed timestamp (uses INSERT OR REPLACE).
        /// </summary>
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

        /// <summary>
        /// Loads all saved IP addresses, ordered by the most recently used.
        /// </summary>
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

        // --- Device Configuration Table Methods ---

        /// <summary>
        /// Creates the tables required for storing device configurations (Devices, ReadingGroups, RegisterDefinitions)
        /// if they don't exist. This includes setting up Foreign Keys with ON DELETE CASCADE.
        /// </summary>
        public static void CreateDeviceTables()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();

                // Main device table
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Devices (
                        DeviceId INTEGER PRIMARY KEY AUTOINCREMENT,
                        DeviceName TEXT NOT NULL,
                        SlaveId INTEGER NOT NULL
                    );";
                command.ExecuteNonQuery();

                // Table for reading groups (tabs in the UI)
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS ReadingGroups (
                        GroupId INTEGER PRIMARY KEY AUTOINCREMENT,
                        DeviceId INTEGER NOT NULL,
                        GroupName TEXT NOT NULL,
                        FunctionCode INTEGER NOT NULL,
                        StartAddress INTEGER NOT NULL,
                        Quantity INTEGER NOT NULL,
                        FOREIGN KEY (DeviceId) REFERENCES Devices (DeviceId) ON DELETE CASCADE
                    );";
                command.ExecuteNonQuery();

                // Table for individual register definitions (name, format)
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS RegisterDefinitions (
                        RegisterId INTEGER PRIMARY KEY AUTOINCREMENT,
                        GroupId INTEGER NOT NULL,
                        RegisterNumber INTEGER NOT NULL,
                        RegisterName TEXT NOT NULL,
                        RegisterDescription TEXT NOT NULL,
                        DisplayFormatColumn TEXT NOT NULL DEFAULT 'Unsigned16', /* Stores the DisplayFormat enum string */
                        FOREIGN KEY (GroupId) REFERENCES ReadingGroups (GroupId) ON DELETE CASCADE
                    );";
                command.ExecuteNonQuery();
            }
        }
    }
}