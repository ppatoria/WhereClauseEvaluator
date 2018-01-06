using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Text.RegularExpressions;
using System.Linq.Expressions;

namespace SqlUtil
{
    public interface IRecord
    {
        string GetValue(string columnName);
    }
    public static class WhereClauseParser
    {
        public static Expression ToExpression<T>(this string whereClause, T record) where T:IRecord
        {
            IList<ParseError> errors = null;
            var whereExpression = new TSql100Parser(false)
                .ParseBooleanExpression(new StringReader(whereClause),
                out errors);
            if(errors != null && errors.Any())
            {
                throw new Exception($"Error {errors} while parsing");
            }
            return ToExpression(whereExpression, record);
        }

        private static Expression ToExpression<T>(BooleanExpression expression, T record ) where T:IRecord
        {
            if (expression is BooleanBinaryExpression)
            {
                var expr = (BooleanBinaryExpression)expression;
                switch (expr.BinaryExpressionType)
                {
                    case BooleanBinaryExpressionType.And:
                        return Expression.And(
                            ToExpression(expr.FirstExpression, record),
                            ToExpression(expr.SecondExpression, record));
                    case BooleanBinaryExpressionType.Or:
                        return Expression.Or(
                            ToExpression<T>(expr.FirstExpression, record),
                            ToExpression(expr.SecondExpression, record));
                }
            }

            if(expression is BooleanComparisonExpression)
            {
                var expr = (BooleanComparisonExpression)expression;
                var pair = GetKeyValue(expr);
                var lhs = record.GetValue(pair.Key);
                var rhs = pair.Value;

                switch(expr.ComparisonType)
                {
                    case BooleanComparisonType.Equals:
                        return Expression.Equal(
                            Expression.Constant(lhs),
                            Expression.Constant(rhs));
                    case BooleanComparisonType.NotEqualToExclamation:
                        return Expression.NotEqual(
                            Expression.Constant(lhs),
                            Expression.Constant(rhs));
                    case BooleanComparisonType.GreaterThan:
                        return Expression.GreaterThan(
                            Expression.Constant(lhs),
                            Expression.Constant(rhs));
                    case BooleanComparisonType.LessThan:
                        return Expression.LessThan(
                            Expression.Constant(lhs),
                            Expression.Constant(rhs));
                    case BooleanComparisonType.GreaterThanOrEqualTo:
                        return Expression.GreaterThanOrEqual(
                            Expression.Constant(lhs),
                            Expression.Constant(rhs));
                    case BooleanComparisonType.LessThanOrEqualTo:
                        return Expression.LessThanOrEqual(
                            Expression.Constant(lhs),
                            Expression.Constant(rhs));
                    default:
                        throw new NotImplementedException("${expr.ComparisonType} not implemented");
                }
            }

            if(expression is BooleanParenthesisExpression)
            {
                var expr = (BooleanParenthesisExpression)expression;
                return ToExpression(expr.Expression, record);
            }

            if(expression is LikePredicate)
            {
                var expr = (LikePredicate)expression;
                var pair = GetKeyValue(expr);
                var lhs = record.GetValue(pair.Key);
                var rhs = pair.Value;
                rhs = rhs.Replace("%", ".*");
                var re = new Regex(rhs);
                return Expression.Call(
                    Expression.Constant(re),
                    (typeof(Regex)).GetMethod("IsMatch", new Type[] { typeof(string) }),
                    Expression.Constant(lhs));
            }

            if(expression is InPredicate)
            {
                var expr = (InPredicate)expression;
                var pair = GetKeyValue(expr);
                var lhs = record.GetValue(pair.Key);
                var rhs = pair.Value.Select(e => ((Literal)e).Value).ToList();
                return Expression.Call(
                    Expression.Constant(rhs),
                    (typeof(List<string>)).GetMethod("Contains", new Type[] { typeof(string) }),
                    Expression.Constant(lhs));
            }

            if(expression is BooleanNotExpression)
            {
                var expr = (BooleanNotExpression)expression;
                return Expression.Not(ToExpression(expr.Expression, record));
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
