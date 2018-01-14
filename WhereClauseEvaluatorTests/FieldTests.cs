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
            var field = new ColumnOperand(fieldName, fieldValue);
            var constOperand = new ConstantOperand(constValue);
            var expr = Expression.Equal(Expression.Constant(field), Expression.Constant(constOperand));
            return
                Expression.Lambda<Func<bool>>(expr).Compile()()
                &&
                field.Equals(constOperand);
        }

        [TestCase("field1", "10", "11", ExpectedResult = false)]
        [TestCase("field1", "10", "10", ExpectedResult = false)]
        [TestCase("field1", "11", "10", ExpectedResult = true)]
        public bool GreaterThan_Operator_Test(string fieldName, string fieldValue, string constValue)
        {
            var field = new ColumnOperand(fieldName, fieldValue);
            var constOperand = new ConstantOperand(constValue);
            var expr = Expression.GreaterThan(Expression.Constant(field), Expression.Constant(constOperand));
            return
                Expression.Lambda<Func<bool>>(expr).Compile()()
                &&
                field > constOperand;
        }
        [TestCase("field1", "11", "10", ExpectedResult = false)]
        [TestCase("field1", "10", "10", ExpectedResult = false)]
        [TestCase("field1", "10", "11", ExpectedResult = true)]
        public bool LessThan_Operator_Test(string fieldName, string fieldValue, string constValue)
        {
            var field = new ColumnOperand(fieldName, fieldValue);
            var constOperand = new ConstantOperand(constValue);
            var expr = Expression.LessThan(Expression.Constant(field), Expression.Constant(constOperand));
            return
                Expression.Lambda<Func<bool>>(expr).Compile()()
                &&
                field < constOperand;
        }
        [TestCase("field1", "10", "11", ExpectedResult = false)]
        [TestCase("field1", "10", "10", ExpectedResult = true)]
        [TestCase("field1", "11", "10", ExpectedResult = true)]
        public bool GreaterThan_Or_Equal_Operator_Test(string fieldName, string fieldValue, string constValue)
        {
            var field = new ColumnOperand(fieldName, fieldValue);
            var constOperand = new ConstantOperand(constValue);
            var expr = Expression.GreaterThanOrEqual(Expression.Constant(field), Expression.Constant(constOperand));
            return
                Expression.Lambda<Func<bool>>(expr).Compile()()
                &&
                field >= constOperand;
        }

        [TestCase("field1", "11", "10", ExpectedResult = false)]
        [TestCase("field1", "10", "10", ExpectedResult = true)]
        [TestCase("field1", "10", "11", ExpectedResult = true)]
        public bool LessThan_Or_Equal_Operator_Test(string fieldName, string fieldValue, string constValue)
        {
            var field = new ColumnOperand(fieldName, fieldValue);
            var constOperand = new ConstantOperand(constValue);
            var expr = Expression.LessThanOrEqual(Expression.Constant(field), Expression.Constant(constOperand));
            return
                Expression.Lambda<Func<bool>>(expr).Compile()()
                &&
                field <= constOperand;
        }

        private void AssertOperator(object obj, string methodName)
        {
            var op = obj as BinaryOperator;
            Assert.That(op, Is.Not.Null);
            Assert.That(op.Data.ToUpper(), Is.EqualTo(methodName.ToUpper()));
        }

        private void AssertColumnOperand(object obj, string columnName, string columnValue)
        {
            var lhs = obj as ColumnOperand;
            Assert.That(lhs, Is.Not.Null);
            Assert.That(lhs.Data.Key, Is.EqualTo(columnName));
            Assert.That(lhs.Data.Value, Is.EqualTo(columnValue));
        }

        private void AssertConstantOperand(object obj, string data)
        {
            var rhs = obj as ConstantOperand;
            Assert.That(rhs, Is.Not.Null);
            Assert.That(rhs.Data, Is.EqualTo(data));
        }
        private void AssertConstantOfListOperand(object obj, IList<string> list)
        {
            var rhs = obj as ConstantOperandOfList;
            Assert.That(rhs, Is.Not.Null);
            Assert.That(rhs.Data, Is.EqualTo(list));
        }

        [TestCase(ExpectedResult = true)]
        public bool EvaluateLikeOperation()
        {
            var methodName = "Like";

            string columnName = "field";
            string columnValue = "Me";
            var columnOperand = new ColumnOperand(columnName, columnValue);

            string pattern = "M%";
            var constOperand = new ConstantOperand(pattern);

            var comparer = Expression.Call(
                null,
                typeof(SqlOperations).GetMethod(methodName),
                Expression.Constant(columnOperand),
                Expression.Constant(constOperand));

            Debug.WriteLine(comparer);

            var e = new ExpressionParser.ExpressionParser(comparer).PrefixExpression;

            AssertOperator(e[0], methodName);
            AssertColumnOperand(e[1], columnName, columnValue);
            AssertConstantOperand(e[2], pattern);

            return Expression.Lambda<Func<bool>>(comparer).Compile()();
        }

        [TestCase(ExpectedResult = true)]
        public bool EvaluateInOperation()
        {
            var list = new List<string> { "1", "2", "3" };
            var operantOfList = new ConstantOperandOfList(list);

            string columnName = "field";
            string columnValue = "1";
            var columnOperand = new ColumnOperand(columnName, columnValue);
            var methodName = "In";

            var comparer = Expression.Call(
                null,
                typeof(SqlOperations).GetMethod(methodName),
                Expression.Constant(columnOperand),
                Expression.Constant(operantOfList ));

            Debug.WriteLine(comparer);

            var e = new ExpressionParser.ExpressionParser(comparer).PrefixExpression;

            AssertOperator(e[0],methodName);
            AssertColumnOperand(e[1], columnName,columnValue);
            AssertConstantOfListOperand(e[2], list);

            return Expression.Lambda<Func<bool>>(comparer).Compile()();
        }
    }
   
}
