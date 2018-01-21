﻿using ExpressionParser;
using SqlParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Node = System.Windows.Controls.TreeViewItem;

namespace ComparisonExpressionVisualizer
{
    public  class ComparisionExpression
    {
        IList<object> _prefix = null;
        public IDictionary<string,string> RecordDictionary { get; }
        public ComparisionExpression(string whereClause,string record)
        {
            var parser = new WhereClauseParser(whereClause);
            var expr = parser.ToExpression(record);
            _prefix = new ExpressionParser.ExpressionParser(expr).PrefixExpression.ToList();
            RecordDictionary = parser.RecordDictionary;
        }
        private void SetColor(Node node, bool? result)
        {
            if (result == null)
            {
                throw new Exception("Invalid Data");
            }
            if (result.Value)
            {
                node.Background = Brushes.LightGreen;

            }
            else
            {
                node.Background = Brushes.LightPink;
            }
        }

        public void Draw( TreeView tree)
        {
            var prefixReverse =  _prefix.Reverse().ToList();
            var stack = new Stack<Node>(prefixReverse.Count);
            Node newNode;
            foreach (var node in prefixReverse)
            {
                if (node is BinaryOperator)
                {
                    Node childNode1 = stack.Pop();
                    Node childNode2 = stack.Pop();

                    var op = (BinaryOperator)node;
                    newNode = new Node() { Header = op.Data };
                    newNode.Items.Add(childNode1);
                    newNode.Items.Add(childNode2);
                    SetColor(newNode, op.Result);
                    stack.Push(newNode);
                }
                else if (node is UnaryOperator)
                {
                    var op = (UnaryOperator)node;
                    Node childNode = stack.Pop();
                    newNode = new Node() { Header = op.Data };
                    newNode.Items.Add(childNode);
                    SetColor(newNode, op.Result);
                    stack.Push(newNode);
                }
                else
                {
                    if (node is ColumnOperand)
                    {
                        var operand = (ColumnOperand)node;
                        newNode = new Node() { Header = $"{operand.Data.Key}[{operand.Data.Value}]" };
                        stack.Push(newNode);
                    }
                    if (node is ConstantOperand)
                    {
                        var operand = (ConstantOperand)node;
                        newNode = new Node() { Header = operand.Data };
                        stack.Push(newNode);

                    }
                    if (node is ConstantOperandOfList)
                    {
                        var operand = (ConstantOperandOfList)node;
                        newNode = new Node() { Header = $"({string.Join(",", operand.Data)})" };
                        stack.Push(newNode);
                    }
                }

            }

            var root = stack.Pop();

            if (stack.Count != 0)
                throw new ArgumentException($"{nameof(_prefix)} prefix expression is invalid. Can't create tree.");

            tree.Items.Add(root);
        }
       
    }
}