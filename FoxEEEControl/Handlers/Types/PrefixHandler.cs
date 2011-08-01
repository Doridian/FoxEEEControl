using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoxEEEControl.Handlers.Types
{
    abstract class PrefixHandler : IHandler
    {
        public abstract void Initialize(object param);
        public abstract string PREFIX { get; }
        protected abstract HandlerItem[] PrefixGetResultsFor(string search);
        protected abstract void PrefixStart(string item);

        public HandlerItem[] GetResultsFor(string search)
        {
            search = RemovePrefix(search);
            if (search == null) return null;
            return PrefixGetResultsFor(search);
        }

        public void Start(string item)
        {
            item = RemovePrefix(item);
            if (item == null) return;
            PrefixStart(item);
        }

        private string RemovePrefix(string str)
        {
            int len = PREFIX.Length + 1;
            if (len >= str.Length) return null;
            if (!str.StartsWith(PREFIX + " ")) return null;
            return str.Substring(len).Trim();
        }
    }
}
