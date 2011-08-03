
namespace FoxEEEControl.Handlers.Types
{
    abstract class GenericHandler
    {
        public virtual void Initialize(object param) { } //true = force, false = noforce < atm
        public abstract HandlerItem[] GetResultsFor(string search);
        public virtual void Start(string item)
        {
            throw new HandlerUseShellExecException();
        }
        public virtual string GetTextForItem(string item)
        {
            return item;
        }
    }
}
