﻿using QueryBuilder.DataTypes;

namespace QueryBuilder
{
    public abstract class AbstractBase
    {
        protected string TableName { get; set; } = "EXAMPLE_TABLE_NAME";
        protected string ModifiedBy { get; set; } = "XYZ";
        protected SqlFunction CurrentTimestampCall { get; set; } = new("CURRENT_TIMESTAMP()");
    }
}
