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
        public string Data;
        public Node Left;
        public Node Right;
        public int Level;
        public Node(string x)
        {
            Data = x;
        }

        public void PrintData()
        {
            Console.WriteLine(Data.PadLeft(Data.Length + Level, '-'));
        }
    }
    public static class BinaryTree
    {

        public static Node CreateBTree(this List<string> prefix)
        {
            prefix.Reverse();
            var stk = new Stack<Node>(prefix.Count);
            Node newNode;
            foreach (var symbol in prefix)
            {
                if (symbol.StartsWith("AND") || symbol.StartsWith("OR") || symbol.StartsWith("NOT"))
                {
                    Node ptr1 = stk.Pop();
                    Node ptr2 = stk.Pop();
                    newNode = new Node(symbol);
                    newNode.Left = ptr2;
                    newNode.Right = ptr1;
                    stk.Push(newNode);
                }
                else
                {
                    newNode = new Node(symbol);
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

        public static void PreOrder(this Node localRoot)
        {
            if (localRoot != null)
            {
                localRoot.PrintData();
                PreOrder(localRoot.Left);
                PreOrder(localRoot.Right);
            }
        }

        public static void InOrder(this Node localRoot)
        {
            if (localRoot != null)
            {
                InOrder(localRoot.Left);
                localRoot.PrintData();
                InOrder(localRoot.Right);
            }
        }

        public static void PostOrder(this Node localRoot)
        {
            if (localRoot != null)
            {
                PostOrder(localRoot.Left);
                PostOrder(localRoot.Right);
                localRoot.PrintData();
            }
        }

        public static void Print(this Node root)
        {
            PreOrder(root);
        }

        public static void AsString(this Node root, ref string treeView)
        {
            if (root != null)
            {
                treeView = treeView + $"{root.Data.PadLeft(root.Data.Length + root.Level, '-')}\n";
                AsString(root.Left, ref treeView);
                AsString(root.Right, ref treeView);
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
            var parser = new WhereClauseParser("((c in (1,2) or c2=2) and (c=1 or c like '0')) or c1=0");
            var expr = parser.ToExpression(mockRecord.Object);
            Console.WriteLine(Expression.Lambda<Func<bool>>(expr).Compile()());
            string treeView = "";
            parser
                .LastResult
                .CreateBTree()
                .AsString(ref treeView);
            Console.WriteLine(treeView);
        }
    }
}
