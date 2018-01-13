using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ExpressionParser
{
    public class ExpressionParser : ExpressionVisitor
    {
        public List<string> PrefixExpression { get; }

        public ExpressionParser(Expression expression)
        {
            PrefixExpression = new List<string>();
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
                PrefixExpression.Add(node.NodeType.ToString());
                Parse(bnode.Left);
                Parse(bnode.Right);
            }
            if(node is ConditionalExpression)
            {
                var cnode = (ConditionalExpression)node;
                PrefixExpression.Add(node.NodeType.ToString());
            }
            if(node is ConstantExpression)
            {
                var cnode = (ConstantExpression)node;
                PrefixExpression.Add(node.ToString());
            }
            if(node is MethodCallExpression)
            {
                var mnode = (MethodCallExpression)node;
                PrefixExpression.Add(mnode.Method.Name);

                var arguments = string.Join(",", mnode.Arguments.Select(a => a.ToString()));
                PrefixExpression.Add(arguments);

                var Object = ParseObject(((ConstantExpression)mnode.Object).Value);
                
                PrefixExpression.Add(Object);
            }
        }

        //public override Expression Visit(Expression node)
        //{
        //    PrefixExpression.Add(node.NodeType.ToString());
        //    return Visit(node);
        //}

        protected override Expression VisitBinary(BinaryExpression node)
        {
            PrefixExpression.Add(node.NodeType.ToString());
            Visit(node.Left);
            Visit(node.Right);
            return null;
        }
        protected override Expression VisitBlock(BlockExpression node)
        {
            PrefixExpression.Add(node.NodeType.ToString());
            return Visit(node);
        }
        protected override Expression VisitConditional(ConditionalExpression node)
        {
            PrefixExpression.Add(node.NodeType.ToString());
            return Visit(node);
        }
        protected override Expression VisitConstant(ConstantExpression node)
        {
            PrefixExpression.Add(node.NodeType.ToString());
            return Visit(node);
        }
        protected override Expression VisitDebugInfo(DebugInfoExpression node)
        {
            PrefixExpression.Add(node.NodeType.ToString());
            return Visit(node);
        }
        protected override Expression VisitDefault(DefaultExpression node)
        {
            PrefixExpression.Add(node.NodeType.ToString());
            return Visit(node);
        }
        protected override Expression VisitDynamic(DynamicExpression node)
        {
            PrefixExpression.Add(node.NodeType.ToString());
            return Visit(node);
        }
        protected override Expression VisitExtension(Expression node)
        {
            PrefixExpression.Add(node.NodeType.ToString());
            return Visit(node);
        }
        protected override Expression VisitGoto(GotoExpression node)
        {
            PrefixExpression.Add(node.NodeType.ToString());
            return Visit(node);
        }
        protected override Expression VisitIndex(IndexExpression node)
        {
            PrefixExpression.Add(node.NodeType.ToString());
            return Visit(node);
        }
        protected override Expression VisitInvocation(InvocationExpression node)
        {
            PrefixExpression.Add(node.NodeType.ToString());
            return Visit(node);
        }
        protected override Expression VisitLabel(LabelExpression node)
        {
            PrefixExpression.Add(node.NodeType.ToString());
            return Visit(node);
        }
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            PrefixExpression.Add(node.NodeType.ToString());
            return Visit(node);
        }
        protected override Expression VisitListInit(ListInitExpression node)
        {
            PrefixExpression.Add(node.NodeType.ToString());
            return Visit(node);
        }
        protected override Expression VisitLoop(LoopExpression node)
        {
            PrefixExpression.Add(node.NodeType.ToString());
            return Visit(node);
        }
        protected override Expression VisitMember(MemberExpression node)
        {
            PrefixExpression.Add(node.NodeType.ToString());
            return Visit(node);
        }
        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            PrefixExpression.Add(node.NodeType.ToString());
            return Visit(node);
        }
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            PrefixExpression.Add(node.NodeType.ToString());
            return Visit(node);
        }
        protected override Expression VisitNew(NewExpression node)
        {
            PrefixExpression.Add(node.NodeType.ToString());
            return Visit(node);
        }
        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            PrefixExpression.Add(node.NodeType.ToString());
            return Visit(node);
        }
        protected override Expression VisitParameter(ParameterExpression node)
        {
            PrefixExpression.Add(node.NodeType.ToString());
            return Visit(node);
        }
        protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
        {
            PrefixExpression.Add(node.NodeType.ToString());
            return Visit(node);
        }
        protected override Expression VisitSwitch(SwitchExpression node)
        {
            PrefixExpression.Add(node.NodeType.ToString());
            return Visit(node);
        }
        protected override Expression VisitTry(TryExpression node)
        {
            PrefixExpression.Add(node.NodeType.ToString());
            return Visit(node);
        }
        protected override Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            PrefixExpression.Add(node.NodeType.ToString());
            return Visit(node);
        }
        protected override Expression VisitUnary(UnaryExpression node)
        {
            PrefixExpression.Add(node.NodeType.ToString());
            return Visit(node);
        }

    }
}
