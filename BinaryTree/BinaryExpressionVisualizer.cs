using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Moq;
using ExpressionParser;
using SqlParser;
using Lookup;

namespace BinaryTree
{
    

    public static class Traverse
    {

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

        public static void Print(this Node node, int align = 4)
        {
            if (node != null)
            {
                string result = string.Empty;
                if (node.Data is BinaryOperator)
                {
                    var bnode = (BinaryOperator)node.Data;
                    result = $"{bnode.Data}";
                    var treeView = result.PadLeft(result.Length + (node.Level * align), '-');
                    PrintColor(treeView, bnode.Result);
                }

                if (node.Data is UnaryOperator)
                {
                    var unode = (UnaryOperator)node.Data;
                    result = $"{unode.Data}";
                    var treeView = result.PadLeft(result.Length + (node.Level * align), '-');
                    PrintColor(treeView, unode.Result);
                }
                if (node.Data is ColumnOperand)
                {
                    var operand = (ColumnOperand)node.Data;
                    result = $"{operand.Data.Key}[{operand.Data.Value}]";
                    var treeView = result.PadLeft(result.Length + (node.Level * align), ' ');
                    PrintColor(treeView);
                }
                if (node.Data is ConstantOperand)
                {
                    var operand = (ConstantOperand)node.Data;
                    result = $"{operand.Data}";
                    var treeView = result.PadLeft(result.Length + (node.Level * align), ' ');
                    PrintColor(treeView);
                }

                if (node.Data is ConstantOperandOfList)
                {
                    var operand = (ConstantOperandOfList)node.Data;
                    result = $"({string.Join(",", operand.Data)})";
                    var treeView = result.PadLeft(result.Length + (node.Level * align), ' ');
                    PrintColor(treeView);
                }

                Print(node.Left, align);
                Print(node.Right, align);
            }
        }

        public static void PrintExperimental (this Node node, int align = 4)
        {            
            if (node != null)
            {
                string result = string.Empty;
                if(node.Data is BinaryOperator)
                {
                    var bnode = (BinaryOperator)node.Data;
                    result = $"{bnode.Data}";
                    var treeView = result.PadLeft(result.Length + (node.Level * align), Padding.GetChar(node.Level));
                    PrintColor(treeView, bnode.Result);
                }

                if(node.Data is UnaryOperator)
                {
                    var unode = (UnaryOperator)node.Data;
                    result = $"{unode.Data}[{unode.Result}]";
                    var treeView = result.PadLeft(result.Length + (node.Level * align), Padding.GetChar(node.Level));
                    PrintColor(treeView, unode.Result);
                }
                if (node.Data is ColumnOperand)
                {
                    var operand = (ColumnOperand)node.Data;
                     result = $"{operand.Data.Key}[{operand.Data.Value}]";
                    var treeView = result.PadLeft(result.Length + (node.Level * align), ' ');
                    PrintColor(treeView);
                }
                if (node.Data is ConstantOperand)
                {
                    var operand = (ConstantOperand)node.Data;
                    result = $"{operand.Data}";
                    var treeView = result.PadLeft(result.Length + (node.Level * align), ' ');
                    PrintColor(treeView);
                }

                if (node.Data is ConstantOperandOfList)
                {
                    var operand = (ConstantOperandOfList)node.Data;
                    result = $"({string.Join(",", operand.Data)})";
                    var treeView = result.PadLeft(result.Length + (node.Level * align), ' ');
                    PrintColor(treeView);
                }

                PrintExperimental(node.Left, align);
                PrintExperimental(node.Right, align);
            }
        }

    }

    class PrefixNotation : List<string> { };
    public class Prefix_Expression_Tree
    {
        public static void Main()
        {
            Mock<ILookup> mockRecord = new Mock<ILookup>();
            mockRecord.Setup(r => r.GetValue("c")).Returns("0");
            mockRecord.Setup(r => r.GetValue("c1")).Returns("1");
            mockRecord.Setup(r => r.GetValue("c2")).Returns("2");
            var parser = new WhereClauseParser("((c in (1,2) or c2=2) and (c=1 or c1=1 or c2=2 or c2=5 or c like 0)) or (c1=0 or not c1 in(1,2))");


            new ExpressionParser.ExpressionParser(parser.ToExpression(mockRecord.Object))
                .PrefixExpression
                .ToTree()
                .Print();


            string treeView = new ExpressionParser.ExpressionParser(parser.ToExpression(mockRecord.Object))
                .PrefixExpression
                .ToTree()
                .AsString(4);

            Console.WriteLine(treeView);
            Debug.WriteLine(treeView);


            new ExpressionParser.ExpressionParser(parser.ToExpression())
                .PrefixExpression
                .ToTree()
                .Print();
         }
    }
}
