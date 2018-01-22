using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ComparisonExpressionVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {        
        private ComparisionExpression _comparisionExpression;

        public MainWindow()
        {
            InitializeComponent();
            Closing += MainWindow_Closing;
            whereClauseTextBox.Text = History<string>.Load(whereClauseTextBox.Name);
            recordTextBox.Text = History<string>.Load(recordTextBox.Name);
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            History<string>.Save(whereClauseTextBox.Name, whereClauseTextBox.Text);
            History<string>.Save(recordTextBox.Name, recordTextBox.Text);            
        }

        private void drawTreeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _comparisionExpression = new ComparisionExpression(
                whereClauseTextBox.Text.Trim(),
                recordTextBox.Text.Trim());

                _comparisionExpression.Draw(expressionTreeView);

                recordKeyValueTable.ItemsSource = _comparisionExpression.ObservableRecordDictionary;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void UpdateRecordButton_Click(object sender, RoutedEventArgs e)
        {
            var updatedRecord = string.Join(
                " ",
                _comparisionExpression
                    .ObservableRecordDictionary
                    .Select(kv => $"{kv.Key}={kv.Value}"));

            recordTextBox.Text = updatedRecord;
        }
    }
}
