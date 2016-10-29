using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDM.xData.xClient
{
    public static class DClientExtensions
    {
        public static int AddWithValue(this IDataParameterCollection Parameters, string name, object value)
        {
            IDbDataParameter Parameter = Factory.CreateDataParameter(Parameters);
            Parameter.ParameterName = name;
            Parameter.Value = value;
            return Parameters.Add(Parameter);
        }

        public static void Dispose(this IDataAdapter da)
        {
            if (da is DbDataAdapter)
            {
                ((DbDataAdapter)da).Dispose();
            }
        }
    }
}
