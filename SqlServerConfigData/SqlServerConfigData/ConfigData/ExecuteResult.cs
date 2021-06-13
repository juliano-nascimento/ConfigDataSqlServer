using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace SqlServerConfigData.ConfigData
{
    public class ExecuteResult
    {
        public DbParameter[] Parameters { get; protected set; }
    }
}
