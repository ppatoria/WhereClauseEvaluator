using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlParser;

namespace ExpressionParser
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

        public static Node ToBTree(this List<object> prefix)
        {
            prefix.Reverse();
            var stk = new Stack<Node>(prefix.Count);
            Node newNode;
            foreach (var node in prefix)
            {
                if (node is BinaryOperator)
                {
                    Node ptr1 = stk.Pop();
                    Node ptr2 = stk.Pop();
                    newNode = new Node(node);
                    newNode.Left = ptr1;
                    newNode.Right = ptr2;
                    stk.Push(newNode);
                }
                else if (node is UnaryOperator)
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

            while (q.Count > 0)
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


    public static class Padding
    {
        static readonly IDictionary<int, char> padCharMap = new Dictionary<int, char>();
        static readonly IDictionary<int, ConsoleColor> printColor = new Dictionary<int, ConsoleColor>();
        static Padding()
        {
            padCharMap.Add(1, '*');
            padCharMap.Add(2, '_');
            padCharMap.Add(3, '=');
            padCharMap.Add(4, '-');
            padCharMap.Add(5, '.');
            padCharMap.Add(6, '~');
            padCharMap.Add(7, '`');
        }

        public static char GetChar(int level)
        {
            if (padCharMap.TryGetValue(level, out char padChar))
                return padChar;
            throw new KeyNotFoundException($"Not padding character found for level {level}.");
        }


    }
    public static class Visitor
    {
        public static void AsStringExperimental(this Node node, ref string treeView, int align = 4)
        {
            if (node != null)
            {
                string result = string.Empty;
                if (node.Data is BinaryOperator)
                {
                    var bnode = (BinaryOperator)node.Data;
                    result = $"{bnode.Data}[{bnode.Result}]\n";
                    treeView += result.PadLeft(result.Length + (node.Level * align), Padding.GetChar(node.Level));
                }

                if (node.Data is UnaryOperator)
                {
                    var unode = (UnaryOperator)node.Data;
                    result = $"{unode.Data}[{unode.Result}]\n";
                    treeView += result.PadLeft(result.Length + (node.Level * align), Padding.GetChar(node.Level));
                }
                if (node.Data is ColumnOperand)
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

                AsStringExperimental(node.Left, ref treeView, align);
                AsStringExperimental(node.Right, ref treeView, align);
            }
        }

        public static string AsString(this Node root, int leftAlign = 4, char pad = '-')
        {
            string view = string.Empty;
            AsString(root, ref view, leftAlign, pad);
            return view;
            void AsString(Node node, ref string treeView, int align = 1, char charPad = '-')
            {
                if (node != null)
                {
                    string result = string.Empty;
                    if (node.Data is BinaryOperator)
                    {
                        var bnode = (BinaryOperator)node.Data;
                        result = $"{bnode.Data}[{bnode.Result}]\n";
                        treeView += result.PadLeft(result.Length + (node.Level * align), charPad);
                    }

                    if (node.Data is UnaryOperator)
                    {
                        var unode = (UnaryOperator)node.Data;
                        result = $"{unode.Data}[{unode.Result}]\n";
                        treeView += result.PadLeft(result.Length + (node.Level * align), charPad);
                    }
                    if (node.Data is ColumnOperand)
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

                    AsString(node.Left, ref treeView, align, charPad);
                    AsString(node.Right, ref treeView, align, charPad);
                }
            }
        }

    }
}
