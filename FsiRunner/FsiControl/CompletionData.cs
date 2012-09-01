using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

using Microsoft.FSharp.Compiler.Server.Shared;

using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ClearLines.FsiControl
{
    public class CompletionData: ICompletionData
    {
        public CompletionData(string text, CompletionType t)
        {
            this.Text = text;

            BitmapImage image;
            if (icons.TryGetValue(t, out image))
                Image = image;
        }

        public ImageSource Image { get; private set; }

        public string Text { get; private set; }
                    
        public object Content  { get { return this.Text; } }

        public object Description { get { return null; } }

        public double Priority { get { return 1; } }


        public void Complete(TextArea textArea, ISegment completionSegment,
            EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, this.Text);
        }


        private static BitmapImage LoadIcon(string name)
        {
            try
            {
                return new BitmapImage(new Uri("pack://application:,,,/FsiControl;component/Icons/" + name + ".png"));
            }
            catch
            {
                return null;
            }
        }

        private static Dictionary<CompletionType, BitmapImage> icons = new Dictionary<CompletionType, BitmapImage> {
            { CompletionType.ActivePattern, LoadIcon("EnumItem") },
            { CompletionType.Class, LoadIcon("Class") },    
            { CompletionType.Delegate, LoadIcon("Delegate") },
            { CompletionType.Enum, LoadIcon("Enum") },
            { CompletionType.Event, LoadIcon("Event") },
            { CompletionType.Exception, LoadIcon("Exception") },
            { CompletionType.Field, LoadIcon("Field") },
            { CompletionType.Interface, LoadIcon("Interface") },
            { CompletionType.Method, LoadIcon("Method") },
            { CompletionType.Module, LoadIcon("Module") },
            { CompletionType.Namespace, LoadIcon("Namespace") },
            { CompletionType.Property, LoadIcon("Property") },
            { CompletionType.Record, LoadIcon("Record") },
            { CompletionType.Structure, LoadIcon("Structure") },
            { CompletionType.TypeDef, LoadIcon("TypeDefinition") },
            { CompletionType.Union, LoadIcon("Union") },
            { CompletionType.UnionCase, LoadIcon("EnumItem") },
            { CompletionType.Value, LoadIcon("Value") },               
        };
    }
}
