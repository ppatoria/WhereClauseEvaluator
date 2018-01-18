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
        public void CreateBTree(List<object> prefix)
        {
            prefix.Reverse();
            var stk = new Stack<TreeNode>(prefix.Count);
            TreeNode newNode;
            foreach (var node in prefix)
            {
                if (node is BinaryOperator)
                {
                    TreeNode ptr1 = stk.Pop();
                    TreeNode ptr2 = stk.Pop();
                    newNode = new TreeNode(((BinaryOperator)node).Data);
                    newNode.Nodes.Add(ptr1);
                    newNode.Nodes.Add(ptr2);
                    stk.Push(newNode);
                }
                else if (node is UnaryOperator)
                {
                    TreeNode ptr = stk.Pop();
                    newNode = new TreeNode(((UnaryOperator)node).Data);
                    newNode.Nodes.Add(ptr);
                    stk.Push(newNode);
                }
                else
                {
                    newNode = new TreeNode(node.ToString());
                    stk.Push(newNode);
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
