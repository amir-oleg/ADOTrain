using System;
using System.Data;
using System.Data.Common;

namespace ADOTrain.Repositories
{
    public class BaseRepository
    {
        protected virtual T GetConnection<T>(Func<IDbConnection, T> getData, string providerInvariantName = default, string connectionString = default)
        {
            try
            {
                var factory = DbProviderFactories.GetFactory(providerInvariantName);
                using (var connection = factory.CreateConnection())
                {
                    if (connection != null)
                    {
                        connection.ConnectionString = connectionString;
                        connection.Open();
                        return getData(connection);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return default;
        }

        protected virtual T CreateCommand<T>(IDbConnection connection, string commandText, int commandTimeout = -1,
            CommandType commandType = CommandType.Text, IDbTransaction transaction = null) where T : IDbCommand, new()
        {
            var command = new T { Connection = connection, CommandText = commandText, CommandType = commandType };

            if (commandTimeout > 0)
            {
                command.CommandTimeout = commandTimeout;
            }

            if (transaction != null)
            {
                command.Transaction = transaction;
            }

            return command;
        }
    }
}
