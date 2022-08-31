using QueryBuilder.DataTypes;
using QueryBuilder.Statements;

namespace QueryBuilderTest.StatementTests
{
    internal class TransactionTest : AbstractTest
    {
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
