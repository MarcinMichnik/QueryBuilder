using Newtonsoft.Json.Linq;

namespace QueryBuilder
{
    interface IStatement
    {
        void AddColumn(string name, JToken value);
        string ToString();
    }
}