using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClearLines.FsiControl
{
    public partial class Editor : UserControl
    {
        public Editor()
        {
            InitializeComponent();
        }

        private static char[] separators = { ' ', '\t', '\r', '\n' };
        private CompletionWindow completionWindow;

        // Completion window is shown on Esc, Tabs are replaced with spaces
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            var editor = e.Source as TextEditor;
            if (editor == null) return;

            var closeWindow = false;
            var showNewCompletion = false;
            switch (e.Key)
            {
                case Key.Tab:
                case Key.Space:
                case Key.OemSemicolon:
                case Key.Enter:
                    closeWindow = true;
                    break;
                case Key.OemPeriod:
                    closeWindow = true;
                    showNewCompletion = true;
                    e.Handled = true;
                    break;
                case Key.Escape:
                    showNewCompletion = true;
                    break;
            }

            if (closeWindow && completionWindow != null)
            {
                completionWindow.CompletionList.RequestInsertion(e);
                if(e.Key == Key.OemPeriod)
                    editor.AppendText(".");
            }

            if (showNewCompletion)
            {
                ShowCompletionWindow(editor);
            }
        }

        private void ShowCompletionWindow(TextEditor editor)
        {
            var wordStart = editor.Document.GetText(0, editor.CaretOffset).LastIndexOfAny(separators) + 1;
            var text = editor.Document.GetText(wordStart, editor.CaretOffset - wordStart);
            var completionStartOffset = text.LastIndexOf('.') + wordStart + 1;

            var vm = (EditorViewModel)this.DataContext;
            var completions = vm.Session.Completions(text);
            if (completions.Length == 0)
                return;

            completionWindow = new CompletionWindow(editor.TextArea) { StartOffset = completionStartOffset };
            var data = completionWindow.CompletionList.CompletionData;

            foreach (var completion in completions.OrderBy(c => c.Item2))
            {
                data.Add(new CompletionData(completion.Item2, completion.Item1));
            }

            completionWindow.Show();

            completionWindow.Closed += delegate
            {
                completionWindow = null;
            };
        }
    }
}
