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
            return text.Equals(obj);
        }

        public override int GetHashCode()
        {
            return text.GetHashCode();
        }
    }
}
