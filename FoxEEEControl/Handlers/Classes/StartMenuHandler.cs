using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Security;
using FoxEEEControl.Handlers.Types;

namespace FoxEEEControl.Handlers.Classes
{
    class StartMenuHandler : GenericHandler
    {
        private readonly char[] pathSep = { '/', '\\' };
        private Dictionary<string, string> availableStuff = new Dictionary<string, string>();

        public override void Initialize(object param)
        {
            SQLiteCommand sqlite_cmd = SQLiteHandler.GetCommand();
            lock (SQLiteHandler.lockObject)
            {
                sqlite_cmd.CommandText = "CREATE TABLE IF NOT EXISTS programs(name TEXT PRIMARY KEY, path TEXT, count INT)";
                sqlite_cmd.ExecuteNonQuery();

                if ((bool)param)
                {
                    List<string> files = new List<string>();
                    __SearchRec(files, Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "*.lnk");
                    __SearchRec(files, Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "*.lnk");

                    string dispName;
                    foreach (string file in files)
                    {
                        try
                        {
                            dispName = file.Remove(file.LastIndexOf('.'));
                            dispName = dispName.Substring(dispName.LastIndexOfAny(pathSep) + 1);
                            if (availableStuff.ContainsKey(dispName)) continue;
                            sqlite_cmd.CommandText = "INSERT INTO programs (name, path, count) VALUES (\"" + SecurityElement.Escape(dispName) + "\",\"" + SecurityElement.Escape(file) + "\" ,0)";
                            sqlite_cmd.ExecuteNonQuery();
                        }
                        catch { }
                    }
                }
            }
        }

        private void __SearchRec(List<string> list, string path, string pattern)
        {
            try
            {
                foreach (string xtmp in Directory.EnumerateFiles(path, pattern, SearchOption.TopDirectoryOnly))
                {
                    list.Add(xtmp);
                }
                foreach (string xtmp in Directory.EnumerateDirectories(path))
                {
                    __SearchRec(list, xtmp, pattern);
                }
            }
            catch (UnauthorizedAccessException) { }
        }

        public override HandlerItem[] GetResultsFor(string search)
        {
            List<HandlerItem> ret = new List<HandlerItem>();

            lock (SQLiteHandler.lockObject)
            {
                availableStuff.Clear();
                SQLiteCommand sqlite_cmd = SQLiteHandler.GetCommand();
                sqlite_cmd.CommandText = "SELECT name, path, count FROM programs WHERE name LIKE \"%" + SecurityElement.Escape(search) + "%\" ORDER BY count DESC, name ASC";
                SQLiteDataReader sqlite_reader = sqlite_cmd.ExecuteReader();
                int namec = sqlite_reader.GetOrdinal("name");
                int pathc = sqlite_reader.GetOrdinal("path");
                int countc = sqlite_reader.GetOrdinal("count");
                string tmp; string tmp2;
                sqlite_cmd = SQLiteHandler.GetCommand();
                while (sqlite_reader.Read())
                {
                    tmp = sqlite_reader.GetString(pathc);
                    tmp2 = sqlite_reader.GetString(namec);
                    if (!File.Exists(tmp))
                    {
                        sqlite_cmd.CommandText = "DELETE FROM programs WHERE name = \"" + SecurityElement.Escape(tmp2) + "\"";
                        sqlite_cmd.ExecuteNonQuery();
                        continue;
                    }
                    ret.Add(new HandlerItem(tmp2, this));
                    availableStuff.Add(tmp2, tmp);
                }
                sqlite_reader.Close();
            }

            return ret.ToArray();
        }

        public override void Start(string item)
        {
            lock (SQLiteHandler.lockObject)
            {
                SQLiteCommand sqlite_cmd = SQLiteHandler.GetCommand();
                sqlite_cmd.CommandText = "UPDATE programs SET count = count + 1 WHERE name = \"" + SecurityElement.Escape(item) + "\"";
                sqlite_cmd.ExecuteNonQuery();

                Process p = new Process();
                p.StartInfo = new ProcessStartInfo(availableStuff[item], "");
                p.Start();
            }
        }
    }
}
