using System.Data.SQLite;
using System.IO;

namespace FoxEEEControl
{
    static class SQLiteHandler
    {
        private static SQLiteConnection sqlite_conn;
        public static object lockObject = new object();

        public static bool DBExists()
        {
            return File.Exists("Program.db");
        }

        public static void Open()
        {
            Close();
            sqlite_conn = new SQLiteConnection("data source=\"Program.db\"");
            sqlite_conn.Open();
        }

        public static SQLiteCommand GetCommand()
        {
            return new SQLiteCommand(sqlite_conn);
        }

        public static void Close()
        {
            try
            {
                sqlite_conn.Close();
            }
            catch { }
        }
    }
}
