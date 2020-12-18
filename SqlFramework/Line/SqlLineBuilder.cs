using System.Collections.Generic;
using System.Data.SqlClient;
using static SqlFramework.SqlFunctions;

namespace SqlFramework.Line
{
    public class SqlLineBuilder
    {
        private SqlLine _sqlLine;
        internal List<Parse> Functions => _sqlLine.Functions;

        public SqlLineBuilder()
        {
            _sqlLine = new SqlLine();
        }

        public SqlLineBuilder(string str)
        {
            _sqlLine = new SqlLine();
            Str(str);
        }

        public SqlLineBuilder(SqlLine sqlLine)
        {
            _sqlLine = new SqlLine(sqlLine);
        }

        public SqlLine ToSqlLine()
        {
            return _sqlLine;
        }

        #region Str
        public SqlLineBuilder Str(string str)
        {
            return new Parse(this, parameters => str + " ").Empty();
        }
        #endregion

        #region Select
        public SqlLineBuilder Select(string tableName)
        {
            return new Parse(this, parameters => $"Select * From {tableName} ").Empty();
        }
        #endregion

        #region Values
        public Parse Values()
        {
            return new Parse(this, InternValues);
        }
        #endregion

        #region Where
        public Parse Where()
        {
            return new Parse(this, InternWhere);
        }
        #endregion

        #region Set
        public Parse Set()
        {
            return new Parse(this, InternSet);
        }
        #endregion
    }
}
