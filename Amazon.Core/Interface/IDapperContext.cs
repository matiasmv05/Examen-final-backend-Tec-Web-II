using Amazon.Core.Enum;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Core.Interface
{
    public interface IDapperContext
    {
        DatabaseProvider Provider { get; }

        Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null,
            CommandType commandType = CommandType.Text);

        Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null,
            CommandType commandType = CommandType.Text);

        Task<int> ExecuteAsync(string sql, object? param = null,
            CommandType commandType = CommandType.Text);

        Task<T> ExecuteScalarAsync<T>(string sql, object? param = null,
            CommandType commandType = CommandType.Text);

        void SetAmbientConnection(IDbConnection conn, IDbTransaction? tx);

        void ClearAmbientConnection();
    }
}
