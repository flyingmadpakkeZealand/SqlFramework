using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace SqlFramework.Line
{
    public class SqlLine
    {
        private string _sqlString;
        private bool _initialized;

        public string SqlString => _initialized ? _sqlString : Setup.BeforeInitMessage;
        public bool Initialized => _initialized;
        internal List<Parse> Functions { get; }

        #region Creation
        public static SqlLineBuilder Build()
        {
            return new SqlLineBuilder();
        }

        public static SqlLineBuilder Build(string str)
        {
            return new SqlLineBuilder(str);
        }

        public static SqlLineBuilder Build(SqlLine sqlLine)
        {
            return new SqlLineBuilder(sqlLine);
        }

        public static implicit operator SqlLine(SqlLineBuilder sqlLineBuilder) => sqlLineBuilder.ToSqlLine();

        internal SqlLine()
        {
            Functions = new List<Parse>();
            _sqlString = string.Empty;
        }

        internal SqlLine(SqlLine sqlLine)
        {
            Functions = new List<Parse>(sqlLine.Functions);
            _sqlString = sqlLine._sqlString;
            _nextInitOffset = sqlLine._nextInitOffset;
        }

        public SqlLineBuilder CopyAndExtend()
        {
            return new SqlLineBuilder(this);
        }
        #endregion

        #region Execution
        private int _nextInitOffset;

        public SqlCommand InitializeCmd(object[] data, SqlConnection conn)
        {
            SqlCommand cmd = new SqlCommand(); //Save cmd object in a field?

            int count = 0;
            if (_initialized)
            {
                for(int i = 0; i < Functions.Count; i++)
                {
                    Parse parse = Functions[i];
                    SqlContainer container =
                        (SqlContainer)parse.Function.DynamicInvoke(
                            data[new Range(count, count + parse.ParamCount)]);
                    cmd.Parameters.AddRange(container.Parameters(i));
                    count += parse.ParamCount;
                }
            }
            else
            {
                for (int i = 0; i < Functions.Count; i++)
                {
                    Parse parse = Functions[i];
                    Delegate function = parse.Function;
                    SqlContainer container =
                        (SqlContainer)function.DynamicInvoke(
                            data[new Range(count, count + parse.ParamCount)]);
                    count += parse.ParamCount;

                    if (container == null)
                    {
                        _sqlString += parse.Initializer(null);
                        Functions.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        SqlParameter[] sqlParameters = container.Parameters(i);
                        if (i >= _nextInitOffset)
                        {
                            _sqlString += parse.Initializer(sqlParameters);
                            parse.CleanRefs();
                        }

                        cmd.Parameters.AddRange(sqlParameters);
                    }
                }

                _nextInitOffset = Functions.Count;
                _initialized = true;
            }

            cmd.Connection = conn;
            cmd.CommandText = _sqlString;

            return cmd;
        }

        public List<TIn> ExecuteQuery<TIn>(Func<SqlDataReader, TIn> readFunc, params object[] paramData)
        {
            return ExecuteQuery<TIn, List<TIn>>(readFunc, paramData);
        }

        public TOut ExecuteQuery<TIn, TOut>(Func<SqlDataReader, TIn> readFunc, params object[] paramData) where TOut : ICollection<TIn>, new()
        {
            TOut items = new TOut();

            using (SqlConnection conn = new SqlConnection(Setup.ConnectionString))
            using (SqlCommand cmd = InitializeCmd(paramData, conn))
            {
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    items.Add(readFunc(reader));
                }
            }

            return items;
        }

        public T ExecuteQuerySingleRead<T>(Func<SqlDataReader, T> readFunc, params object[] paramData)
        {
            using (SqlConnection conn = new SqlConnection(Setup.ConnectionString))
            using (SqlCommand cmd = InitializeCmd(paramData, conn))
            {
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return readFunc(reader);
                }
            }

            return default;
        }

        public void ExecuteNonQuery(params object[] paramData)
        {
            using (SqlConnection conn = new SqlConnection(Setup.ConnectionString))
            using (SqlCommand cmd = InitializeCmd(paramData, conn))
            {
                conn.Open();

                cmd.ExecuteNonQuery();
            }
        }
        #endregion
    }

    #region Old Code
    //public class SqlLine<T>
    //{
    //    private T _sample;
    //    private char _paramChar;
    //    private string _sqlStr;
    //    private Dictionary<int, Func<T, SqlParameter[]>> _sqlFuncs;

    //    internal Dictionary<int, Func<T, SqlParameter[]>> SqlFuncs
    //    {
    //        get { return _sqlFuncs; }
    //    }

    //    public string SqlString
    //    {
    //        get { return _sqlStr; }
    //    }

    //    internal SqlLine(T sample, char paramChar)
    //    {
    //        _sqlFuncs = new Dictionary<int, Func<T, SqlParameter[]>>();
    //        _sample = sample;
    //        _paramChar = paramChar;
    //    }

    //    private SqlLine(SqlLine<T> sqlLine) //Immutable implementation.
    //    {
    //        _sqlFuncs = new Dictionary<int, Func<T, SqlParameter[]>>(sqlLine._sqlFuncs);
    //        _sample = sqlLine._sample;
    //        _paramChar = sqlLine._paramChar;
    //        _sqlStr = sqlLine._sqlStr;
    //    }

    //    private void AddFunction(Func<T, SqlParameter> func)
    //    {
    //        int hash = func.GetHashCode();
    //        Add();

    //        void Add()
    //        {
    //            if (!_sqlFuncs.ContainsKey(hash))
    //            {
    //                _sqlFuncs.Add(hash, arg => new []{func(arg)});
    //            }
    //            else if (!_sqlFuncs[hash].Equals(func))
    //            {
    //                hash++;
    //                Add();
    //            }
    //        }
    //    }

    //    private void AddFunction(Func<T, SqlParameter[]> func)
    //    {
    //        int hash = func.GetHashCode();
    //        Add();

    //        void Add()
    //        {
    //            if (!_sqlFuncs.ContainsKey(hash))
    //            {
    //                _sqlFuncs.Add(hash, func);
    //            }
    //            else if (!_sqlFuncs[hash].Equals(func))
    //            {
    //                hash++;
    //                Add();
    //            }
    //        }
    //    }

    //    #region Str
    //    private SqlLine<T> InternStr(string str)
    //    {
    //        _sqlStr += " " + str;
    //        _sqlStr = _sqlStr.Trim();
    //        return this;
    //    }

    //    public SqlLine<T> Str(string str)
    //    {
    //        return new SqlLine<T>(this).InternStr(str);
    //    }
    //    #endregion

    //    #region Where
    //    private SqlLine<T> InternWhere(params SqlParameter[] parameters)
    //    {
    //        _sqlStr += " Where ";
    //        foreach (SqlParameter sqlParameter in parameters)
    //        {
    //            _sqlStr += $"{sqlParameter.ParameterName.TrimStart(_paramChar)} = {sqlParameter.ParameterName} And ";
    //        }

    //        _sqlStr = _sqlStr.Remove(_sqlStr.Length - 4);
    //        return this;
    //    }

    //    private SqlLine<T> InternWhere(Func<T, SqlParameter[]> paramFunc)
    //    {
    //        AddFunction(paramFunc);

    //        return InternWhere(paramFunc(_sample));
    //    }

    //    private SqlLine<T> InternWhere(Func<T, SqlParameter> paramFunc)
    //    {
    //        AddFunction(paramFunc);

    //        return InternWhere(paramFunc(_sample));
    //    }

    //    public SqlLine<T> Where(Func<T, SqlParameter[]> paramFunc)
    //    {
    //        return new SqlLine<T>(this).InternWhere(paramFunc);
    //    }

    //    public SqlLine<T> Where(Func<T, SqlParameter> paramFunc)
    //    {
    //        return new SqlLine<T>(this).InternWhere(paramFunc);
    //    }
    //    #endregion

    //    #region Values
    //    private SqlLine<T> InternValues(params SqlParameter[] parameters)
    //    {
    //        string output = "";
    //        string values = "";

    //        foreach (SqlParameter sqlParameter in parameters)
    //        {
    //            output += sqlParameter.ParameterName.TrimStart(_paramChar) + ",";
    //            values += sqlParameter.ParameterName + ",";
    //        }

    //        output = output.TrimEnd(',');
    //        values = values.TrimEnd(',');

    //        _sqlStr += $"({output}) Values ({values})";
    //        return this;
    //    }

    //    private SqlLine<T> InternValues(Func<T, SqlParameter[]> paramFunc)
    //    {
    //        AddFunction(paramFunc);

    //        return InternValues(paramFunc(_sample));
    //    }

    //    private SqlLine<T> InternValues(Func<T, SqlParameter> paramFunc)
    //    {
    //        AddFunction(paramFunc);

    //        return InternValues(paramFunc(_sample));
    //    }

    //    public SqlLine<T> Values(Func<T, SqlParameter[]> paramFunc)
    //    {
    //        return new SqlLine<T>(this).InternValues(paramFunc);
    //    }

    //    public SqlLine<T> Values(Func<T, SqlParameter> paramFunc)
    //    {
    //        return new SqlLine<T>(this).InternValues(paramFunc);
    //    }
    //    #endregion

    //    #region Set
    //    private SqlLine<T> InternSet(params SqlParameter[] parameters)
    //    {
    //        string output = " Set ";
    //        foreach (SqlParameter sqlParameter in parameters)
    //        {
    //            output += $"{sqlParameter.ParameterName.TrimStart(_paramChar)} = {sqlParameter.ParameterName},";
    //        }

    //        _sqlStr += output.TrimEnd(',');
    //        return this;
    //    }

    //    private SqlLine<T> InternSet(Func<T, SqlParameter[]> paramFunc)
    //    {
    //        AddFunction(paramFunc);

    //        return InternSet(paramFunc(_sample));
    //    }

    //    private SqlLine<T> InternSet(Func<T, SqlParameter> paramFunc)
    //    {
    //        AddFunction(paramFunc);

    //        return InternSet(paramFunc(_sample));
    //    }

    //    public SqlLine<T> Set(Func<T, SqlParameter[]> paramFunc)
    //    {
    //        return new SqlLine<T>(this).InternSet(paramFunc);
    //    }

    //    public SqlLine<T> Set(Func<T, SqlParameter> paramFunc)
    //    {
    //        return new SqlLine<T>(this).InternSet(paramFunc);
    //    }
    //    #endregion

    //    #region InnerJoin
    //    private SqlLine<T> InternInnerJoin(string on, string equals)
    //    {
    //        string t1 = on.Split('.')[0];
    //        string t2 = equals.Split('.')[0];
    //        int t1int = _sqlStr.IndexOf(t1);
    //        int t2int = _sqlStr.IndexOf(t2);
    //        string join = "Inner Join ";
    //        string joinedTable;

    //        if (t1int == -1 || t1int > t2int && !_sqlStr.Contains(join + t1))
    //        {
    //            joinedTable = t1;
    //        }
    //        else if (t2int == -1 || t2int > t1int && !_sqlStr.Contains(join + t2))
    //        {
    //            joinedTable = t2;
    //        }
    //        else
    //        {
    //            throw new ArgumentException("Tables already joined");
    //        }

    //        _sqlStr += $" {join}{joinedTable} on {on} = {equals}";

    //        return this;
    //    }

    //    public SqlLine<T> InnerJoin(string on, string equals)
    //    {
    //        return new SqlLine<T>(this).InternInnerJoin(on, equals);
    //    }
    //    #endregion
    //}
    #endregion
}
