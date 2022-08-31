using QueryBuilder.DataTypes;
using QueryBuilder.Statements;

namespace QueryBuilderTest
{
    public abstract class AbstractTest
    {
        protected SqlFunction CurrentTimestampCall { get; set; } = new("CURRENT_TIMESTAMP()");
        protected string TableName { get; set; } = "\"APP\".\"EXAMPLE_TABLE_NAME\"";
        protected TimeZoneInfo TimeZone { get; set; }
            = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

        protected Insert GetInsertWithMasterPrimaryKey(int id)
        {
            Insert query = new(TableName);

            query.AddColumn("MASTER_ID", new SqlFunction("SEQ.NEXT_VAL"));
            query.AddColumn("ID", id);
            query.AddColumn("NAME", "HANNAH");
            query.AddColumn("SAVINGS", 12.1);
            query.AddColumn("MODIFIED_AT", CurrentTimestampCall);
            query.AddColumn("MODIFIED_BY", "NOT LOGGED IN");

            return query;
        }
    }
}
