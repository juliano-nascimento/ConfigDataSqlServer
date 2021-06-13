using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace SqlServerConfigData.ConfigData
{
    public class ContextTransaction : IDisposable
    {
        private IsolationLevel _isolationLevel;

        public bool Initialized { get; private set; }
        public DbTransaction Transaction { get; private set; }

        public ContextTransaction(IsolationLevel isolationLevel)
        {
            _isolationLevel = isolationLevel;
        }

        public void Begin(DbConnection connection)
        {
            if (connection == null)
                throw new ApplicationException("Uma conexão com a base de dados deve ser criada antes de iniciar uma transação");

            Transaction = connection.BeginTransaction(_isolationLevel);
            Initialized = true;
        }

        public void Commit()
        {
            if (Transaction == null)
                throw new ApplicationException("Nenhuma transação inicializada. As alterações não foram efetivadas.");

            Transaction.Commit();
            Initialized = false;
        }

        public void Rollback()
        {
            if (Transaction == null)
                throw new ApplicationException("Nenhuma transação inicializada. As alterações não foram desfeitas.");

            Transaction.Rollback();
            Initialized = false;
        }

        bool disposed;

        public void Dispose()
        {
            if (!disposed)
            {
                if (Transaction != null)
                    using (Transaction) { }

                disposed = true;
            }
        }
    }
}
