using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using H3C_BPMT_E.Models;

namespace H3C_BPMT_E.Services
{
    class LogService
    {
        private const string dbPath = "Data Source=log.db;Version=3;";
        public LogService()
        {
            InitializeDatabase();
        }
        private void InitializeDatabase()
        {
            using (SQLiteConnection conn = new SQLiteConnection(dbPath))
            {
                conn.Open();
                string createTableQuery = "CREATE TABLE IF NOT EXISTS Logs (Id INTEGER PRIMARY KEY, Message TEXT, Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP)";
                using (SQLiteCommand command = new SQLiteCommand(createTableQuery, conn))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public void AddDialogContent(Log LogContent)
        {
            using (SQLiteConnection conn = new SQLiteConnection(dbPath))
            {
                conn.Open();
                string insertQuery = "INSERT INTO Logs (Message, Timestamp) VALUES (@Message, @Timestamp)";
                using (SQLiteCommand command = new SQLiteCommand(insertQuery, conn))
                {
                    command.Parameters.AddWithValue("@Message", LogContent.Message);
                    command.Parameters.AddWithValue("@Timestamp", LogContent.Timestamp);
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<Log> GetAllDialogContents(DateTime startDate, DateTime endDate)
        {
            string startDateString = startDate.ToString("yyyy-MM-dd 00:00:00");
            string endDateString = endDate.ToString("yyyy-MM-dd 23:59:59");
            List<Log> LogContents = new List<Log>();
            using (SQLiteConnection conn = new SQLiteConnection(dbPath))
            {
                conn.Open();
                string selectQuery = $"SELECT * FROM Logs WHERE Timestamp BETWEEN datetime('{startDateString}') AND datetime('{endDateString}')";
                using (SQLiteCommand command = new SQLiteCommand(selectQuery, conn))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            LogContents.Add(new Log
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Message = reader["Message"].ToString(),
                                Timestamp = Convert.ToDateTime(reader["Timestamp"])
                            });
                        }
                    }
                }
            }
            return LogContents;
        }
    }
}
