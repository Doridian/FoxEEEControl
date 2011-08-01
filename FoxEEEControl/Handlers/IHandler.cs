using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;

namespace FoxEEEControl.Handlers
{
    interface IHandler
    {
        void Initialize(object param); //true = force, false = noforce < atm
        HandlerItem[] GetResultsFor(string search);
        void Start(string item);
    }
}
