using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FoxEEEControl.Handlers
{
    class DirectoryBrowserHandler : IHandler
    {
        private readonly char[] pathSep = { '/', '\\' };

        public void Initialize(bool forceFully) { }

        public HandlerItem[] GetResultsFor(string search)
        {
            if (search.Length < 3 || search[1] != ':' || (search[2] != '\\' && search[2] != '/')) return null;
            List<HandlerItem> ret = new List<HandlerItem>();
            try
            {
                char c = search[search.Length - 1];
                string patt = "*";
                if (c != '/' && c != '\\')
                {
                    int x = search.LastIndexOfAny(pathSep) + 1;
                    patt = search.Substring(x) + "*";
                    search = search.Remove(x);
                }
                else
                {
                    ret.Add(new HandlerItem(search,this));
                }
                foreach (string s in Directory.GetFileSystemEntries(search, patt))
                {
                    ret.Add(new HandlerItem(s, this));
                }
            }
            catch { }
            return ret.ToArray();
        }

        public void Start(HandlerItem item)
        {
            throw new HandlerUseShellExecException();
        }
    }
}
