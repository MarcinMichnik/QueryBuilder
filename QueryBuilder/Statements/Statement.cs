using System.Text;
using Newtonsoft.Json.Linq;
using QueryBuilder.DataTypes;

namespace QueryBuilder.Statements
{
    public abstract class ColumnStatement : AbstractBase
    {
        protected Dictionary<string, JToken> Columns { get; set; } = new();

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
    }
}