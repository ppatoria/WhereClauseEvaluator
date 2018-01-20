namespace WhereClauseVisualizer
{
    partial class WhereClauseVisualizer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.treeView = new System.Windows.Forms.TreeView();
            this.WhereClauseTextBox = new System.Windows.Forms.RichTextBox();
            this.ViewCommand = new System.Windows.Forms.Button();
            this.recordForLookup = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // treeView
            // 
            this.treeView.Location = new System.Drawing.Point(0, 0);
            this.treeView.Name = "treeView";
            this.treeView.Size = new System.Drawing.Size(677, 1090);
            this.treeView.TabIndex = 0;
            this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.ExpressionTree_AfterSelect);
            // 
            // WhereClauseTextBox
            // 
            this.WhereClauseTextBox.Location = new System.Drawing.Point(683, 38);
            this.WhereClauseTextBox.Name = "WhereClauseTextBox";
            this.WhereClauseTextBox.Size = new System.Drawing.Size(1117, 96);
            this.WhereClauseTextBox.TabIndex = 1;
            this.WhereClauseTextBox.Text = "";
            this.WhereClauseTextBox.TextChanged += new System.EventHandler(this.WhereClauseTextBox_TextChanged);
            // 
            // ViewCommand
            // 
            this.ViewCommand.Location = new System.Drawing.Point(961, 306);
            this.ViewCommand.Name = "ViewCommand";
            this.ViewCommand.Size = new System.Drawing.Size(75, 23);
            this.ViewCommand.TabIndex = 2;
            this.ViewCommand.Text = "View";
            this.ViewCommand.UseVisualStyleBackColor = true;
            this.ViewCommand.Click += new System.EventHandler(this.ViewCommand_Click);
            // 
            // recordForLookup
            // 
            this.recordForLookup.Location = new System.Drawing.Point(683, 162);
            this.recordForLookup.Name = "recordForLookup";
            this.recordForLookup.Size = new System.Drawing.Size(1117, 96);
            this.recordForLookup.TabIndex = 3;
            this.recordForLookup.Text = "";
            this.recordForLookup.TextChanged += new System.EventHandler(this.richTextBox1_TextChanged);
            // 
            // WhereClauseVisualizer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1980, 1093);
            this.Controls.Add(this.recordForLookup);
            this.Controls.Add(this.ViewCommand);
            this.Controls.Add(this.WhereClauseTextBox);
            this.Controls.Add(this.treeView);
            this.Name = "WhereClauseVisualizer";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.RichTextBox WhereClauseTextBox;
        private System.Windows.Forms.Button ViewCommand;
        private System.Windows.Forms.RichTextBox recordForLookup;
    }
}

