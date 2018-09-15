using Dapper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;

namespace AuraWeb.Services
{
    /// <summary>
    /// This service will interact with a single SQLite DB, the filename of which will be
    /// sent to the constructor
    /// </summary>
    public class SQLiteService
    {
        private string _FILENAME { get; set; }

        public SQLiteService(string filename)
        {
            _FILENAME = filename;
        }

        /// <summary>
        /// Get an opened connection from the SQLite DB.
        /// </summary>
        /// <param name="failIfMissing">True causes a failure if the DB filename does not exist. Default is false.</param>
        /// <param name="version">SQLite DB version number in string format. Default is "3"</param>
        /// <returns></returns>
        public DbConnection GetConnection(bool failIfMissing = false, string version = "3")
        {
            string failIfMissingString = (failIfMissing == true ? "True" : "False");
            string connectionString = new DbConnectionStringBuilder()
            {
                { "Data Source", _FILENAME },
                { "Version", version },
                { "FailIfMissing", failIfMissingString },
            }.ConnectionString;
            SQLiteConnection conn = new SQLiteConnection(connectionString);
            conn.Open();
            return conn;
        }

        public void Execute(string sql)
        {
            using (var conn = GetConnection())
            {
                conn.Execute(sql);
            }
        }

        public void ExecuteMultiple(List<string> sqlStatements)
        {
            using (var conn = GetConnection())
            {
                for (int x = 0; x < sqlStatements.Count; x++)
                {
                    string sql = sqlStatements[x];
                    conn.Execute(sql);
                }
            }
        }

        public T SelectSingle<T>(string sql, object parameters = null, bool _verbose = true)
        {
            if (String.IsNullOrEmpty(sql)) return default(T);

            using (var cnn = GetConnection())
            {
                IEnumerable<T> result = Enumerable.Empty<T>();
                dynamic dbResult;
                if (parameters != null) dbResult = cnn.Query<dynamic>(sql, parameters);
                else dbResult = cnn.Query<dynamic>(sql);
                result = Slapper.AutoMapper.MapDynamic<T>(dbResult, false) as IEnumerable<T>;
                return result.FirstOrDefault();
            }
        }

        public List<T> SelectMultiple<T>(string sql, object parameters = null, bool _verbose = true)
        {
            if (String.IsNullOrEmpty(sql)) return null;

            using (var cnn = GetConnection())
            {
                IEnumerable<T> result = Enumerable.Empty<T>();
                dynamic dbResult;
                if (parameters != null) dbResult = cnn.Query<dynamic>(sql, parameters);
                else dbResult = cnn.Query<dynamic>(sql);
                result = Slapper.AutoMapper.MapDynamic<T>(dbResult, false) as IEnumerable<T>;
                return result.ToList();
            }
        }
    }
}
