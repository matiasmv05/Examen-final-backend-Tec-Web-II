using Amazon.Core.Enum;
using Amazon.Core.Interface;
using MySqlConnector;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Infrastructure.Repositories
{
        public class DbConnectionFactory : IDbConnectionFactory
        {
            private readonly string _sqlConn;

            public DbConnectionFactory(IConfiguration config)
            {
                _sqlConn = config.GetConnectionString("ConnectionMySQL")
                    ?? throw new InvalidOperationException("Connection string 'ConnectionMySQL' not found.");

                Provider = DatabaseProvider.MySql;
            }

            public DatabaseProvider Provider { get; }

            public IDbConnection CreateConnection()
            {
                var connection = new MySqlConnection(_sqlConn);
                return connection;
            }
        }
    

}
