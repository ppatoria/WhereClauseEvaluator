using ICSharpCode.AvalonEdit.CodeCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

    public static class CompletionListExtension
    {
        public static void AddCompletionData(this CompletionList completionList, IList<string> values)
        {
            foreach (var completionData in values.Select(v => new CompletionData(v)))
            {
                completionList.CompletionData.Add(completionData);
            }
        }

    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {        
        private ComparisionExpression _comparisionExpression;

        private History<string> _whereClauseTextHistory = new History<string>();
        private History<string> _recordTextHistory = new History<string>();

        CompletionWindow _completionWindow;

        Preferences _preferences;
        public MainWindow()
        {
            InitializeComponent();

            whereClauseTextBox.TextArea.TextEntering += WhereClause_TextEntering;
            whereClauseTextBox.TextArea.TextEntered += WhereClause_TextEntered;

            recordTextBox.TextArea.TextEntering += Record_TextEntering;
            recordTextBox.TextArea.TextEntered += Record_TextEntered;
            Closing += MainWindow_Closing;
            whereClauseHistoryList.SelectionChanged += WhereClauseHistoryList_SelectionChanged;
            recordHistoryList.SelectionChanged += RecordHistoryList_SelectionChanged;

            var whereClauseHistory = _whereClauseTextHistory.Load(whereClauseTextBox.Name);
            whereClauseHistoryList.ItemsSource = whereClauseHistory;
            
            var recordHistory = _recordTextHistory.Load(recordTextBox.Name);
            recordHistoryList.ItemsSource = recordHistory;

            _preferences = PreferencesSerializer.Deserialize();
        }

        private void Record_TextEntered(object sender, TextCompositionEventArgs e)
        {
        }

        private void Record_TextEntering(object sender, TextCompositionEventArgs e)
        {
        }

       


        private void WhereClause_TextEntered(object sender, TextCompositionEventArgs e)
        {
            _completionWindow = new CompletionWindow(whereClauseTextBox.TextArea);

            if(whereClauseHistoryList.Text.Trim().Length <= 0 
                && _comparisionExpression != null 
                && _comparisionExpression.ObservableRecordDictionary != null)
            {
                    _completionWindow
                        .CompletionList
                        .AddCompletionData(_comparisionExpression.ObservableRecordDictionary.Keys);

            }
            // show keys
            if (e.Text == " ")
            {
                /**
                 * If ends with 
                 * = or < or > or in(   
                 * show values
                 */
                 
                if(Regex.IsMatch(whereClauseTextBox.Text.TrimEnd(), @"$(?<=([=<>])|(in\s*\())"))
                {
                    _completionWindow
                        .CompletionList
                        .AddCompletionData(_comparisionExpression.ObservableRecordDictionary.Values);
                }
                else
                {
                    _completionWindow
                        .CompletionList
                        .AddCompletionData(_comparisionExpression.ObservableRecordDictionary.Keys);
                }
            }
            _completionWindow.Show();
            _completionWindow.Closed += delegate { _completionWindow = null; };
        }

        private void WhereClause_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && _completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    _completionWindow.CompletionList.RequestInsertion(e);
                }
            }
        }

        private void RecordHistoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
                recordTextBox.Text = e.AddedItems[0].ToString();
        }

        private void WhereClauseHistoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           whereClauseTextBox.Text = e.AddedItems[0].ToString();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _whereClauseTextHistory.Save(whereClauseTextBox.Name, whereClauseTextBox.Text);
            _recordTextHistory.Save(recordTextBox.Name, recordTextBox.Text);
            PreferencesSerializer.Serialize(_preferences);
        }

        private void drawTreeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _comparisionExpression = new ComparisionExpression(
                whereClauseTextBox.Text.Trim(),
                recordTextBox.Text.Trim());
                _comparisionExpression.Draw(expressionTreeView, _preferences.ExpandAll);

                recordKeyValueTable.ItemsSource = _comparisionExpression.ObservableRecordDictionary;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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

        private void CopyTreeViewButton_Click(object sender, RoutedEventArgs e)
        {
            if (_comparisionExpression != null)
            {
                Thread thread = new Thread(() => Clipboard.SetText(_comparisionExpression.ToString()));
                thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
                thread.Start();
                thread.Join();
            }
        }

        private void AlwayCollapseTree_Checked(object sender, RoutedEventArgs e)
        {
            if(_preferences != null)
                _preferences.ExpandAll = false;
        }

        private void AlwayCollapseTree_Unchecked(object sender, RoutedEventArgs e)
        {
            if(_preferences != null)
                _preferences.ExpandAll = true;

        }
    }
}
