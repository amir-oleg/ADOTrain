using System;
using System.Data;
using System.Data.SqlClient;

namespace ADOTrain
{
    public class Repository
    {
        private static readonly string ConnectionString = Properties.Resource.ConnectionString;
        public T GetConnection<T>(Func<SqlConnection,T> getData)
        {
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    return getData(connection);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return default(T);
        }
    }
}
