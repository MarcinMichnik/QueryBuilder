using Newtonsoft.Json.Linq;

namespace QueryBuilder
{
    public abstract class Statement : IStatement
    {
        public string TableName { get; set; }
        public Dictionary<string, JToken> Columns { get; set; }

        protected Statement(string tableName)
        {
            TableName = tableName;
            Columns = new Dictionary<string, JToken>();
        }

        public void AddColumn(string name, JToken value)
        {
            Columns.Add(name, value);
        }

        protected string ConvertJTokenToString(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.String:
                    return $"'{token}'";
                case JTokenType.Integer:
                    return token.ToString();
                case JTokenType.Float:
                    string stringLiteral = token.ToString();
                    return stringLiteral.Replace(",", ".");
                case JTokenType.Date:
                    DateTime datetimeValue = (DateTime)token;
                    DateTime utcDateTime = datetimeValue.ToUniversalTime();
                    string utcDateTimeString = utcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss");
                    return $"TO_DATE('{utcDateTimeString}', 'YYYY-MM-DD\"T\"HH24:MI:SS')";
                default:
                    throw new Exception("NOT EXPECTED");
            }
        }

        string IStatement.ToString()
        {
            throw new Exception("NOT EXPECTED"); ;
        }
    }
}