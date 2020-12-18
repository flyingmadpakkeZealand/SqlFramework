using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using SqlFramework.Line;
using static SqlFramework.SqlFunctions;

namespace SqlFramework.Expression
{
    public class SqlExpression
    {
        public string SqlString => _cmd.CommandText;
        private SqlCommand _cmd;
        private int _iteration;

        public SqlExpression()
        {
            _cmd = new SqlCommand();
            _iteration = 0;
        }

        public SqlExpression(string str)
        {
            _cmd = new SqlCommand(str + " ");
            _iteration = 0;
        }

        private void AddContainer(SqlContainer sqlContainer, Func<SqlParameter[], string> initializer)
        {
            SqlParameter[] sqlParameters = sqlContainer.Parameters(_iteration++);
            _cmd.CommandText += initializer(sqlParameters);
            _cmd.Parameters.AddRange(sqlParameters);
        }

        #region Execution
        public List<TIn> ExecuteQuery<TIn>(Func<SqlDataReader, TIn> readFunc)
        {
            return ExecuteQuery<TIn, List<TIn>>(readFunc);
        }

        public TOut ExecuteQuery<TIn, TOut>(Func<SqlDataReader, TIn> readFunc) where TOut : ICollection<TIn>, new()
        {
            TOut items = new TOut();

            using (SqlConnection conn = new SqlConnection(Setup.ConnectionString))
            {
                _cmd.Connection = conn;
                conn.Open();

                SqlDataReader reader = _cmd.ExecuteReader();

                while (reader.Read())
                {
                    items.Add(readFunc(reader));
                }
            }

            return items;
        }

        public T ExecuteQuerySingleRead<T>(Func<SqlDataReader, T> readFunc)
        {
            using (SqlConnection conn = new SqlConnection(Setup.ConnectionString))
            {
                _cmd.Connection = conn;
                conn.Open();

                SqlDataReader reader = _cmd.ExecuteReader();

                if (reader.Read())
                {
                    return readFunc(reader);
                }
            }

            return default;
        }

        public void ExecuteNonQuery()
        {
            using (SqlConnection conn = new SqlConnection(Setup.ConnectionString))
            {
                _cmd.Connection = conn;
                conn.Open();

                _cmd.ExecuteNonQuery();
            }
        }
        #endregion

        public SqlExpression Str(string str)
        {
            _cmd.CommandText += str + " ";
            return this;
        }

        public SqlExpression Select(string tablename)
        {
            _cmd.CommandText += $"Select * from {tablename} ";
            return this;
        }

        public SqlExpression Where(SqlContainer sqlContainer)
        {
            AddContainer(sqlContainer, InternWhere);
            return this;
        }

        public SqlExpression Values(SqlContainer sqlContainer)
        {
            AddContainer(sqlContainer, InternValues);
            return this;
        }

        public SqlExpression Set(SqlContainer sqlContainer)
        {
            AddContainer(sqlContainer, InternSet);
            return this;
        }
    }
}
