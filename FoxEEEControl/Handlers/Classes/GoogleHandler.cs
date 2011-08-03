using System.Collections.Generic;
using FoxEEEControl.Handlers.Types;

namespace FoxEEEControl.Handlers.Classes
{
    class GoogleHandler : PrefixHandler
    {
        public override string PREFIX { get { return "g"; } }

        protected override HandlerItem[] PrefixGetResultsFor(string search)
        {
            List<HandlerItem> ret = new List<HandlerItem>();
            ret.Add(new HandlerItem("http://www.google.de/search?q=" + search, this));
            return ret.ToArray();
        }
    }
}
