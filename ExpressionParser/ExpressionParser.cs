using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SqlUtil;
namespace ExpressionParser
{
    public class ExpressionParser : ExpressionVisitor
    {
        public List<object> PrefixExpression { get; }

        public ExpressionParser(Expression expression)
        {
            PrefixExpression = new List<object>();
            Parse(expression);
        }

        private string ParseObject(object obj)
        {
            if(obj is List<string>)
            {
                return string.Join(",", ((List<string>)obj));
            }
            if(obj is Regex)
            {
                return ((Regex)obj).ToString();
            }
            throw new NotImplementedException();
        }

        private void Parse(Expression node)
        {
            if (node is BinaryExpression)
            {
                var bnode = (BinaryExpression)node;
                var op = new BinaryOperator(bnode.NodeType.ToString());
                op.Result = Expression.Lambda<Func<bool>>(bnode).Compile()();

                PrefixExpression.Add(op);
                Parse(bnode.Left);
                Parse(bnode.Right);
            }

            if(node is UnaryExpression)
            {
                var unode = (UnaryExpression)node;
                var op = new UnaryOperator(unode.NodeType.ToString());
                op.Result = Expression.Lambda<Func<bool>>(unode).Compile()();

                PrefixExpression.Add(op);
                Parse(unode.Operand);
            }

            if(node is ConditionalExpression)
            {
                var cnode = (ConditionalExpression)node;
                PrefixExpression.Add( new BinaryOperator(cnode.NodeType.ToString()));
            }
            if(node is ConstantExpression)
            {
                var cnode = (ConstantExpression)node;
                PrefixExpression.Add(cnode.Value);
            }
            if(node is MethodCallExpression)
            {
                var mnode = (MethodCallExpression)node;
                PrefixExpression.Add(new BinaryOperator(mnode.Method.Name));

                foreach(var arg in mnode.Arguments)
                {
                    var argValue = ((ConstantExpression)arg).Value;
                    PrefixExpression.Add(argValue);
                }

                if (mnode.Object != null)
                {
                    var Object = ParseObject(((ConstantExpression)mnode.Object).Value);
                    PrefixExpression.Add(Object);
                }
            }
        }
    }
}
