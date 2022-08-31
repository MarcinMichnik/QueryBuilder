namespace QueryBuilder.Statements
{
    public class Delete : FilterableStatement, IStatement
    {
        public Delete(string tableName)
        {
            TableName = tableName;
        }

        public string ToString(TimeZoneInfo timeZone)
        {
            string whereClauseLiterals = SerializeWhereClauses(timeZone);

            return @$"DELETE FROM {TableName} 
                      {whereClauseLiterals};";
        }
    }
}