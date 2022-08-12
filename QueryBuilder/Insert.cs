using System.Text;
using Newtonsoft.Json.Linq;

namespace QueryBuilder
{
    public class Insert : Statement
    {
        // key is columnName, value is sequencer method literal
        public KeyValuePair<string, string>? MasterPrimary { get; set; }

        public Insert(string tableName) : base(tableName)
        {
            TableName = tableName;
        }

        override public string ToString()
        {
            string columns = GetColumns();
            string values = GetValues();

            return @$"INSERT INTO {TableName} (
                          {columns}
                      ) VALUES (
                          {values}
                      );";
        }

        private string GetColumns()
        {
            StringBuilder columns = new StringBuilder();

            if (MasterPrimary != null)
                columns.AppendLine($"{MasterPrimary?.Key},");

            
            foreach (KeyValuePair<string, JToken> column in Columns)
            {
                string columnLiteral = $"{column.Key},";
                columns.AppendLine(columnLiteral);
            }

            // remove last comma and newline
            int newLineLength = Environment.NewLine.Length;
            columns.Length = columns.Length - newLineLength - 1;

            return columns.ToString();
        }

        private string GetValues()
        {
            StringBuilder columns = new StringBuilder();

            if (MasterPrimary != null)
                columns.AppendLine($"{MasterPrimary?.Value},");

            foreach (KeyValuePair<string, JToken> column in Columns)
            {
                string convertedValue = ConvertJTokenToString(column.Value);
                string columnLiteral = $"{convertedValue},";
                columns.AppendLine(columnLiteral);
            }

            // remove last comma and newline
            int newLineLength = Environment.NewLine.Length;
            columns.Length = columns.Length - newLineLength - 1;

            return columns.ToString();
        }
    }
}