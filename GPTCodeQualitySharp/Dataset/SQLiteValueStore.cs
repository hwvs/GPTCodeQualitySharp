using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace GPTCodeQualitySharp.Dataset
{
    public class SQLiteValueStore : IValueStore
    {
        string _connectionString;
        // Choose the path to the database file
        // Create the database file if it doesn't exist
        // Create the table if it doesn't exist
        // StoreValue (upsert)
        // TryGetValue

        public SQLiteValueStore(string databasePath)
        {
            _connectionString = $"data source={databasePath}";

            using (var connection = new SQLiteConnection(_connectionString))
            {
                try
                {

                    // Create the table if it doesn't exist
                    connection.Open();
                    using (SQLiteTransaction trans = connection.BeginTransaction())
                    {
                        foreach (ValueStoreTable table in ValueStoreTable.Values)
                        {
                            using (SQLiteCommand command = new SQLiteCommand(connection))
                            {
                                command.CommandText = "CREATE TABLE IF NOT EXISTS [$table] (key TEXT PRIMARY KEY, value TEXT);";
                                command.Parameters.AddWithValue("$table", table.Name);
                                command.ExecuteNonQuery();
                            }
                        }

                        trans.Commit();
                    }
                }
                finally
                {
                    connection?.Close();
                }
            }

        }

        private string GetHash(IHashableData key)
        {
            return HashProvider.SHA256TruncFromString(key.ToHashableString());
        }

        public void StoreValue(ValueStoreTable table, IHashableData key, string value)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    using (SQLiteTransaction trans = connection.BeginTransaction())
                    {
                        using (SQLiteCommand command = new SQLiteCommand(connection))
                        {
                            command.CommandText = "INSERT OR REPLACE INTO [$table] (key, value) VALUES ($key, $value);";
                            command.Parameters.AddWithValue("$table", table.Name);
                            command.Parameters.AddWithValue("$key", GetHash(key));
                            command.Parameters.AddWithValue("$value", value);
                            command.ExecuteNonQuery();
                            trans.Commit();
                        }
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        public bool TryGetValue(ValueStoreTable table, IHashableData key, out string? value)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText = "SELECT value FROM [$table] WHERE key = $key;";
                        command.Parameters.AddWithValue("$table", table.Name);
                        command.Parameters.AddWithValue("$key", GetHash(key));

                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            bool hasValue = reader.Read();
                            if (hasValue)
                            {
                                value = reader.GetString(0);
                            }
                            else
                            {
                                value = null;
                            }

                            return hasValue;
                        }
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
        }




    }
}
