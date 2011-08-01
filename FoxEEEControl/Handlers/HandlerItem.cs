using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoxEEEControl.Handlers
{
    class HandlerItem
    {
        public HandlerItem(string txt, IHandler handl)
        {
            text = txt;
            handler = handl;
        }

        public string text;
        public IHandler handler;

        public override string ToString()
        {
            return text;
        }

        public override bool Equals(object obj)
        {
            return text.Equals(obj);
        }

        public override int GetHashCode()
        {
            return text.GetHashCode();
        }
    }
}
