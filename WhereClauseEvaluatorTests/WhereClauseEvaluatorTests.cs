using NUnit.Framework;
using SqlUtil;
using Moq;

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
        public bool EvaluateCorrectly(string whereClause)
        {
            return WhereClauseParser.Evaluate(whereClause, _mockRecord.Object);
        }
    }
}
