using System;
using System.Linq.Expressions;
using NUnit.Framework;
using WhereClauseEvaluator;

namespace WhereClauseEvaluatorTests
{
    [TestFixture]
    class FieldTests
    {
        [TestCase("field1", "10", "10", ExpectedResult = true)]
        [TestCase("field1", "10", "20", ExpectedResult = false)]
        public bool EqualityTest(string fieldName, string fieldValue, string constValue)
        {
            var field = new Field(fieldName, fieldValue);
            var expr = Expression.Equal(Expression.Constant(field), Expression.Constant(constValue));
            return
                Expression.Lambda<Func<bool>>(expr).Compile()()
                &&
                field.Equals(constValue);
        }

        [TestCase("field1", "10", "11", ExpectedResult = false)]
        [TestCase("field1", "10", "10", ExpectedResult = false)]
        [TestCase("field1", "11", "10", ExpectedResult = true)]
        public bool GreaterThan_Operator_Test(string fieldName, string fieldValue, string constValue)
        {
            var field = new Field(fieldName, fieldValue);
            var expr = Expression.GreaterThan(Expression.Constant(field), Expression.Constant(constValue));
            return
                Expression.Lambda<Func<bool>>(expr).Compile()()
                &&
                field > constValue;
        }
        [TestCase("field1", "11", "10", ExpectedResult = false)]
        [TestCase("field1", "10", "10", ExpectedResult = false)]
        [TestCase("field1", "10", "11", ExpectedResult = true)]
        public bool LessThan_Operator_Test(string fieldName, string fieldValue, string constValue)
        {
            var field = new Field(fieldName, fieldValue);
            var expr = Expression.LessThan(Expression.Constant(field), Expression.Constant(constValue));
            return
                Expression.Lambda<Func<bool>>(expr).Compile()()
                &&
                field < constValue;
        }
        [TestCase("field1", "10", "11", ExpectedResult = false)]
        [TestCase("field1", "10", "10", ExpectedResult = true)]
        [TestCase("field1", "11", "10", ExpectedResult = true)]
        public bool GreaterThan_Or_Equal_Operator_Test(string fieldName, string fieldValue, string constValue)
        {
            var field = new Field(fieldName, fieldValue);
            var expr = Expression.GreaterThanOrEqual(Expression.Constant(field), Expression.Constant(constValue));
            return
                Expression.Lambda<Func<bool>>(expr).Compile()()
                &&
                field >= constValue;
        }
        [TestCase("field1", "11", "10", ExpectedResult = false)]
        [TestCase("field1", "10", "10", ExpectedResult = true)]
        [TestCase("field1", "10", "11", ExpectedResult = true)]
        public bool LessThan_Or_Equal_Operator_Test(string fieldName, string fieldValue, string constValue)
        {
            var field = new Field(fieldName, fieldValue);
            var expr = Expression.LessThanOrEqual(Expression.Constant(field), Expression.Constant(constValue));
            return
                Expression.Lambda<Func<bool>>(expr).Compile()()
                &&
                field <= constValue;
        }
    }
}
