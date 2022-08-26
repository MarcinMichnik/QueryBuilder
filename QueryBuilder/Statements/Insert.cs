using System.Text;
using Newtonsoft.Json.Linq;

namespace QueryBuilder.Statements
{
    public class Insert : Statement, IStatement
    {
        public Insert(string tableName)
        {
            TableName = tableName;
        }

        public string ToString(TimeZoneInfo timeZone)
        {
            string columns = GetColumns();
            string values = GetValues(timeZone);

            return @$"INSERT INTO {TableName} (
                          {columns}
                      ) VALUES (
                          {values}
                      );";
        }

        private string GetColumns()
        {
            StringBuilder columnStringBuilder = new();

            foreach (KeyValuePair<string, JToken> column in Columns)
            {
                string columnLiteral = $"{column.Key},";
                columnStringBuilder.AppendLine(columnLiteral);
            }

            RemoveTrailingSigns(columnStringBuilder);

            return columnStringBuilder.ToString();
        }

        private static void RemoveTrailingSigns(StringBuilder columns)
        {
            int newLineStrLength = Environment.NewLine.Length;
            int newLength = columns.Length - newLineStrLength - 1;
            columns.Length = newLength;
        }

        private string GetValues(TimeZoneInfo timeZone)
        {
            StringBuilder columnStringBuilder = new();

            foreach (KeyValuePair<string, JToken> column in Columns)
            {
                string convertedValue = ConvertJTokenToString(column.Value, timeZone);
                string columnLiteral = $"{convertedValue},";
                columnStringBuilder.AppendLine(columnLiteral);
            }

            RemoveTrailingSigns(columnStringBuilder);

            return columnStringBuilder.ToString();
        }
    }
}