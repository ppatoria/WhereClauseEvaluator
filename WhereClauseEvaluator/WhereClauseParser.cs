using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Lookup;

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

    public class WhereClauseParser
    {
        private BooleanExpression expression_;
        DirectoryCatalog _dirCatalog;

        [Import(typeof(ILookupFactory), AllowRecomposition = true)]
        private ILookupFactory _lookupFactory;


        private void Compose()
        {
            //var path = $@"{Directory.GetCurrentDirectory()}\Plugin";
            var path = @"C:\Users\prashubh\Documents\Visual Studio 2017\Projects\WhereClauseEvaluator\SampleLookup\bin\Debug\Plugin";
             _dirCatalog = new DirectoryCatalog(path);
            CompositionContainer container = new CompositionContainer(_dirCatalog);
            container.SatisfyImportsOnce(this);
            container.ComposeParts(this);
        }

        public WhereClauseParser(string whereClause)
        {            
            expression_ = new TSql100Parser(false)
                .ParseBooleanExpression(new StringReader(whereClause), out IList<ParseError>  errors);
            if(errors != null && errors.Any())
            {
                throw new Exception($"Error {errors} while parsing");
            }
        }
        private bool Evaluate(Expression expr)
        {
            return Expression.Lambda<Func<bool>>(expr).Compile()();
        }
        /// <summary>
        /// Creates Linq Expression for a where clause string.
        /// NOTE: User need to provide the implementation of ILookup and place the assembly in Pluging directory under working directory.
        ///       Else if implemetation is not provided in the directory use overloaded ToExpression(ILookup) method.
        /// </summary>
        /// <returns></returns>
        public Expression ToExpression()
        {
            Compose();
            if (_lookupFactory == null)
            {
                throw new InvalidOperationException(
                    $@"ILookup is null.\n 
                        Either provide the implementation of ILookup and place in the Plugin directory under working directory \n
                        or \n
                        use overloaded ToExpression(ILookup) method instead and pass the required ILoopup implementation");
            }
            var lookupImpl = _lookupFactory.GetLookup();
            return ToExpression(lookupImpl);
        }

        public Expression ToExpression<T>(T lookup) where T : ILookup
        {
            return ToExpression(expression_);

            Expression ToExpression(BooleanExpression expression)
            {
                if (expression is BooleanBinaryExpression)
                {
                    var expr = (BooleanBinaryExpression)expression;
                    switch (expr.BinaryExpressionType)
                    {
                        case BooleanBinaryExpressionType.And:
                            return Expression.And(
                                ToExpression(expr.FirstExpression),
                                ToExpression(expr.SecondExpression));
                        case BooleanBinaryExpressionType.Or:
                            return Expression.Or(
                                ToExpression(expr.FirstExpression),
                                ToExpression(expr.SecondExpression));
                    }
                }

                if (expression is BooleanComparisonExpression)
                {
                    var expr = (BooleanComparisonExpression)expression;
                    var pair = GetKeyValue(expr);
                    var lhs = new ColumnOperand(pair.Key, lookup.GetValue(pair.Key));
                    var rhs = new ConstantOperand(pair.Value);
                    switch (expr.ComparisonType)
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

                if (expression is BooleanParenthesisExpression)
                {
                    var expr = (BooleanParenthesisExpression)expression;
                    return ToExpression(expr.Expression);
                }

                if (expression is LikePredicate)
                {
                    var expr = (LikePredicate)expression;
                    var pair = GetKeyValue(expr);
                    var lhs = new ColumnOperand(pair.Key, lookup.GetValue(pair.Key));
                    var rhs = new ConstantOperand(pair.Value);

                    return Expression.Call(
                        null,
                        typeof(SqlOperations).GetMethod("Like"),
                        Expression.Constant(lhs),
                        Expression.Constant(rhs));
                }

                if (expression is InPredicate)
                {
                    var expr = (InPredicate)expression;
                    var pair = GetKeyValue(expr);
                    var lhs = new ColumnOperand(pair.Key, lookup.GetValue(pair.Key));
                    var rhs = new ConstantOperandOfList(pair.Value.Select(e => ((Literal)e).Value).ToList());

                    return Expression.Call(
                        null,
                        typeof(SqlOperations).GetMethod("In"),
                        Expression.Constant(lhs),
                        Expression.Constant(rhs));
                }

                if (expression is BooleanNotExpression)
                {
                    var expr = (BooleanNotExpression)expression;
                    return Expression.Not(ToExpression(expr.Expression));
                }

                throw new InvalidExpressionException($"{expression} not supported");

            }
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
