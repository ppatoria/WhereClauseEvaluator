using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryTree
{

    class Node
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

    class Stack1
    {
        private Node[] a;
        private int top, m;

        public Stack1(int max)
        {
            m = max;
            a = new Node[m];
            top = -1;
        }

        public void push(Node key)
        {
            a[++top] = key;
        }

        public Node pop()
        {
            return (a[top--]);
        }

        public bool isEmpty()
        {
            return (top == -1);
        }
    }

    class Stack2
    {
        private char[] a;
        private int top, m;

        public Stack2(int max)
        {
            m = max;
            a = new char[m];
            top = -1;
        }

        public void push(char key)
        {
            a[++top] = key;
        }

        public char pop()
        {
            return (a[top--]);
        }

        public bool isEmpty()
        {
            return (top == -1);
        }
    }

    class Conversion
    {
        private Stack2 s;
        private String input;
        private String output = "";

        public Conversion(String str)
        {
            input = str;
            s = new Stack2(str.Length);
        }

        public String inToPost()
        {
            for (int i = 0; i < input.Length; i++)
            {
                char ch = input.ElementAt(i);
                switch (ch)
                {
                    case '+':
                    case '-':
                        gotOperator(ch, 1);
                        break;
                    case '*':
                    case '/':
                        gotOperator(ch, 2);
                        break;
                    case '(':
                        s.push(ch);
                        break;
                    case ')':
                        gotParenthesis();
                        break;
                    default:
                        output = output + ch;
                        break;
                }
            }
            while (!s.isEmpty())
                output = output + s.pop();
            return output;
        }

        private void gotOperator(char opThis, int prec1)
        {
            while (!s.isEmpty())
            {
                char opTop = s.pop();
                if (opTop == '(')
                {
                    s.push(opTop);
                    break;
                }
                else
                {
                    int prec2;
                    if (opTop == '+' || opTop == '-')
                        prec2 = 1;
                    else
                        prec2 = 2;
                    if (prec2 < prec1)
                    {
                        s.push(opTop);
                        break;
                    }
                    else
                        output = output + opTop;
                }
            }
            s.push(opThis);
        }

        private void gotParenthesis()
        {
            while (!s.isEmpty())
            {
                char ch = s.pop();
                if (ch == '(')
                    break;
                else
                    output = output + ch;
            }
        }
    }

    class Tree
    {
        private Node root;

        public Tree()
        {
            root = null;
        }

        //public void insert(String s)
        //{
        //    Conversion c = new Conversion(s);
        //    s = c.inToPost();
        //    Stack1 stk = new Stack1(s.Length);
        //    s = s + "#";
        //    int i = 0;

        //    char symbol = s.ElementAt(i);

        //    Node newNode;
        //    while (symbol != '#')
        //    {
        //        if (symbol >= '0' && symbol <= '9' || symbol >= 'A'
        //                && symbol <= 'Z' || symbol >= 'a' && symbol <= 'z')
        //        {
        //            newNode = new Node(symbol);
        //            stk.push(newNode);
        //        }
        //        else if (symbol == '+' || symbol == '-' || symbol == '/'
        //              || symbol == '*')
        //        {
        //            Node ptr1 = stk.pop();
        //            Node ptr2 = stk.pop();
        //            newNode = new Node(symbol);
        //            newNode.leftChild = ptr2;
        //            newNode.rightChild = ptr1;
        //            stk.push(newNode);
        //        }
        //        symbol = s.ElementAt(++i);
        //    }
        //    root = stk.pop();
        //}
        // or and c=1 c=2 
        //[TestCase("((c in (1,2) or c2=2) and (c=1 or c like '0')) or c1=0", ExpectedResult = true)]
        // or and or c=1 c=2 or c=3 c=4 c=5
        // a c=5 or
        // aa and c=5 or
        // aaa or and c=5 or
        // c=1 c=2 or c=3 c=4 or and c=5 or

        // c3
        // c4
        // c5

        // (c3) or (c4)
        // c5

        // c=1
        // c=2
        
        // (c1) or (c2)
        // (c3) or (c4)
        // c5

        // (c1) or (c2) and (c3) or (c4)
        // c5
        
        // ((c1) or (c2) and (c3) or (c4)) or c5
        
        /* or
         * ----and
         * --------or
         * --------c1
         * --------c2
         * --------or
         * --------c3
         * --------c4
         * ----c5
         * 
         * 
         * */

        

        // (or(and ((or (c1) (c2))(or (c3) (c4)))(c5)  
        


        private Node CreateBTree(List<String> prefix)
        {
            prefix.Reverse();
            Stack1 stk = new Stack1(prefix.Count);
            Node newNode;
            foreach(var symbol in prefix)
            {
                if (symbol == "AND" || symbol == "OR" || symbol == "NOT")
                {
                    Node ptr1 = stk.pop();
                    Node ptr2 = stk.pop();
                    newNode = new Node(symbol);
                    newNode.leftChild = ptr2;
                    newNode.rightChild = ptr1;
                    stk.push(newNode);
                }
                else
                {
                    newNode = new Node(symbol);
                    stk.push(newNode);
                }
                
            }
            return stk.pop();
        }
        public void insert(List<string> prefix)
        {
            root = CreateBTree(prefix);
        }

        //public void insert(String s)
        //{
        //    Conversion c = new Conversion(s);
        //    s = c.inToPost().Trim();
        //    Stack1 stk = new Stack1(s.Length);
        //    s = s + "#";
        //    int i = 0;
           
        //    char symbol = s.ElementAt(i);
            
        //    Node newNode;
        //    while (symbol != '#')
        //    {
                
        //        if (s.StartsWith("AND") || s.StartsWith("OR") || s.StartsWith("NOT"))
        //        {
        //            Node ptr1 = stk.pop();
        //            Node ptr2 = stk.pop();
        //            newNode = new Node(symbol);
        //            newNode.leftChild = ptr2;
        //            newNode.rightChild = ptr1;
        //            stk.push(newNode);
        //        }
        //        else
        //        {
        //            newNode = new Node(symbol);
        //            stk.push(newNode);
        //        }
        //        symbol = s.ElementAt(++i);
        //    }
        //    root = stk.pop();
        //}


        public void traverse(int type)
        {
            switch (type)
            {
                case 1:
                    Console.WriteLine("Preorder Traversal:-    ");
                    preOrder(root);
                    break;
                case 2:
                     Console.WriteLine("Inorder Traversal:-     ");
                    inOrder(root);
                    break;
                case 3:
                    Console.WriteLine("Postorder Traversal:-   ");
                    postOrder(root);
                    break;
                default:
                    Console.WriteLine("Invalid Choice");
                    break;
            }
        }

        private void preOrder(Node localRoot)
        {
            if (localRoot != null)
            {
                localRoot.displayNode();
                preOrder(localRoot.leftChild);
                preOrder(localRoot.rightChild);
            }
        }

        private void inOrder(Node localRoot)
        {
            if (localRoot != null)
            {
                inOrder(localRoot.leftChild);
                localRoot.displayNode();
                inOrder(localRoot.rightChild);
            }
        }

        private void postOrder(Node localRoot)
        {
            if (localRoot != null)
            {
                postOrder(localRoot.leftChild);
                postOrder(localRoot.rightChild);
                localRoot.displayNode();
            }
        }
    }

    public class Prefix_Expression_Tree
    {
        //public static void Main() 
        //{
        //    String ch = "y";
        //    //DataInputStream inp = new DataInputStream(System.in);
        //while (ch.Equals("y"))
        //{
        //    Tree t1 = new Tree();
        //Console.WriteLine("Enter the Expression");
        //String a = Console.ReadLine();
        //t1.insert(a);
        //    t1.traverse(1);
        //    Console.WriteLine("");
        //t1.traverse(2);
        //    Console.WriteLine("");
        //t1.traverse(3);
        //    Console.WriteLine("");
        //Console.WriteLine("Enter y to continue ");
        //ch = Console.ReadLine();
        //}
        public static void Main()
        {
            Tree t1 = new Tree();
            var prefix = new List<string>();
            prefix.Add("OR");
            prefix.Add("AND");
            prefix.Add("OR");
            prefix.Add("c=1");
            prefix.Add("c=2");
            prefix.Add("OR");
            prefix.Add("c=3");
            prefix.Add("c=4");
            prefix.Add("c=5");

            t1.insert(prefix);
            t1.traverse(1);
            t1.traverse(2);
            t1.traverse(3);

        }
    }
    public class Class1
    {
    }
}
