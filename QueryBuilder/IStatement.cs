using Newtonsoft.Json.Linq;

namespace QueryBuilder
{
    public interface IStatement
    {
        void AddColumn(string name, JToken value);
        string ToString();
    }
}