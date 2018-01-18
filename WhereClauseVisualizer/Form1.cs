using ExpressionParser;
using Moq;
using SqlUtil;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhereClauseVisualizer
{



    public partial class WhereClauseVisualizer : Form
    {
        public WhereClauseVisualizer()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void WhereClauseTextBox_TextChanged(object sender, EventArgs e)
        {

        }
        private void SetColor(TreeNode node, bool? result)
        {
            if (result == null)
            {
                throw new Exception("Invalid Data");
            }
            if (result.Value)
            {
                node.BackColor = Color.LightGreen;
                
            }
            else
            {
                node.BackColor = Color.LightPink;
            }
        }
        public void CreateBTree(List<object> prefix)
        {
            prefix.Reverse();
            var stk = new Stack<TreeNode>(prefix.Count);
            TreeNode newNode;
            foreach (var node in prefix)
            {
                if (node is BinaryOperator)
                {
                    TreeNode childNode1 = stk.Pop();                    
                    TreeNode childNode2 = stk.Pop();

                    var op = (BinaryOperator)node;                    
                    newNode = new TreeNode(op.Data);
                    newNode.Nodes.Add(childNode1);
                    newNode.Nodes.Add(childNode2);
                    SetColor(newNode, op.Result);
                    stk.Push(newNode);
                }
                else if (node is UnaryOperator)
                {
                    var op = (UnaryOperator)node;                    
                    TreeNode childNode = stk.Pop();
                    newNode = new TreeNode(op.Data);
                    newNode.Nodes.Add(childNode);
                    SetColor(newNode, op.Result);
                    stk.Push(newNode);
                }
                else
                {
                    if(node is ColumnOperand)
                    {
                        var operand = (ColumnOperand)node;
                        newNode = new TreeNode($"{operand.Data.Key}[{operand.Data.Value}]");
                        stk.Push(newNode);
                    }
                    if(node is ConstantOperand)
                    {
                        var operand = (ConstantOperand)node;
                        newNode = new TreeNode(operand.Data);
                        stk.Push(newNode);

                    }
                    if(node is ConstantOperandOfList)
                    {
                        var operand = (ConstantOperandOfList)node;
                        newNode = new TreeNode($"({string.Join(",",operand.Data)})");
                        stk.Push(newNode);
                    }
                }

            }
            treeView.Nodes.Add(stk.Pop());
        }

        private void ViewCommand_Click(object sender, EventArgs e)
        {
            Mock<IRecord> mockRecord = new Mock<IRecord>();
            mockRecord.Setup(r => r.GetValue("c")).Returns("0");
            mockRecord.Setup(r => r.GetValue("c1")).Returns("1");
            mockRecord.Setup(r => r.GetValue("c2")).Returns("2");

            var whereClause = WhereClauseTextBox.Text.Trim();
            var parser = new WhereClauseParser(whereClause);
            var expr = parser.ToExpression(mockRecord.Object);
            Console.WriteLine(Expression.Lambda<Func<bool>>(expr).Compile()());
            var prefix = new ExpressionParser.ExpressionParser(parser.ToExpression(mockRecord.Object))
                .PrefixExpression;
            CreateBTree(prefix);
        }

        private void ExpressionTree_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }
    }

    public static class BinaryTree
    {

       
    }
}
