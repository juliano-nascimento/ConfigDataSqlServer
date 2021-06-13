using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace SqlServerConfigData.ConfigData
{
    public class ExecutionParameter
    {
        private DbCommand _command;

        internal DbParameter[] Parameters;

        internal ExecutionParameter(DbCommand command)
        {
            _command = command;
            Parameters = new DbParameter[0];
        }

        public DbParameter Add(string name, ParameterDirection direction, DbType type, object value)
        {
            return Add(name, direction, type, 0, value);
        }

        public DbParameter Add(string name, ParameterDirection direction, DbType type, int size, object value)
        {
            DbParameter param = _command.CreateParameter();

            param.Value = value;
            param.DbType = type;
            param.Direction = direction;
            param.ParameterName = name;
            param.Size = size;

            Array.Resize(ref Parameters, Parameters.Length + 1);

            Parameters[^1] = param;

            return param;
        }
    }
}
