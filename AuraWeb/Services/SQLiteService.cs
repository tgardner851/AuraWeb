﻿using Dapper;
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
        /// GetById an opened connection from the SQLite DB.
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

        public void Execute(string sql, object parameters = null)
        {
            using (var conn = GetConnection())
            {
                if (parameters != null) conn.Execute(sql, parameters);
                else conn.Execute(sql);
            }
        }

        public void ExecuteMultiple(List<string> sqlStatements, List<object> parameters = null)
        {
            using (var conn = GetConnection())
            {
                using (var transaction = conn.BeginTransaction())
                {
                    for (int x = 0; x < sqlStatements.Count; x++)
                    {
                        object parameter = null;
                        string sql = sqlStatements[x];
                        if (parameters != null) parameter = parameters[x];
                        if (parameters != null) conn.Execute(sql, parameter);
                        else conn.Execute(sql);
                    }
                    transaction.Commit();
                }
            }
        }

        public T SelectSingle<T>(string sql, object parameters = null, bool useSlapper = true)
        {
            if (String.IsNullOrEmpty(sql)) return default(T);

            using (var cnn = GetConnection())
            {
                IEnumerable<T> result = Enumerable.Empty<T>();
                if(useSlapper)
                {
                    dynamic dbResult;
                    if (parameters != null) dbResult = cnn.Query<dynamic>(sql, parameters);
                    else dbResult = cnn.Query<dynamic>(sql);
                    result = Slapper.AutoMapper.MapDynamic<T>(dbResult, false) as IEnumerable<T>;
                }
                else
                {
                    if (parameters != null) result = cnn.Query<T>(sql, parameters);
                    else result = cnn.Query<T>(sql);
                }
                return result.FirstOrDefault();
            }
        }

        public List<T> SelectMultiple<T>(string sql, object parameters = null, bool useSlapper = true)
        {
            if (String.IsNullOrEmpty(sql)) return null;

            using (var cnn = GetConnection())
            {
                IEnumerable<T> result = Enumerable.Empty<T>();
                if(useSlapper)
                {
                    dynamic dbResult;
                    if (parameters != null) dbResult = cnn.Query<dynamic>(sql, parameters);
                    else dbResult = cnn.Query<dynamic>(sql);
                    result = Slapper.AutoMapper.MapDynamic<T>(dbResult, false) as IEnumerable<T>;
                }
                else
                {
                    if (parameters != null) result = cnn.Query<T>(sql, parameters);
                    else result = cnn.Query<T>(sql);
                }
                return result.ToList();
            }
        }
    }
}
