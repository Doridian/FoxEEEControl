using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoxEEEControl.Handlers
{
    class NCalcHandler : IHandler
    {
        public void Initialize(bool forceFully) { }

        public HandlerItem[] GetResultsFor(string search)
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

        public void Start(HandlerItem item) { }
    }
}
