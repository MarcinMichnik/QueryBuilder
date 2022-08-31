using System.Text;
using Newtonsoft.Json.Linq;
using QueryBuilder.DataTypes;

namespace QueryBuilder.Statements
{
    public class Update : FilterableStatement, IStatement
    {
        protected Dictionary<string, JToken> Columns { get; set; } = new();

        public Update(string tableName)
        {
            TableName = tableName;
        }

        public void AddColumn(string name, JToken value)
        {
            Columns.Add(name, value);
        }

        public void AddColumn(string name, SqlFunction function)
        {
            // Save function as JTokenType.String with a prefix
            string functionLiteral = function.GetPrefixedLiteral();
            Columns.Add(name, functionLiteral);
        }

        public string ToString(TimeZoneInfo timeZone)
        {
            if (WhereClauses.Count == 0)
            {
                throw new Exception(
                    "WhereClauses must not be empty or else the update will affect the entire table!");
            }

            string primaryKeyLookups = SerializeWhereClauses(timeZone);
            string columns = SerializeColumns(timeZone);

            return @$"UPDATE {TableName} SET
                          {columns} 
                      {primaryKeyLookups};";
        }

        private string SerializeColumns(TimeZoneInfo timeZone)
        {
            StringBuilder columns = new StringBuilder();
            foreach (KeyValuePair<string, JToken> column in Columns)
            {
                string convertedValue = ConvertJTokenToString(column.Value, timeZone);
                string columnLiteral = $"{column.Key} = {convertedValue},";
                columns.AppendLine(columnLiteral);
            }

            // remove last comma and newline
            int newLineLength = Environment.NewLine.Length;
            int valueToSubtract = newLineLength + 1;
            columns.Length -= valueToSubtract;

            return columns.ToString();
        }
    }
}