using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace SqlFramework
{
    public class SqlContainer : IEnumerable<Parameter>
    {
        private List<Parameter> _parameters;

        internal SqlParameter[] Parameters(int mod)
        {
            SqlParameter[] sqlParameters = new SqlParameter[_parameters.Count];

            for (int i = 0; i < sqlParameters.Length; i++)
            {
                Parameter parameter = _parameters[i];
                SqlParameter sqlParameter = new SqlParameter($"{Setup.Pchar}{mod}{Setup.Pchar}{parameter.ColumnName}", parameter.Data);
                sqlParameters[i] = sqlParameter;
            }

            return sqlParameters;
        }

        public SqlContainer(string columnName, object data)
        {
            _parameters = new List<Parameter> {new Parameter(columnName, data)};
        }

        public SqlContainer()
        {
            _parameters = new List<Parameter>();
        }

        public void Add(Parameter parameter)
        {
            _parameters.Add(parameter);
        }

        public IEnumerator<Parameter> GetEnumerator()
        {
            return _parameters.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
