using Newtonsoft.Json.Linq;

namespace QueryBuilder.Statements
{
    public interface IStatement
    {
        void AddColumn(string name, JToken value);
        string ToString();
    }
}