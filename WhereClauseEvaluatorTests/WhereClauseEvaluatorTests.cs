using NUnit.Framework;
using SqlParser;
using Moq;
using System.Diagnostics;
using System.Linq.Expressions;
using System;
using ExpressionParser;
using Lookup;

namespace WhereClauseEvaluatorTests
{
    [TestFixture]
    public class WhereClauseEvaluatorTests
    {
        private Mock<ILookup> _mockRecord;
        [SetUp]
        public void Init()
        {
            _mockRecord = new Mock<ILookup>();
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
            return Expression.Lambda<Func<bool>>(whereExpression).Compile()();
        }

        [TestCase("((c in (1,2) or c2=2) and (c=1 or c like '0')) or c1=0")]
        [TestCase("not (c in (1, 2))")]
        public void Should_Parse_And_Return_PrefixExpression(string whereClause)
        {
            var parser = new WhereClauseParser(whereClause);
            var whereExpression = parser.ToExpression(_mockRecord.Object);
            var expressionParser = new ExpressionParser.ExpressionParser(whereExpression);
            expressionParser.PrefixExpression.ForEach(e => Debug.WriteLine(e));
            Assert.That(expressionParser.PrefixExpression.Count, Is.GreaterThan(0));
        }

    }

}
