
namespace FoxEEEControl.Handlers.Types
{
    abstract class PrefixHandler : GenericHandler
    {
        public abstract string PREFIX { get; }
        protected abstract HandlerItem[] PrefixGetResultsFor(string search);

        public override HandlerItem[] GetResultsFor(string search)
        {
            search = RemovePrefix(search);
            if (search == null) return null;
            return PrefixGetResultsFor(search);
        }

        public override void Start(string item)
        {
            string item2 = RemovePrefix(item);
            if (item2 == null) PrefixStart(item);
            else PrefixStart(item2);
        }

        private string RemovePrefix(string str)
        {
            int len = PREFIX.Length + 1;
            if (len >= str.Length) return null;
            if (!str.StartsWith(PREFIX + " ")) return null;
            return str.Substring(len).Trim();
        }

        public override string GetTextForItem(string item)
        {
            return PREFIX + " " + item;
        }

        protected virtual void PrefixStart(string item)
        {
            throw new HandlerUseShellExecException(item);
        }
    }
}
