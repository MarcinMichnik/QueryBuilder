using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using QueryBuilder;
using QueryBuilder.DataTypes;
using QueryBuilder.Statements;

namespace QueryBuilderTest
{
    public class QueryBuilderTest
    {
        public SqlFunction SqlFunctionEntity { get; set; } = new("CURRENT_TIMESTAMP()");
        public string TableName { get; set; }

        [SetUp]
        public void Setup()
        {
            TableName = "\"APP\".\"EXAMPLARY_TABLE_NAME\"";
        }

        [Test]
        public void TestInsertWithSequencedMasterPrimaryKey()
        {
            Insert query = GetInsertWithMasterPrimaryKey(1);
            string actual = query.ToString();

            Insert expected = new(TableName);
            expected.AddColumn("MASTER_ID", new SqlFunction("SEQ.NEXT_VAL"));
            expected.AddColumn("ID", 1);
            expected.AddColumn("NAME", "HANNAH");
            expected.AddColumn("SAVINGS", 12.1);
            expected.AddColumn("MODIFIED_AT", SqlFunctionEntity);
            expected.AddColumn("MODIFIED_BY", "NOT LOGGED IN");

            string actualEscaped = Regex.Replace(actual, @"\s", "");
            string expectedEscaped = Regex.Replace(expected.ToString(), @"\s", "");

            Assert.That(actualEscaped, Is.EqualTo(expectedEscaped));
        }

        [Test]
        public void TestInsertWithoutMasterPrimaryKey()
        {
            Statement query = GetInsertWithoutMasterPrimaryKey();

            string actual = query.ToString();

            Insert expected = new(TableName);
            expected.AddColumn("ID", 1);
            expected.AddColumn("NAME", "HANNAH");
            expected.AddColumn("SAVINGS", 12.1);
            expected.AddColumn("MODIFIED_AT", SqlFunctionEntity);
            expected.AddColumn("MODIFIED_BY", "NOT LOGGED IN");

            string actualEscaped = Regex.Replace(actual, @"\s", "");
            string expectedEscaped = Regex.Replace(expected.ToString(), @"\s", "");

            Assert.That(actualEscaped, Is.EqualTo(expectedEscaped));
        }

        [Test]
        public void TestUpdateWithOnePrimaryKey()
        {
            Update query = GetUpdateWithOnePrimaryKey();

            string actual = query.ToString();

            Update expected = new(TableName);
            expected.AddColumn("NAME", "HANNAH");
            expected.AddColumn("SAVINGS", 12.1);
            expected.AddColumn("MODIFIED_AT", SqlFunctionEntity);
            expected.AddColumn("MODIFIED_BY", "NOT LOGGED IN");
            expected.PrimaryKeyLookups.Add(new KeyValuePair<string, JToken>("ID", 1));

            string actualEscaped = Regex.Replace(actual, @"\s", "");
            string expectedEscaped = Regex.Replace(expected.ToString(), @"\s", "");

            Assert.That(actualEscaped, Is.EqualTo(expectedEscaped));
        }

        [Test]
        public void TestUpdateWithManyPrimaryKeys()
        {
            Update query = GetUpdateWithManyPrimaryKeys();
            string actual = query.ToString();

            Update expected = new(TableName);
            expected.AddColumn("NAME", "HANNAH");
            expected.AddColumn("SAVINGS", 12.1);
            expected.AddColumn("MODIFIED_AT", SqlFunctionEntity);
            expected.AddColumn("MODIFIED_BY", "NOT LOGGED IN");
            expected.PrimaryKeyLookups.Add(new KeyValuePair<string, JToken>("ID", 1));
            expected.PrimaryKeyLookups.Add(new KeyValuePair<string, JToken>("EXTERNAL_ID", 301));

            string actualEscaped = Regex.Replace(actual, @"\s", "");
            string expectedEscaped = Regex.Replace(expected.ToString(), @"\s", "");

            Assert.That(actualEscaped, Is.EqualTo(expectedEscaped));
        }

        [Test]
        public void TestTransactionWithTwoInserts()
        {
            Transaction query = GetTransactionWithTwoInserts();
            string actual = query.ToString();

            Transaction expected = new();
            Insert expectedOne = new(TableName);
            expectedOne.AddColumn("MASTER_ID", new SqlFunction("SEQ.NEXT_VAL"));
            expectedOne.AddColumn("ID", 1);
            expectedOne.AddColumn("NAME", "HANNAH");
            expectedOne.AddColumn("SAVINGS", 12.1);
            expectedOne.AddColumn("MODIFIED_AT", SqlFunctionEntity);
            expectedOne.AddColumn("MODIFIED_BY", "NOT LOGGED IN");
            expected.Statements.Add(expectedOne);

            Insert expectedTwo = new(TableName);
            expectedTwo.AddColumn("MASTER_ID", new SqlFunction("SEQ.NEXT_VAL"));
            expectedTwo.AddColumn("ID", 2);
            expectedTwo.AddColumn("NAME", "HANNAH");
            expectedTwo.AddColumn("SAVINGS", 12.1);
            expectedTwo.AddColumn("MODIFIED_AT", SqlFunctionEntity);
            expectedTwo.AddColumn("MODIFIED_BY", "NOT LOGGED IN");
            expected.Statements.Add(expectedTwo);

            string actualEscaped = Regex.Replace(actual, @"\s", "");
            string expectedEscaped = Regex.Replace(expected.ToString(), @"\s", "");

            Assert.That(actualEscaped, Is.EqualTo(expectedEscaped));
        }

        private Insert GetInsertWithMasterPrimaryKey(int id)
        {
            Insert query = new(TableName);

            query.AddColumn("MASTER_ID", new SqlFunction("SEQ.NEXT_VAL"));
            query.AddColumn("ID", id);
            query.AddColumn("NAME", "HANNAH");
            query.AddColumn("SAVINGS", 12.1);
            query.AddColumn("MODIFIED_AT", SqlFunctionEntity);
            query.AddColumn("MODIFIED_BY", "NOT LOGGED IN");

            return query;
        }

        private Insert GetInsertWithoutMasterPrimaryKey()
        {
            Insert query = new Insert(TableName);

            query.AddColumn("ID", 1);
            query.AddColumn("NAME", "HANNAH");
            query.AddColumn("SAVINGS", 12.1);
            query.AddColumn("MODIFIED_AT", SqlFunctionEntity);
            query.AddColumn("MODIFIED_BY", "NOT LOGGED IN");

            return query;
        }

        private Update GetUpdateWithOnePrimaryKey()
        {
            Update query = new Update(TableName);

            KeyValuePair<string, JToken> primaryKeyLookup
                = new KeyValuePair<string, JToken>("ID", 1);
            query.PrimaryKeyLookups.Add(primaryKeyLookup);

            query.AddColumn("NAME", "HANNAH");
            query.AddColumn("SAVINGS", 12.1);
            query.AddColumn("MODIFIED_AT", SqlFunctionEntity);
            query.AddColumn("MODIFIED_BY", "NOT LOGGED IN");

            return query;
        }

        private Update GetUpdateWithManyPrimaryKeys()
        {
            Update query = new Update(TableName);

            KeyValuePair<string, JToken> primaryKeyLookup 
                = new KeyValuePair<string, JToken>("ID", 1);
            query.PrimaryKeyLookups.Add(primaryKeyLookup);

            KeyValuePair<string, JToken> primaryKeyLookup2 
                = new KeyValuePair<string, JToken>("EXTERNAL_ID", 301);
            query.PrimaryKeyLookups.Add(primaryKeyLookup2);

            query.AddColumn("NAME", "HANNAH");
            query.AddColumn("SAVINGS", 12.1);
            query.AddColumn("MODIFIED_AT", SqlFunctionEntity);
            query.AddColumn("MODIFIED_BY", "NOT LOGGED IN");

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