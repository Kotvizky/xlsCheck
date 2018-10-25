using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Check
{
    class JsonParser : IJsonParser
    {

        public JsonParser(object[] initObject)
        {
            this.initObject = initObject;
        }

        object[] initObject;

        public dynamic getParam(string paramName, out Exception exc)
        {
            exc = null;
            dynamic paramValue = null;
            foreach (dynamic jsonObj in initObject)
            {
                try
                {
                    paramValue = ((Dictionary<string, object>)jsonObj)[paramName];
                    break;
                }
                catch (Exception e)
                {
                    exc = e;
                }
            }
            return paramValue;
        }
    }
}
