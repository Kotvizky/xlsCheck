using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Check
{
    interface IJsonParser
    {
        dynamic getParam(string paramName, out Exception exc);
    }
}
