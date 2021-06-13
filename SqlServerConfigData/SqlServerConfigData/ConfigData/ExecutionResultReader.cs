using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace SqlServerConfigData.ConfigData
{
    public class ExecutionResultReader
    {
        public DbParameter[] Parameters { get; protected set; }
        public DbDataReader Reader { get; protected set; }

        internal ExecutionResultReader(DbDataReader reader)
            : this(reader, null)
        {
        }

        internal ExecutionResultReader(DbDataReader reader, params DbParameter[] parameters)
        {
            Reader = reader;
            Parameters = parameters;
        }
    }
}
