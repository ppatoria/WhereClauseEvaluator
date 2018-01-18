using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Moq;
using SqlUtil;

namespace BinaryTree
{
    public class Node
    {
        public object Data;
        public Node Left;
        public Node Right;
        public int Level;
        public Node(object x)
        {
            Data = x;
        }

    }
    public static class BinaryTree
    {

        public static Node CreateBTree(this List<object> prefix)
        {
            prefix.Reverse();
            var stk = new Stack<Node>(prefix.Count);
            Node newNode;
            foreach (var node in prefix)
            {
                if(node is BinaryOperator)
                {
                    Node ptr1 = stk.Pop();
                    Node ptr2 = stk.Pop();
                    newNode = new Node(node);
                    newNode.Left = ptr1;
                    newNode.Right = ptr2;
                    stk.Push(newNode);
                }
                else if(node is UnaryOperator)
                {
                    Node ptr = stk.Pop();
                    newNode = new Node(node);
                    newNode.Left = null;
                    newNode.Right = ptr;
                    stk.Push(newNode);
                }
                else
                {
                    newNode = new Node(node);
                    stk.Push(newNode);
                }

            }
            var root = stk.Pop();
            SetLevel(root);
            return root;
        }

        private static void SetLevel(Node root)
        {
            if (root == null) return;

            var q = new Queue<Node>();
            root.Level = 1;
            q.Enqueue(root);

            while (q.Count > 0 )
            {
                var node = q.Dequeue();
                if (node.Left != null)
                {
                    node.Left.Level = node.Level + 1;
                    q.Enqueue(node.Left);
                }
                if (node.Right != null)
                {
                    node.Right.Level = node.Level + 1;
                    q.Enqueue(node.Right);
                }
            }
        }
    }

    public static class Traverse
    {
        static readonly IDictionary<int, char> padCharMap = new Dictionary<int, char>();
        static readonly IDictionary<int, ConsoleColor> printColor = new Dictionary<int, ConsoleColor>();
        static Traverse()
        {
            padCharMap.Add(1, '*');
            padCharMap.Add(2, '_');
            padCharMap.Add(3, '=');
            padCharMap.Add(4, '-');
            padCharMap.Add(5, '.');
            padCharMap.Add(6, '~');
            padCharMap.Add(7, '`');


        }
        public static void PreOrder(this Node localRoot)
        {
            if (localRoot != null)
            {
                localRoot.Data.ToString();
                PreOrder(localRoot.Left);
                PreOrder(localRoot.Right);
            }
        }

        public static void InOrder(this Node localRoot)
        {
            if (localRoot != null)
            {
                InOrder(localRoot.Left);
                localRoot.Data.ToString();
                InOrder(localRoot.Right);
            }
        }

        public static void PostOrder(this Node localRoot)
        {
            if (localRoot != null)
            {
                PostOrder(localRoot.Left);
                PostOrder(localRoot.Right);
                localRoot.Data.ToString();
            }
        }

        private static void PrintColor(string text, bool? isSuccessful = null)
        {
            if(isSuccessful == null)
            {
                    Console.WriteLine(text);
                return;

            }

            if(isSuccessful.Value)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine(text);
                    Console.ResetColor();

            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine(text);
                    Console.ResetColor();
            }
        }

        public static void PrintMin(this Node node, int align = 12)
        {
            if (node != null)
            {
                string result = string.Empty;
                if (node.Data is BinaryOperator)
                {
                    var bnode = (BinaryOperator)node.Data;
                    result = $"{bnode.Data}\n";
                    var treeView = result.PadLeft(result.Length + (node.Level * align), '-');
                    PrintColor(treeView, bnode.Result);
                }

                if (node.Data is UnaryOperator)
                {
                    var unode = (UnaryOperator)node.Data;
                    result = $"{unode.Data}\n";
                    var treeView = result.PadLeft(result.Length + (node.Level * align), '-');
                    PrintColor(treeView, unode.Result);
                }
                if (node.Data is ColumnOperand)
                {
                    var operand = (ColumnOperand)node.Data;
                    result = $"{operand.Data.Key}[{operand.Data.Value}]\n";
                    var treeView = result.PadLeft(result.Length + (node.Level * align), ' ');
                    PrintColor(treeView);
                }
                if (node.Data is ConstantOperand)
                {
                    var operand = (ConstantOperand)node.Data;
                    result = $"{operand.Data}\n";
                    var treeView = result.PadLeft(result.Length + (node.Level * align), ' ');
                    PrintColor(treeView);
                }

                if (node.Data is ConstantOperandOfList)
                {
                    var operand = (ConstantOperandOfList)node.Data;
                    result = $"({string.Join(",", operand.Data)})\n";
                    var treeView = result.PadLeft(result.Length + (node.Level * align), ' ');
                    PrintColor(treeView);
                }

                PrintMin(node.Left, align);
                PrintMin(node.Right, align);
            }
        }

        public static void Print(this Node node, int align = 4)
        {            
            if (node != null)
            {
                string result = string.Empty;
                if(node.Data is BinaryOperator)
                {
                    var bnode = (BinaryOperator)node.Data;
                    result = $"{bnode.Data}\n";
                    var treeView = result.PadLeft(result.Length + (node.Level * align), padCharMap[node.Level]);
                    PrintColor(treeView, bnode.Result);
                }

                if(node.Data is UnaryOperator)
                {
                    var unode = (UnaryOperator)node.Data;
                    result = $"{unode.Data}[{unode.Result}]\n";
                    var treeView = result.PadLeft(result.Length + (node.Level * align), padCharMap[node.Level]);
                    PrintColor(treeView, unode.Result);
                }
                if (node.Data is ColumnOperand)
                {
                    var operand = (ColumnOperand)node.Data;
                     result = $"{operand.Data.Key}[{operand.Data.Value}]\n";
                    var treeView = result.PadLeft(result.Length + (node.Level * align), ' ');
                    PrintColor(treeView);
                }
                if (node.Data is ConstantOperand)
                {
                    var operand = (ConstantOperand)node.Data;
                    result = $"{operand.Data}\n";
                    var treeView = result.PadLeft(result.Length + (node.Level * align), ' ');
                    PrintColor(treeView);
                }

                if (node.Data is ConstantOperandOfList)
                {
                    var operand = (ConstantOperandOfList)node.Data;
                    result = $"({string.Join(",", operand.Data)})\n";
                    var treeView = result.PadLeft(result.Length + (node.Level * align), ' ');
                    PrintColor(treeView);
                }

                Print(node.Left, align);
                Print(node.Right, align);
            }
        }

        public static void ToString(this Node node, ref string treeView, int align = 4)
        {            
            if (node != null)
            {
                string result = string.Empty;
                if(node.Data is BinaryOperator)
                {
                    var bnode = (BinaryOperator)node.Data;
                    result = $"{bnode.Data}[{bnode.Result}]\n";
                    treeView += result.PadLeft(result.Length + (node.Level * align), padCharMap[node.Level]);
                }

                if(node.Data is UnaryOperator)
                {
                    var unode = (UnaryOperator)node.Data;
                    result = $"{unode.Data}[{unode.Result}]\n";
                    treeView += result.PadLeft(result.Length + (node.Level * align), padCharMap[node.Level]);
                }
                if(node.Data is ColumnOperand)
                {
                    var operand = (ColumnOperand)node.Data;
                     result = $"{operand.Data.Key}[{operand.Data.Value}]\n";
                    treeView += result.PadLeft(result.Length + (node.Level * align), ' ');
                }

                if (node.Data is ConstantOperand)
                {
                    var operand = (ConstantOperand)node.Data;
                    result = $"{operand.Data}\n";
                    treeView += result.PadLeft(result.Length + (node.Level * align), ' ');
                }

                if (node.Data is ConstantOperandOfList)
                {
                    var operand = (ConstantOperandOfList)node.Data;
                    result = $"({string.Join(",", operand.Data)})\n";
                    treeView += result.PadLeft(result.Length + (node.Level * align), ' ');
                }
                
                ToString(node.Left, ref treeView, align);
                ToString(node.Right, ref treeView, align);
            }
        }
        
        public static void AsString(this Node node, ref string treeView, int align = 1, char pad = '-' )
        {
            if (node != null)
            {
                string result = string.Empty;
                if(node.Data is BinaryOperator)
                {
                    var bnode = (BinaryOperator)node.Data;
                    result = $"{bnode.Data}[{bnode.Result}]\n";
                    treeView += result.PadLeft(result.Length + (node.Level * align), pad);
                }

                if(node.Data is UnaryOperator)
                {
                    var unode = (UnaryOperator)node.Data;
                    result = $"{unode.Data}[{unode.Result}]\n";
                    treeView += result.PadLeft(result.Length + (node.Level * align), pad);
                }
                if(node.Data is ColumnOperand)
                {
                    var operand = (ColumnOperand)node.Data;
                     result = $"{operand.Data.Key}[{operand.Data.Value}]\n";
                    treeView += result.PadLeft(result.Length + (node.Level * align), ' ');
                }

                if (node.Data is ConstantOperand)
                {
                    var operand = (ConstantOperand)node.Data;
                    result = $"{operand.Data}\n";
                    treeView += result.PadLeft(result.Length + (node.Level * align), ' ');
                }

                if (node.Data is ConstantOperandOfList)
                {
                    var operand = (ConstantOperandOfList)node.Data;
                    result = $"({string.Join(",", operand.Data)})\n";
                    treeView += result.PadLeft(result.Length + (node.Level * align), ' ');
                }
                
                AsString(node.Left, ref treeView, align, pad);
                AsString(node.Right, ref treeView, align, pad);
            }
        }
    }

    class PrefixNotation : List<string> { };
    public class Prefix_Expression_Tree
    {
        public static void Main()
        {
            Mock<IRecord> mockRecord = new Mock<IRecord>();
            mockRecord.Setup(r => r.GetValue("c")).Returns("0");
            mockRecord.Setup(r => r.GetValue("c1")).Returns("1");
            mockRecord.Setup(r => r.GetValue("c2")).Returns("2");
            var parser = new WhereClauseParser("((c in (1,2) or c2=2) and (c=1 or c1=1 or c2=2 or c2=5 or c like 0)) or (c1=0 or not c1 in(1,2))");
            //var parser = new WhereClauseParser("not c1 in(1,2)");
            //var parser = new WhereClauseParser("c1 > 1 and c1 >=1");
            //var parser = new WhereClauseParser("c=0 or c1=1 or c2=2");
            var expr = parser.ToExpression(mockRecord.Object);
            Console.WriteLine(Expression.Lambda<Func<bool>>(expr).Compile()());
            string treeView = "";
            new ExpressionParser.ExpressionParser(parser.ToExpression(mockRecord.Object))
                .PrefixExpression
                .CreateBTree()
                .PrintMin(4);
            Console.WriteLine(treeView);
            Debug.WriteLine(treeView);
         }
    }
}
