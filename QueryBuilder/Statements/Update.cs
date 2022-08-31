using System.Text;
using Newtonsoft.Json.Linq;

namespace QueryBuilder.Statements
{
    public class Update : Statement, IStatement
    {
        public Update(string tableName)
        {
            TableName = tableName;
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
                      WHERE {primaryKeyLookups};";
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