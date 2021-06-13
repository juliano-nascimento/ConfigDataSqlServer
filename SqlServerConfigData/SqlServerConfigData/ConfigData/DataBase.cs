using Microsoft.Extensions.Configuration;
using SqlServerConfigData.Criptografia;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerConfigData.ConfigData
{
    public class DataBase:IDisposable
    {
        private const Int32 CommandTimeout = 30;
        DbConnection _connection;
        ContextTransaction _contextTransaction;
        private static IConfiguration _configuration;

        public bool IsConnectionString { get; set; }
        string _connectionStringKey;

        public String DatabaseName
        {
            get
            {
                if (ConnectionState.Open != _connection.State)
                {
                    return String.Empty;
                }
                return _connection.Database;
            }
        }

        public DataBase(string name)
        {
            _connectionStringKey = name;
        }

        public DataBase(string name, IConfiguration configuration)
        {
            _connectionStringKey = name;
            _configuration = configuration;
        }

        public DataBase(string name, bool isconnectionString)
        {
            _connectionStringKey = name;
            this.IsConnectionString = isconnectionString;
        }

        public static string GetServidor(string name)
        {
            string cnnStr = _configuration.GetConnectionString("Connection");

            SqlConnectionStringBuilder builder
                = new SqlConnectionStringBuilder(cnnStr);

            return builder.DataSource.ToUpper();
        }

        protected virtual DbConnection Create()
        {
            string connectionString;

            if (!this.IsConnectionString)
                connectionString = Connection.GetConnection(_configuration); //_configuration.GetConnectionString("Connection");
            else
                connectionString = _connectionStringKey;

            return new SqlConnection(new Base64Crypt().Decrypt(connectionString));
        }

        protected virtual DbCommand Create(DbConnection connection)
        {
            return connection.CreateCommand();
        }

        protected virtual void PrepareCommand(DbCommand command, DbTransaction transaction, CommandType type, string text, int timeout, params DbParameter[] parameters)
        {
            command.CommandType = type;
            command.CommandText = text;

            command.CommandTimeout = timeout;

            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }
        }

        void PrepareCommand(DbCommand command, CommandType type, string text, int timeout, params DbParameter[] parameters)
        {
            DbTransaction trn = null;

            if (_contextTransaction != null)
            {
                if (!_contextTransaction.Initialized)
                {
                    _contextTransaction.Begin(_connection);
                }

                trn = _contextTransaction.Transaction;
            }

            command.Transaction = trn;

            PrepareCommand(command, trn, type, text, timeout, parameters);
        }

        void CreateAndOpenConnection()
        {
            _connection = Create();

            if (_connection == null)
            {
                throw new ApplicationException("Nenhuma conexão com a base de dados foi criada.");
            }

            if (ConnectionState.Closed == _connection.State)
            {
                _connection.Open();
            }
        }

        public void BeginTran(IsolationLevel level)
        {
            if (_contextTransaction != null)
            {
                throw new ApplicationException("Já existe uma transação inicializada.");
            }

            _contextTransaction = new ContextTransaction(level);
        }

        public void BeginTran()
        {
            BeginTran(IsolationLevel.Unspecified);
        }

        public void CommitTran()
        {
            using (_contextTransaction)
            {
                _contextTransaction.Commit();
            }

            _contextTransaction = null;
        }

        public void RollbackTran()
        {
            using (_contextTransaction)
            {
                _contextTransaction.Rollback();
            }

            _contextTransaction = null;
        }

        #region "[ExecuteNoQueryAsync]"
        public async Task<int> ExecuteNoQueryAsync(CommandType type, string text)
        {
            return await ExecuteNoQueryAsync(type, text, CommandTimeout, null);
        }

        public async Task<int> ExecuteNoQueryAsync(DbConnection cnn, CommandType type, string text)
        {
            _connection = cnn;
            return await ExecuteNoQueryAsync(type, text, CommandTimeout, null);
        }

        public async Task<int> ExecuteNoQueryAsync(CommandType type, string text, int timeout)
        {
            return await ExecuteNoQueryAsync(type, text, CommandTimeout, null);
        }

        public async Task<int> ExecuteNoQueryAsync(CommandType type, string text, Action<ExecutionParameter> parametersHandle)
        {
            return await ExecuteNoQueryAsync(type, text, CommandTimeout, parametersHandle);
        }

        public async Task<int> ExecuteNoQueryAsync(CommandType type, string text, int timeout, Action<ExecutionParameter> parametersHandle)
        {
            CreateAndOpenConnection();

            using (DbCommand cmd = Create(_connection))
            {
                ExecutionParameter ep
                    = new ExecutionParameter(cmd);

                if (parametersHandle != null)
                {
                    parametersHandle(ep);
                }

                PrepareCommand(cmd, type, text, timeout, ep.Parameters);

                return await cmd.ExecuteNonQueryAsync();
            }
        }
        #endregion

        #region "[ExecuteNoQuery]"
        public int ExecuteNoQuery(CommandType type, string text)
        {
            return ExecuteNoQuery(type, text, CommandTimeout, null);
        }

        public int ExecuteNoQuery(DbConnection cnn, CommandType type, string text)
        {
            _connection = cnn;
            return ExecuteNoQuery(type, text, CommandTimeout, null);
        }

        public int ExecuteNoQuery(CommandType type, string text, int timeout)
        {
            return ExecuteNoQuery(type, text, CommandTimeout, null);
        }

        public int ExecuteNoQuery(CommandType type, string text, Action<ExecutionParameter> parametersHandle)
        {
            return ExecuteNoQuery(type, text, CommandTimeout, parametersHandle);
        }

        public int ExecuteNoQuery(CommandType type, string text, int timeout, Action<ExecutionParameter> parametersHandle)
        {
            CreateAndOpenConnection();

            using (DbCommand cmd = Create(_connection))
            {
                ExecutionParameter ep
                    = new ExecutionParameter(cmd);

                if (parametersHandle != null)
                {
                    parametersHandle(ep);
                }

                PrepareCommand(cmd, type, text, timeout, ep.Parameters);

                return cmd.ExecuteNonQuery();
            }
        }
        #endregion

        #region "[ExecuteScalarAsync]"
        public async Task<object> ExecuteScalarAsync(CommandType type, string text)
        {
            return await ExecuteScalarAsync(type, text, CommandTimeout, null);
        }

        public async Task<object> ExecuteScalarAsync(CommandType type, string text, int timeout)
        {
            return await ExecuteScalarAsync(type, text, timeout, null);
        }

        public async Task<object> ExecuteScalarAsync(CommandType type, string text, Action<ExecutionParameter> parametersHandle)
        {
            return await ExecuteScalarAsync(type, text, CommandTimeout, parametersHandle);
        }

        public async Task<object> ExecuteScalarAsync(CommandType type, string text, int timeout, Action<ExecutionParameter> parametersHandle)
        {
            CreateAndOpenConnection();

            using (DbCommand cmd = Create(_connection))
            {
                ExecutionParameter ep
                    = new ExecutionParameter(cmd);

                if (parametersHandle != null)
                {
                    parametersHandle(ep);
                }

                PrepareCommand(cmd, type, text, timeout, ep.Parameters);

                return await cmd.ExecuteScalarAsync();
            }
        }
        #endregion

        #region "[ExecuteScalar"]
        public object ExecuteScalar(CommandType type, string text)
        {
            return ExecuteScalar(type, text, CommandTimeout, null);
        }

        public object ExecuteScalar(CommandType type, string text, int timeout)
        {
            return ExecuteScalar(type, text, timeout, null);
        }

        public object ExecuteScalar(CommandType type, string text, Action<ExecutionParameter> parametersHandle)
        {
            return ExecuteScalar(type, text, CommandTimeout, parametersHandle);
        }

        public object ExecuteScalar(CommandType type, string text, int timeout, Action<ExecutionParameter> parametersHandle)
        {
            CreateAndOpenConnection();

            using (DbCommand cmd = Create(_connection))
            {
                ExecutionParameter ep
                    = new ExecutionParameter(cmd);

                if (parametersHandle != null)
                {
                    parametersHandle(ep);
                }

                PrepareCommand(cmd, type, text, timeout, ep.Parameters);

                return cmd.ExecuteScalar();
            }
        }

        #endregion

        #region "[ExecuteReaderAsync]"
        public async Task<DbDataReader> ExecuteReaderAsync(CommandType type, string text)
        {
            return await ExecuteReaderAsync(type, text, null);
        }

        public async Task<DbDataReader> ExecuteReaderAsync(CommandType type, string text, Action<ExecutionParameter> parametersHandle)
        {
            return await ExecuteReaderAsync(type, text, CommandTimeout, parametersHandle);
        }

        public async Task<DbDataReader> ExecuteReaderAsync(CommandType type, string text, int timeout, Action<ExecutionParameter> parametersHandle)
        {
            try
            {
                CreateAndOpenConnection();

                using (DbCommand cmd = Create(_connection))
                {
                    ExecutionParameter ep
                    = new ExecutionParameter(cmd);

                    if (parametersHandle != null)
                    {
                        parametersHandle(ep);
                    }

                    PrepareCommand(cmd, type, text, timeout, ep.Parameters);

                    return await cmd.ExecuteReaderAsync();
                }
            }
            catch (Exception ex)
            {
                var t = ex.Message;
            }
            return null;
        }

        public void ExecuteReaderAsync(CommandType type, string text, Action<ExecutionParameter> parametersHandle, Action<ExecutionResultReader> resultHandler)
        {
            ExecuteReaderAsync(type, text, CommandTimeout, parametersHandle, resultHandler);
        }

        public async void ExecuteReaderAsync(CommandType type, string text, int timeout, Action<ExecutionParameter> parametersHandle, Action<ExecutionResultReader> resultHandler)
        {
            CreateAndOpenConnection();

            using (DbCommand cmd = Create(_connection))
            {
                ExecutionParameter ep
                    = new ExecutionParameter(cmd);

                if (parametersHandle != null)
                {
                    parametersHandle(ep);
                }

                PrepareCommand(cmd, type, text, timeout, ep.Parameters);

                using (DbDataReader dbReader = await cmd.ExecuteReaderAsync())
                {
                    if (resultHandler != null)
                    {
                        resultHandler(new ExecutionResultReader(dbReader, ep.Parameters));
                    }
                }
            }
        }
        #endregion

        #region "[ExecuteReader]"

        public DbDataReader ExecuteReader(CommandType type, string text)
        {
            return ExecuteReader(type, text, null);
        }

        public DbDataReader ExecuteReader(CommandType type, string text, Action<ExecutionParameter> parametersHandle)
        {
            return ExecuteReader(type, text, CommandTimeout, parametersHandle);
        }

        public DbDataReader ExecuteReader(CommandType type, string text, int timeout, Action<ExecutionParameter> parametersHandle)
        {
            try
            {
                CreateAndOpenConnection();

                using (DbCommand cmd = Create(_connection))
                {
                    ExecutionParameter ep
                    = new ExecutionParameter(cmd);

                    if (parametersHandle != null)
                    {
                        parametersHandle(ep);
                    }

                    PrepareCommand(cmd, type, text, timeout, ep.Parameters);

                    return cmd.ExecuteReader();
                }
            }
            catch (Exception ex)
            {
                var t = ex.Message;
            }
            return null;
        }

        public void ExecuteReader(CommandType type, string text, Action<ExecutionParameter> parametersHandle, Action<ExecutionResultReader> resultHandler)
        {
            ExecuteReader(type, text, CommandTimeout, parametersHandle, resultHandler);
        }

        public void ExecuteReader(CommandType type, string text, int timeout, Action<ExecutionParameter> parametersHandle, Action<ExecutionResultReader> resultHandler)
        {
            CreateAndOpenConnection();

            using (DbCommand cmd = Create(_connection))
            {
                ExecutionParameter ep
                    = new ExecutionParameter(cmd);

                if (parametersHandle != null)
                {
                    parametersHandle(ep);
                }

                PrepareCommand(cmd, type, text, timeout, ep.Parameters);

                using (DbDataReader dbReader = cmd.ExecuteReader())
                {
                    if (resultHandler != null)
                    {
                        resultHandler(new ExecutionResultReader(dbReader, ep.Parameters));
                    }
                }
            }
        }

        //public void Bulk(DataTable dt)
        //{
        //    var conn = Create();

        //    using (var iConn = new MySqlConnection(conn.ConnectionString))
        //    {
        //        iConn.Open();

        //        MySqlBulkLoader bulk = new MySqlBulkLoader(iConn as MySqlConnection);
        //        bulk. .BulkCopyTimeout = 300;
        //        bulk.DestinationTableName = dt.TableName;
        //        bulk.WriteToServer(dt);
        //    }
        //}
        #endregion

        bool disposed;

        public void Dispose()
        {
            if (!disposed)
            {
                if (_contextTransaction != null)
                {
                    using (_contextTransaction) { }
                }

                if (_connection != null)
                {
                    using (_connection) { }
                }
                disposed = true;
            }
        }

        public static DataBase GetDataBase(IConfiguration configuration)
        {
            return new DataBase("CurrentEnvironment", configuration);
        }

        public static DataBase GetDataBaseDes(IConfiguration configuration)
        {
            return new DataBase("CurrentEnvironmentDes", configuration);
        }

        public static DataBase GetDataBaseHom(IConfiguration configuration)
        {
            return new DataBase("CurrentEnvironmentHom", configuration);
        }
    }
}
