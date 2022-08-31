namespace QueryBuilder.Statements
{
    public class Delete : Statement, IStatement
    {
        public Delete(string tableName)
        {
            TableName = tableName;
        }

        public string ToString(TimeZoneInfo timeZone)
        {
            string whereClauseLiterals = SerializeWhereClauses(timeZone);

            return @$"DELETE FROM {TableName} 
                      WHERE {whereClauseLiterals};";
        }
    }
}