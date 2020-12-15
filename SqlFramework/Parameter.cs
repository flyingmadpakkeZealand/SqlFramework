using System.Data.SqlClient;

namespace SqlFramework
{
    public class Parameter
    {
        public string ColumnName { get; set; }
        public object Data { get; set; }

        public Parameter(string columnName, object data)
        {
            ColumnName = Setup.Pchar + columnName;
            Data = data;
        }
    }
}
