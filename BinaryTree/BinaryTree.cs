using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryTree
{

    public class Node
    {
        public string data;
        public Node leftChild;
        public Node rightChild;

        public Node(string x)
        {
            data = x;
        }

        public void displayNode()
        {
            Console.WriteLine(data);
        }
    }
    public static class BinaryTree
    {

        public static Node Create(this List<string> prefix)
        {
            prefix.Reverse();
            var stk = new Stack<Node>(prefix.Count);
            Node newNode;
            foreach (var symbol in prefix)
            {
                if (symbol == "AND" || symbol == "OR" || symbol == "NOT")
                {
                    Node ptr1 = stk.Pop();
                    Node ptr2 = stk.Pop();
                    newNode = new Node(symbol);
                    newNode.leftChild = ptr2;
                    newNode.rightChild = ptr1;
                    stk.Push(newNode);
                }
                else
                {
                    newNode = new Node(symbol);
                    stk.Push(newNode);
                }

            }
            return stk.Pop();
        }
    }

    public static class Traverse
    { 

        public static void PreOrder(this Node localRoot)
        {
            if (localRoot != null)
            {
                localRoot.displayNode();
                PreOrder(localRoot.leftChild);
                PreOrder(localRoot.rightChild);
            }
        }

        public static void InOrder(this Node localRoot)
        {
            if (localRoot != null)
            {
                InOrder(localRoot.leftChild);
                localRoot.displayNode();
                InOrder(localRoot.rightChild);
            }
        }

        public static void PostOrder(this Node localRoot)
        {
            if (localRoot != null)
            {
                PostOrder(localRoot.leftChild);
                PostOrder(localRoot.rightChild);
                localRoot.displayNode();
            }
        }
    }

    public class Prefix_Expression_Tree
    {
        public static void Main()
        {
            var prefix = new List<string>
            {
                "OR",
                "AND",
                "OR",
                "c=1",
                "c=2",
                "OR",
                "c=3",
                "c=4",
                "c=5"
            };
            var root = BinaryTree.Create(prefix);
            root.InOrder();
            root.PreOrder();
            root.PostOrder();
        }
    }
}
