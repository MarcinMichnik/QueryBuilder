using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryBuilder.DataTypes
{
    public class SqlFunction
    {
        public string Literal { get; set; }

        public SqlFunction(string literal)
        {
            Literal = literal;
        }
    }
}
