﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Text.RegularExpressions;
using System.Linq.Expressions;

namespace SqlParser
{
    public static class SqlOperations
    {
        public static bool Like(this ColumnOperand lhs, ConstantOperand rhs)
        {
            return Regex.IsMatch(lhs.Data.Value, (rhs.Data.Replace("%", ".*")));
        }

        public static bool In(this ColumnOperand lhs, ConstantOperandOfList rhs)
        {
            return rhs.Data.Contains(lhs.Data.Value);
        }
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
        }

        public Expression ToExpression<T>(T record) where T:ILookup
        {
            return ToExpression(expression_ ,record);
        }

        private bool Evaluate(Expression expr)
        {
            return Expression.Lambda<Func<bool>>(expr).Compile()();
        }

        private Expression ToExpression<T>(BooleanExpression expression, T record ) where T:ILookup
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
                var lhs = new ColumnOperand (pair.Key,record.GetValue(pair.Key));
                var rhs = new ConstantOperand(pair.Value);
                switch(expr.ComparisonType)
                {
                    case BooleanComparisonType.Equals:
                        return  Expression.Equal(
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
                var lhs = new ColumnOperand (pair.Key,record.GetValue(pair.Key));
                var rhs = new ConstantOperand (pair.Value);

                return Expression.Call(
                    null,
                    typeof(SqlOperations).GetMethod("Like"),
                    Expression.Constant(lhs),
                    Expression.Constant(rhs));
            }

            if(expression is InPredicate)
            {
                var expr = (InPredicate)expression;
                var pair = GetKeyValue(expr);
                var lhs = new ColumnOperand (pair.Key,record.GetValue(pair.Key));
                var rhs = new ConstantOperandOfList(pair.Value.Select(e => ((Literal)e).Value).ToList());

                return Expression.Call(
                    null,
                    typeof(SqlOperations).GetMethod("In"),
                    Expression.Constant(lhs),
                    Expression.Constant(rhs));
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