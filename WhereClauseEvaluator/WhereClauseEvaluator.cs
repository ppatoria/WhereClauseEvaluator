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
        public string LastResult { get { return sb_.ToString(); } }
        StringBuilder sb_ = new StringBuilder();
        Node<string> node_ = null;
        public Node<string> BinaryTree { get; }
        private static int level_ = 0;
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
            node_ = new Node<string>();
        }

        public Expression ToExpression<T>(T record) where T:IRecord
        {
            return ToExpression(expression_ ,record);
        }

        private Expression ToExpression<T>(BooleanExpression expression, T record ) where T:IRecord
        {
            if (expression is BooleanBinaryExpression)
            {
                var expr = (BooleanBinaryExpression)expression;
                switch (expr.BinaryExpressionType)
                {
                    case BooleanBinaryExpressionType.And:
                        sb_.Append("AND\n".PadLeft(level_++ * 4));                        
                        return Expression.And(
                            ToExpression(expr.FirstExpression, record),
                            ToExpression(expr.SecondExpression, record));
                    case BooleanBinaryExpressionType.Or:
                        sb_.Append("OR\n");
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
                        sb_.Append($"{lhs}={rhs}\n");
                        return Expression.Equal(
                            Expression.Constant(lhs),
                            Expression.Constant(rhs));
                    case BooleanComparisonType.NotEqualToExclamation:
                        sb_.Append($"{lhs}!={rhs}\n");
                        return Expression.NotEqual(
                            Expression.Constant(lhs),
                            Expression.Constant(rhs));
                    case BooleanComparisonType.GreaterThan:
                        sb_.Append($"{lhs}>{rhs}\n");
                        return Expression.GreaterThan(
                            Expression.Constant(lhs),
                            Expression.Constant(rhs));
                    case BooleanComparisonType.LessThan:
                        sb_.Append($"{lhs}<{rhs}\n");
                        return Expression.LessThan(
                            Expression.Constant(lhs),
                            Expression.Constant(rhs));
                    case BooleanComparisonType.GreaterThanOrEqualTo:
                        sb_.Append($"{lhs}>={rhs}\n");
                        return Expression.GreaterThanOrEqual(
                            Expression.Constant(lhs),
                            Expression.Constant(rhs));
                    case BooleanComparisonType.LessThanOrEqualTo:
                        sb_.Append($"{lhs}<={rhs}\n");
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
                //sb_.Append($"{expr.ScriptTokenStream[0].Text}\n");
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
                sb_.Append($"{lhs} LIKE {rhs}\n");
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
                sb_.Append($"{lhs} IN({string.Join(",", rhs)})\n");
                return Expression.Call(
                    Expression.Constant(rhs),
                    (typeof(List<string>)).GetMethod("Contains", new Type[] { typeof(string) }),
                    Expression.Constant(lhs));
            }

            if(expression is BooleanNotExpression)
            {
                var expr = (BooleanNotExpression)expression;
                sb_.Append("NOT\n");
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
