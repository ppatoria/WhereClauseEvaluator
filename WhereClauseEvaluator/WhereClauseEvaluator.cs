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
    public class Node<T>
    {
        public T Data;
        public Node<T> Right;
        public Node<T> Left;
        public int Level = 0;
    }

    public class WhereClauseParser
    {
        public List<string> LastResult { get; }
        private BooleanExpression expression_;
        public WhereClauseParser(string whereClause)
        {
            IList<ParseError> errors = null;
            expression_ = new TSql100Parser(false)
                .ParseBooleanExpression(new StringReader(whereClause),
                out errors);
            if(errors != null && errors.Any())
            {
                throw new Exception($"Error {errors} while parsing");
            }
            LastResult = new List<string>();
        }

        public Expression ToExpression<T>(T record) where T:IRecord
        {
            return ToExpression(expression_ ,record);
        }

        private bool Evaluate(Expression expr)
        {
            return Expression.Lambda<Func<bool>>(expr).Compile()();
        }

        private Expression ToExpression<T>(BooleanExpression expression, T record ) where T:IRecord
        {
            if (expression is BooleanBinaryExpression)
            {
                var expr = (BooleanBinaryExpression)expression;
                switch (expr.BinaryExpressionType)
                {
                    case BooleanBinaryExpressionType.And:
                        LastResult.Add("AND");                        
                        return Expression.And(
                            ToExpression(expr.FirstExpression, record),
                            ToExpression(expr.SecondExpression, record));
                    case BooleanBinaryExpressionType.Or:
                        LastResult.Add("OR");
                        return Expression.Or(
                            ToExpression<T>(expr.FirstExpression, record),
                            ToExpression(expr.SecondExpression, record));
                }
            }

            if(expression is BooleanComparisonExpression)
            {
                Expression resultExpr;
                var expr = (BooleanComparisonExpression)expression;
                var pair = GetKeyValue(expr);
                var lhs = new Field (pair.Key,record.GetValue(pair.Key));
                var rhs = new Field (pair.Value);
                switch(expr.ComparisonType)
                {
                    case BooleanComparisonType.Equals:
                        resultExpr  =  Expression.Equal(
                            Expression.Constant(lhs),
                            Expression.Constant(rhs));
                        var result = string.Format($"{lhs}={rhs} => [{Evaluate(resultExpr)}]");
                        LastResult.Add(result);
                        return resultExpr;
                    case BooleanComparisonType.NotEqualToExclamation:
                        resultExpr =  Expression.NotEqual(
                            Expression.Constant(lhs),
                            Expression.Constant(rhs));
                        LastResult.Add($"{lhs}!={rhs} => [{Expression.Lambda<Func<bool>>(resultExpr).Compile()()}]");
                        return resultExpr;
                    case BooleanComparisonType.GreaterThan:
                        resultExpr = Expression.GreaterThan(
                            Expression.Constant(lhs),
                            Expression.Constant(rhs));
                        LastResult.Add($"{lhs}>{rhs} => [{Expression.Lambda<Func<bool>>(resultExpr).Compile()()}]");
                        return resultExpr;
                    case BooleanComparisonType.LessThan:
                        resultExpr = Expression.LessThan(
                            Expression.Constant(lhs),
                            Expression.Constant(rhs));
                        LastResult.Add($"{lhs}<{rhs} => [{Expression.Lambda<Func<bool>>(resultExpr).Compile()()}]");
                        return resultExpr;
                    case BooleanComparisonType.GreaterThanOrEqualTo:
                        resultExpr = Expression.GreaterThanOrEqual(
                            Expression.Constant(lhs),
                            Expression.Constant(rhs));
                        LastResult.Add($"{lhs}>={rhs} => [{Expression.Lambda<Func<bool>>(resultExpr).Compile()()}]");
                        return resultExpr;
                    case BooleanComparisonType.LessThanOrEqualTo:
                        resultExpr = Expression.LessThanOrEqual(
                            Expression.Constant(lhs),
                            Expression.Constant(rhs));
                        LastResult.Add($"{lhs}<={rhs} => [{Expression.Lambda<Func<bool>>(resultExpr).Compile()()}]");
                        return resultExpr;
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
                var lhs = new Field (pair.Key,record.GetValue(pair.Key));
                var rhs = new Field (pair.Value);
                rhs = rhs.Value.Replace("%", ".*");
                var re = new Regex(rhs);
                var resultExpr = Expression.Call(
                    Expression.Constant(re),
                    (typeof(Regex)).GetMethod("IsMatch", new Type[] { typeof(string) }),
                    Expression.Constant(lhs));
                LastResult.Add($"{lhs} LIKE {rhs} => [{Expression.Lambda<Func<bool>>(resultExpr).Compile()()}]");
                return resultExpr;
            }

            if(expression is InPredicate)
            {
                var expr = (InPredicate)expression;
                var pair = GetKeyValue(expr);
                var lhs = new Field (pair.Key,record.GetValue(pair.Key));
                var rhs = pair.Value.Select(e => ((Literal)e).Value).ToList();
                var resultExpr = Expression.Call(
                    Expression.Constant(rhs),
                    (typeof(List<string>)).GetMethod("Contains", new Type[] { typeof(string) }),
                    Expression.Constant(lhs));
                LastResult.Add($"{lhs} IN({string.Join(",", rhs)}) => [{Expression.Lambda<Func<bool>>(resultExpr).Compile()()}]");
                return resultExpr;
            }

            if(expression is BooleanNotExpression)
            {
                var expr = (BooleanNotExpression)expression;
                LastResult.Add("NOT");
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
