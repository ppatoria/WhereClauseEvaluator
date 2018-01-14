using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using SqlUtil;

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

        [TestCase(ExpectedResult = true)]
        public bool EvaluateLikeOperation()
        {
            var comparer = Expression.Call(
                null,
                typeof(SqlOperations).GetMethod("Like"),
                Expression.Constant(new Field("field","Me")),
                Expression.Constant("M%"));
            Debug.WriteLine(comparer);
            var e = new ExpressionParser.ExpressionParser(comparer).PrefixExpression;
            Assert.That(string.Join(",", e), Is.EqualTo("Like,field[Me],M%"));
            return Expression.Lambda<Func<bool>>(comparer).Compile()();
        }

        [TestCase(ExpectedResult = true)]
        public bool EvaluateInOperation()
        {
            var comparer = Expression.Call(
                null,
                typeof(SqlOperations).GetMethod("In"),
                Expression.Constant(new Field("field","1")),
                Expression.Constant(new List<string> { "1","2","3"}));

            Debug.WriteLine(comparer);

            var e = new ExpressionParser.ExpressionParser(comparer).PrefixExpression;

            Assert.That(string.Join(",", e), Is.EqualTo("In,field[1],(1,2,3)"));

            return Expression.Lambda<Func<bool>>(comparer).Compile()();
        }
    }
    public static class SqlOperations
    {
        public static bool Like(this Field lhs, string rhs)
        {
            return Regex.IsMatch(lhs.Value, (rhs.Replace("%", ".*")));
        }

        public static bool In(this Field lhs, IList<string> rhs)
        {
            return rhs.Contains(lhs.Value);
        }
    }
}
