using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using QueryBuilder;
using QueryBuilder.DataTypes;
using QueryBuilder.Statements;

namespace QueryBuilderTest
{
    public class QueryBuilderTest
    {
        public SqlFunction CurrentTimestampCall { get; set; } = new("CURRENT_TIMESTAMP()");
        public string TableName { get; set; } = "\"APP\".\"EXAMPLE_TABLE_NAME\"";
        private TimeZoneInfo TimeZone { get; set; } 
            = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

        [Test]
        public void TestInsertWithSequencedMasterPrimaryKey()
        {
            Insert query = GetInsertWithMasterPrimaryKey(1);

            string expected = @$"INSERT INTO {TableName} (
                                    MASTER_ID,
                                    ID,
                                    NAME,
                                    SAVINGS,
                                    MODIFIED_AT,
                                    MODIFIED_BY
                                ) VALUES (
                                    SEQ.NEXT_VAL,
                                    1,
                                    'HANNAH',
                                    12.1,
                                    {CurrentTimestampCall.Literal},
                                    'NOT LOGGED IN'
                                );";

            string actual = query.ToString(TimeZone);
            string actualEscaped = TestHelpers.RemoveWhitespace(actual);
            string expectedEscaped = TestHelpers.RemoveWhitespace(expected);

            Assert.That(actualEscaped, Is.EqualTo(expectedEscaped));
        }

        [Test]
        public void TestInsertWithoutMasterPrimaryKey()
        {
            Insert query = GetInsertWithoutMasterPrimaryKey();

            string expected = @$"INSERT INTO {TableName} (
                                    ID,
                                    NAME,
                                    SAVINGS,
                                    DATE_FROM,
                                    MODIFIED_AT,
                                    MODIFIED_BY
                                ) VALUES (
                                    1,
                                    'HANNAH',
                                    12.1,
                                    TO_DATE('2022-01-01""T""00:00:00', 'YYYY-MM-DD""T""HH24:MI:SS'),
                                    {CurrentTimestampCall.Literal},
                                    'NOT LOGGED IN'
                                );";

            string actual = query.ToString(TimeZone);
            string actualEscaped = TestHelpers.RemoveWhitespace(actual);
            string expectedEscaped = TestHelpers.RemoveWhitespace(expected);

            Assert.That(actualEscaped, Is.EqualTo(expectedEscaped));
        }

        [Test]
        public void TestUpdateWithOnePrimaryKey()
        {
            Update query = GetUpdateWithOnePrimaryKey();

            string expected = @$"UPDATE {TableName} SET 
                                    NAME = 'HANNAH',
                                    SAVINGS = 12.1,
                                    MODIFIED_AT = {CurrentTimestampCall.Literal},
                                    MODIFIED_BY = 'NOT LOGGED IN'
                                WHERE
                                    ID = 1;";

            string actual = query.ToString(TimeZone);
            string actualEscaped = TestHelpers.RemoveWhitespace(actual);
            string expectedEscaped = TestHelpers.RemoveWhitespace(expected.ToString());

            Assert.That(actualEscaped, Is.EqualTo(expectedEscaped));
        }

        [Test]
        public void TestUpdateWithManyPrimaryKeys()
        {
            Update query = GetUpdateWithManyPrimaryKeys();

            string expected = @$"UPDATE {TableName} SET 
                                    NAME = 'HANNAH',
                                    SAVINGS = 12.1,
                                    MODIFIED_AT = {CurrentTimestampCall.Literal},
                                    MODIFIED_BY = 'NOT LOGGED IN'
                                WHERE
                                    ID = 1 
                                    AND EXTERNAL_ID = 301;";

            string actual = query.ToString(TimeZone);
            string actualEscaped = TestHelpers.RemoveWhitespace(actual);
            string expectedEscaped = TestHelpers.RemoveWhitespace(expected.ToString());

            Assert.That(actualEscaped, Is.EqualTo(expectedEscaped));
        }

        [Test]
        public void TestTransactionWithTwoInserts()
        {
            Transaction query = GetTransactionWithTwoInserts();

            string expected = @$"BEGIN
                                    INSERT INTO {TableName} (
                                        MASTER_ID,
                                        ID,
                                        NAME,
                                        SAVINGS,
                                        MODIFIED_AT,
                                        MODIFIED_BY
                                    ) VALUES (
                                        SEQ.NEXT_VAL,
                                        1,
                                        'HANNAH',
                                        12.1,
                                        {CurrentTimestampCall.Literal},
                                        'NOT LOGGED IN'
                                    );

                                    INSERT INTO {TableName} (
                                        MASTER_ID,
                                        ID,
                                        NAME,
                                        SAVINGS,
                                        MODIFIED_AT,
                                        MODIFIED_BY
                                    ) VALUES (
                                        SEQ.NEXT_VAL,
                                        2,
                                        'HANNAH',
                                        12.1,
                                        {CurrentTimestampCall.Literal},
                                        'NOT LOGGED IN'
                                    );
                                END;";

            string actual = query.ToString(TimeZone);
            string actualEscaped = TestHelpers.RemoveWhitespace(actual);
            string expectedEscaped = TestHelpers.RemoveWhitespace(expected);

            Assert.That(actualEscaped, Is.EqualTo(expectedEscaped));
        }

        private Insert GetInsertWithMasterPrimaryKey(int id)
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

        private Insert GetInsertWithoutMasterPrimaryKey()
        {
            Insert query = new(TableName);

            query.AddColumn("ID", 1);
            query.AddColumn("NAME", "HANNAH");
            query.AddColumn("SAVINGS", 12.1);
            query.AddColumn("DATE_FROM", DateTime.Parse("2022-01-01T00:00:00+01:00"));
            query.AddColumn("MODIFIED_AT", CurrentTimestampCall);
            query.AddColumn("MODIFIED_BY", "NOT LOGGED IN");

            return query;
        }

        private Update GetUpdateWithOnePrimaryKey()
        {
            Update query = new(TableName);
            query.AddColumn("NAME", "HANNAH");
            query.AddColumn("SAVINGS", 12.1);
            query.AddColumn("MODIFIED_AT", CurrentTimestampCall);
            query.AddColumn("MODIFIED_BY", "NOT LOGGED IN");

            query.Where("ID", "=", 1);

            return query;
        }

        private Update GetUpdateWithManyPrimaryKeys()
        {
            Update query = new(TableName);
            query.AddColumn("NAME", "HANNAH");
            query.AddColumn("SAVINGS", 12.1);
            query.AddColumn("MODIFIED_AT", CurrentTimestampCall);
            query.AddColumn("MODIFIED_BY", "NOT LOGGED IN");

            query.Where("ID", "=", 1);
            query.Where("EXTERNAL_ID", "=", 301);

            return query;
        }

        private Transaction GetTransactionWithTwoInserts()
        {
            Transaction query = new();

            Insert insert1 = GetInsertWithMasterPrimaryKey(1);
            Insert insert2 = GetInsertWithMasterPrimaryKey(2);

            query.Statements.Add(insert1);
            query.Statements.Add(insert2);

            return query;
        }
    }
}