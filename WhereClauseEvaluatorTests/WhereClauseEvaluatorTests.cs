using NUnit.Framework;
using SqlUtil;
using Moq;
using System.Diagnostics;
using System.Linq.Expressions;
using System;
using ExpressionParser;

namespace WhereClauseEvaluatorTests
{
    [TestFixture]
    public class WhereClauseEvaluatorTests
    {
        private Mock<IRecord> _mockRecord;
        [SetUp]
        public void Init()
        {
            _mockRecord = new Mock<IRecord>();
            _mockRecord.Setup(r => r.GetValue("c")).Returns("0");
            _mockRecord.Setup(r => r.GetValue("c1")).Returns("1");
            _mockRecord.Setup(r => r.GetValue("c2")).Returns("2");
        }

        [TestCase("c=0", ExpectedResult=true)]
        [TestCase("c=0 and c1=1", ExpectedResult = true)]
        [TestCase("(c=0 or c=1) and (c2=2)", ExpectedResult = true)]
        [TestCase("((c=0 or c=1) and (c2=2))", ExpectedResult = true)]
        [TestCase("c like 0", ExpectedResult = true)]
        [TestCase("c like '%0%'", ExpectedResult = true)]
        [TestCase("c in (0,1,2)", ExpectedResult = true)]
        [TestCase("c in (1,2)", ExpectedResult = false)]
        [TestCase("((c in (1,2) or c2=2) and (c=1 or c like '0')) or c1=0", ExpectedResult = true)]
        [TestCase("not (c in (1, 2))", ExpectedResult = true)]
        public bool EvaluateCorrectly(string whereClause)
        {
            var parser = new WhereClauseParser(whereClause);
            var whereExpression = parser.ToExpression(_mockRecord.Object);
            Debug.WriteLine($"{whereClause} =>\n {whereExpression}");
            Debug.WriteLine(parser.LastResult);
            return Expression.Lambda<Func<bool>>(whereExpression).Compile()();
        }

        [TestCase("((c in (1,2) or c2=2) and (c=1 or c like '0')) or c1=0")]
        public void Should_Parse_And_Return_PrefixExpression(string whereClause)
        {
            var parser = new WhereClauseParser(whereClause);
            var whereExpression = parser.ToExpression(_mockRecord.Object);
            var expressionParser = new ExpressionParser.ExpressionParser(whereExpression);
            expressionParser.PrefixExpression.ForEach(e => Debug.WriteLine(e));
            Assert.That(expressionParser.PrefixExpression.Count, Is.GreaterThan(0));
        }

        private class Field : IEquatable<string>
        {
            public Field(string name, string value)
            {
                Name = name;
                Value = value;
            }
            public string Name { get;  }
            public string Value { get; }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
            public override bool Equals(object other)
            {
                return Value.Equals(other);
            }
            public bool Equals(string other)
            {
                return Value.Equals(other);
            }

            public static bool operator == (Field lhs, string rhs)
            {
                return lhs.Equals(rhs);
            }
            public static bool operator != (Field lhs, string rhs)
            {
                return !lhs.Equals(rhs);
            }

            public static bool operator > (Field lhs, string rhs)
            {
                return
                    double.Parse(lhs.Value)
                    >
                    double.Parse(rhs);
            }
            public static bool operator < (Field lhs, string rhs)
            {
                return
                    double.Parse(lhs.Value)
                    <
                    double.Parse(rhs);
            }

            public static bool operator <= (Field lhs, string rhs)
            {
                return
                    double.Parse(lhs.Value)
                    <=
                    double.Parse(rhs);
            }
            public static bool operator >= (Field lhs, string rhs)
            {
                return
                    double.Parse(lhs.Value)
                    >=
                    double.Parse(rhs);
            }
        }

        [TestCase("field1","10", "10", ExpectedResult = true)]
        [TestCase("field1","10", "20", ExpectedResult = false)]
        public bool EqualityTest(string fieldName, string fieldValue, string constValue)
        {
            var field = new Field(fieldName, fieldValue);
            var expr = Expression.Equal(Expression.Constant(field), Expression.Constant(constValue));
            return
                Expression.Lambda<Func<bool>>(expr).Compile()()
                &&
                field.Equals(constValue);
        }

        [TestCase("field1","10", "11", ExpectedResult = false)]
        [TestCase("field1","10", "10", ExpectedResult = false)]
        [TestCase("field1","11", "10", ExpectedResult = true)]
        public bool GreaterThan_Operator_Test(string fieldName, string fieldValue, string constValue)
        {
            var field = new Field(fieldName, fieldValue);
            var expr = Expression.GreaterThan(Expression.Constant(field), Expression.Constant(constValue));
            return
                Expression.Lambda<Func<bool>>(expr).Compile()()
                &&
                field > constValue;
        }
        [TestCase("field1","11", "10", ExpectedResult = false)]
        [TestCase("field1","10", "10", ExpectedResult = false)]
        [TestCase("field1","10", "11", ExpectedResult = true)]
        public bool LessThan_Operator_Test(string fieldName, string fieldValue, string constValue)
        {
            var field = new Field(fieldName, fieldValue);
            var expr = Expression.LessThan(Expression.Constant(field), Expression.Constant(constValue));
            return
                Expression.Lambda<Func<bool>>(expr).Compile()()
                &&
                field < constValue;
        }
        [TestCase("field1","10", "11", ExpectedResult = false)]
        [TestCase("field1","10", "10", ExpectedResult = true)]
        [TestCase("field1","11", "10", ExpectedResult = true)]
        public bool GreaterThan_Or_Equal_Operator_Test(string fieldName, string fieldValue, string constValue)
        {
            var field = new Field(fieldName, fieldValue);
            var expr = Expression.GreaterThanOrEqual(Expression.Constant(field), Expression.Constant(constValue));
            return
                Expression.Lambda<Func<bool>>(expr).Compile()()
                &&
                field >= constValue;
        }
        [TestCase("field1","11", "10", ExpectedResult = false)]
        [TestCase("field1","10", "10", ExpectedResult = true)]
        [TestCase("field1","10", "11", ExpectedResult = true)]
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
