using Newtonsoft.Json.Linq;
using QueryBuilder.BulkOperations;
using QueryBuilder.Statements;

namespace QueryBuilderTest.BulkOperationTests
{
    internal class BulkMergeTest : AbstractTest
    {
        private List<string> PrimaryKeyIdentifiers { get; set; } = new() { "ID" };
        private int MaxOperationSize { get; set; } = 1000;

        [Test]
        public void TestSingleMerge()
        {
            int start = 1;
            JArray incoming = GetArrayOfEntities(start);
            JArray existing = GetArrayOfEntities(start + 1);

            BulkMerge actual = new(
                incoming, existing, TableName, PrimaryKeyIdentifiers);

            string expected = @$"BEGIN
                                    INSERT INTO {TableName} (
                                        ID,
                                        NAME,
                                        MODIFIED_AT,
                                        MODIFIED_BY
                                    ) VALUES (
                                        {start},
                                        'HANNAH',
                                        {CurrentTimestampCall.Literal},
                                        '{ModifiedBy}'
                                    );
                                 END;";

            string actualStr = actual.ToString(TestHelpers.TimeZone);
            string actualEscaped = TestHelpers.RemoveWhitespace(actualStr);

            string expectedEscaped = TestHelpers.RemoveWhitespace(expected);

            Assert.That(actualEscaped, Is.EqualTo(expectedEscaped));
        }

        [Test]
        public void TestBulkMerge()
        {
            int start = 1;
            JArray incoming = GetBigArrayOfEntities(start, MaxOperationSize);
            JArray existing = GetBigArrayOfEntities(start + 1, MaxOperationSize - 1);

            BulkMerge actual = new(
                incoming, existing, TableName, PrimaryKeyIdentifiers);

            string expected = @$"BEGIN
                                    INSERT INTO {TableName} (
                                        ID,
                                        NAME,
                                        MODIFIED_AT,
                                        MODIFIED_BY
                                    ) VALUES (
                                        {start},
                                        'HANNAH',
                                        {CurrentTimestampCall.Literal},
                                        '{ModifiedBy}'
                                    );

                                    INSERT INTO {TableName} (
                                        ID,
                                        NAME,
                                        MODIFIED_AT,
                                        MODIFIED_BY
                                    ) VALUES (
                                        {MaxOperationSize},
                                        'HANNAH',
                                        {CurrentTimestampCall.Literal},
                                        '{ModifiedBy}'
                                    );
                                 END;";

            string actualStr = actual.ToString(TestHelpers.TimeZone);
            string actualEscaped = TestHelpers.RemoveWhitespace(actualStr);

            string expectedEscaped = TestHelpers.RemoveWhitespace(expected);

            Assert.That(actualEscaped, Is.EqualTo(expectedEscaped));
        }

        [Test]
        public void TestBulkMergeWithBigTransactions()
        {
            JArray incoming = GetBigArrayOfEntities(1, MaxOperationSize);

            BulkMerge actualEntity = new(
                incoming, new JArray(), TableName, PrimaryKeyIdentifiers);
            int actual = actualEntity.GetTransactionCount();

            int expected = (int)Math.Ceiling(MaxOperationSize / (double)actualEntity.MaxTransactionSize);

            Assert.That(actual, Is.EqualTo(expected));
        }

        private static JArray GetBigArrayOfEntities(int min, int max)
        {
            JArray incoming = new();
            for (int i = min; i <= max; i++)
            {
                JObject o = new()
                {
                    ["ID"] = i,
                    ["NAME"] = "HANNAH"
                };
                incoming.Add(o);
            }

            return incoming;
        }

        private BulkMerge PopulateBulkMergeWithTransaction(
            BulkMerge bulkMergeEntity, List<int> id)
        {
            Transaction transaction = new();
            foreach (int i in id)
            {
                Insert example = new(TableName);
                example.AddColumn("ID", i);
                example.AddColumn("NAME", "HANNAH");
                example.AddColumn("MODIFIED_AT", CurrentTimestampCall);
                example.AddColumn("MODIFIED_BY", ModifiedBy);
                transaction.AddStatement(example);
            }

            bulkMergeEntity.AddTransaction(transaction);
            return bulkMergeEntity;
        }

        private static JArray GetArrayOfEntities(int id)
        {
            JArray array = new();
            JObject element = new()
            {
                ["ID"] = id,
                ["NAME"] = "HANNAH"
            };
            array.Add(element);
            return array;
        }
    }
}
