using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Text.RegularExpressions;

namespace SqlUtil
{
    public interface IRecord
    {
        string GetValue(string columnName);
    }
    public class WhereClauseEvaluator<T> where T : IRecord
    {
        private readonly string _whereClause;
        private readonly T _record;

        public WhereClauseEvaluator(string whereClause, T recored)
        {
            _whereClause = whereClause;
            _record = recored;
        }

        public bool Evaluate()
        {
            IList<ParseError> errors = null;
            var whereExpression = new TSql100Parser(false)
                .ParseBooleanExpression(new StringReader(_whereClause),
                out errors);
            if(errors != null && errors.Any())
            {
                throw new Exception($"Error {errors} while parsing");
            }
            return Evaluate(whereExpression);
        }

        private bool Evaluate(BooleanExpression expression)
        {
            if (expression is BooleanBinaryExpression)
            {
                var expr = (BooleanBinaryExpression)expression;
                switch (expr.BinaryExpressionType)
                {
                    case BooleanBinaryExpressionType.And:
                        return Evaluate(expr.FirstExpression)
                            &&
                            Evaluate(expr.SecondExpression);
                    case BooleanBinaryExpressionType.Or:
                        return Evaluate(expr.FirstExpression)
                            ||
                            Evaluate(expr.SecondExpression);
                }
            }

            if(expression is BooleanComparisonExpression)
            {
                var expr = (BooleanComparisonExpression)expression;
                var pair = GetKeyValue(expr);
                var lhs = _record.GetValue(pair.Key);
                var rhs = pair.Value;

                switch(expr.ComparisonType)
                {
                    case BooleanComparisonType.Equals:
                        return lhs == rhs;
                    case BooleanComparisonType.NotEqualToExclamation:
                        return lhs != rhs;
                    case BooleanComparisonType.GreaterThan:
                        return Double.Parse(lhs) > Double.Parse(rhs);
                    case BooleanComparisonType.LessThan:
                        return Double.Parse(lhs) < Double.Parse(rhs);
                    case BooleanComparisonType.GreaterThanOrEqualTo:
                        return Double.Parse(lhs) >= Double.Parse(rhs);
                    case BooleanComparisonType.LessThanOrEqualTo:
                        return Double.Parse(lhs) <= Double.Parse(rhs);
                    case BooleanComparisonType.NotEqualToBrackets:
                        return Double.Parse(lhs) > Double.Parse(rhs);
                    default:
                        throw new NotImplementedException("${expr.ComparisonType} not implemented");
                }
            }

            if(expression is BooleanParenthesisExpression)
            {
                var expr = (BooleanParenthesisExpression)expression;
                return Evaluate(expr.Expression);
            }

            if(expression is LikePredicate)
            {
                var expr = (LikePredicate)expression;
                var pair = GetKeyValue(expr);
                var lhs = _record.GetValue(pair.Key);
                var rhs = pair.Value;
                rhs = rhs.Replace("%", ".*");
                return Regex.IsMatch(lhs, rhs);                
            }

            if(expression is InPredicate)
            {
                var expr = (InPredicate)expression;
                var pair = GetKeyValue(expr);
                var lhs = _record.GetValue(pair.Key);
                var rhs = pair.Value;
                var x = pair.Value[0];
                return rhs.Any(e => ((Literal)e).Value == lhs);
            }

            throw new InvalidExpressionException($"{expression} not supported");

        }
        private static KeyValuePair<string, List<ScalarExpression>> GetKeyValue(InPredicate expr)
        {
            if (expr.Expression is ColumnReferenceExpression == false)
                throw new InvalidExpressionException($"Expected ColumnReferenceExpresssion. {expr} not supported");

            var key = ((ColumnReferenceExpression)expr.Expression)
                .MultiPartIdentifier
                .Identifiers
                .ElementAtOrDefault(0)
                ?.Value;


            return new KeyValuePair<string, List<ScalarExpression>>(key, expr.Values.ToList());
        }

        // TODO make overloaded GetKeyValue a generic method
        private static KeyValuePair<string, string> GetKeyValue(LikePredicate expr)
        {
            if (expr.FirstExpression is ColumnReferenceExpression == false)
                throw new InvalidExpressionException($"Expected ColumnReferenceExpresssion. {expr} not supported");

            var key = ((ColumnReferenceExpression)expr.FirstExpression)
                .MultiPartIdentifier
                .Identifiers
                .ElementAtOrDefault(0)
                ?.Value;

            if (expr.SecondExpression is Literal == false)
                throw new InvalidExpressionException($"Exprected Litereal, {expr} not supported");

            var value = ((Literal)expr.SecondExpression).Value;

            return new KeyValuePair<string, string>(key, value);
        }

        private static KeyValuePair<string, string> GetKeyValue(BooleanComparisonExpression expr)
        {
            if (expr.FirstExpression is ColumnReferenceExpression == false)
                throw new InvalidExpressionException($"Expected ColumnReferenceExpresssion. {expr} not supported");

            var key = ((ColumnReferenceExpression)expr.FirstExpression)
                .MultiPartIdentifier
                .Identifiers
                .ElementAtOrDefault(0)
                ?.Value;

            if (expr.SecondExpression is Literal == false)
                throw new InvalidExpressionException($"Exprected Litereal, {expr} not supported");

            var value = ((Literal)expr.SecondExpression).Value;

            return new KeyValuePair<string, string>(key, value);
        }
    }

}
