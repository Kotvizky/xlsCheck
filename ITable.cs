using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Check
{
    interface ITable
    {
        DataTable table { get; }
        void Open( object[] join);
        string[] Select(object[] filter, object[] fields = null);
        string[] Select(string filter, object[] fields = null);

    }
}
