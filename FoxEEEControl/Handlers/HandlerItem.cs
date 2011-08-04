using FoxEEEControl.Handlers.Types;

namespace FoxEEEControl.Handlers
{
    class HandlerItem
    {
        public HandlerItem(string txt, GenericHandler handl)
        {
            text = txt;
            handler = handl;
        }

        public string text;
        public GenericHandler handler;

        public override string ToString()
        {
            return text;
        }

        public override bool Equals(object obj)
        {
            if(!(obj is HandlerItem)) return false;
            return text.Equals(((HandlerItem)obj).text);
        }

        public override int GetHashCode()
        {
            return text.GetHashCode();
        }
    }
}
