using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using SqlFramework.Line;

namespace SqlFramework
{
    internal static class SqlFunctions
    {
        #region Values
        internal static string InternValues(SqlParameter[] sqlParameters)
        {
            string output = "";
            string values = "";

            foreach (SqlParameter sqlParameter in sqlParameters)
            {
                output += sqlParameter.ParameterName.TrimParam() + ",";
                values += sqlParameter.ParameterName + ",";
            }

            output = output.TrimEnd(',');
            values = values.TrimEnd(',');

            return $"({output}) Values ({values})";
        }
        #endregion


        #region Where
        internal static string InternWhere(SqlParameter[] sqlParameters)
        {
            string sqlString = "Where ";

            foreach (SqlParameter parameter in sqlParameters)
            {
                sqlString += $"{parameter.ParameterName.TrimParam()} = {parameter.ParameterName} And ";
            }

            return sqlString.Remove(sqlString.Length - 4);
        }
        #endregion


        #region Set
        internal static string InternSet(params SqlParameter[] parameters)
        {
            string output = " Set ";
            foreach (SqlParameter sqlParameter in parameters)
            {
                output += $"{sqlParameter.ParameterName.TrimParam()} = {sqlParameter.ParameterName},";
            }

            return output.TrimEnd(',');
        }
        #endregion
    }
}
