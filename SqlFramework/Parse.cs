﻿using System;
using System.Data.SqlClient;

namespace SqlFramework
{
    public class Parse
    {
        internal Delegate Function { get; private set; }
        internal int ParamCount { get; private set; }
        internal Func<SqlParameter[], string> Initializer { get; private set; }
        
        private SqlLine _sqlLine;

        internal Parse(SqlLine sqlLine, Func<SqlParameter[], string> initializer)
        {
            _sqlLine = sqlLine;
            _sqlLine.Functions.Add(this);
            Initializer = initializer;
        }

        internal void CleanRefs()
        {
            _sqlLine = null;
            Initializer = null;
        }

        internal SqlLine Empty()
        {
            Func<SqlContainer> empty = () => null;
            Function = empty;
            ParamCount = 0;
            return _sqlLine;
        }

        #region ParamFunctions
        public SqlLine Param<T1>(Func<T1, SqlContainer> func)
        {
            ParamCount = 1;
            Function = func;
            return _sqlLine;
        }

        public SqlLine Param<T1, T2>(Func<T1, T2, SqlContainer> func)
        {
            ParamCount = 2;
            Function = func;
            return _sqlLine;
        }

        public SqlLine Param<T1, T2, T3>(Func<T1, T2, T3, SqlContainer> func)
        {
            ParamCount = 3;
            Function = func;
            return _sqlLine;
        }

        public SqlLine Param<T1, T2, T3, T4>(Func<T1, T2, T3, T4, SqlContainer> func)
        {
            ParamCount = 4;
            Function = func;
            return _sqlLine;
        }

        public SqlLine Param<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, SqlContainer> func)
        {
            ParamCount = 5;
            Function = func;
            return _sqlLine;
        }

        public SqlLine Param<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, SqlContainer> func)
        {
            ParamCount = 6;
            Function = func;
            return _sqlLine;
        }

        public SqlLine Param<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, SqlContainer> func)
        {
            ParamCount = 7;
            Function = func;
            return _sqlLine;
        }

        public SqlLine Param<T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, SqlContainer> func)
        {
            ParamCount = 8;
            Function = func;
            return _sqlLine;
        }
        #endregion
    }
}
