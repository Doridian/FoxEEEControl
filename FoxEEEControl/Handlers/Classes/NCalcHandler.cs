using System.Collections.Generic;
using FoxEEEControl.Handlers.Types;

namespace FoxEEEControl.Handlers.Classes
{
    class NCalcHandler : GenericHandler
    {
        public override HandlerItem[] GetResultsFor(string search)
        {
            List<HandlerItem> ret = new List<HandlerItem>();
            try
            {
                NCalc.Expression exp = new NCalc.Expression(search);
                ret.Add(new HandlerItem(exp.Evaluate() + " = " + exp.ParsedExpression.ToString(),this));
            }
            catch { }
            return ret.ToArray();
        }

        public override void Start(string item) { }
    }
}
