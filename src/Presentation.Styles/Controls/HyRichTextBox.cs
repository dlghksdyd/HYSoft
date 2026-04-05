using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace HYSoft.Presentation.Styles.Controls
{
    public class HyRichTextBox : RichTextBox
    {
        static HyRichTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyRichTextBox), new FrameworkPropertyMetadata(typeof(HyRichTextBox)));
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(HyRichTextBox),
                new FrameworkPropertyMetadata(new CornerRadius(4)));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register(
                nameof(Watermark),
                typeof(string),
                typeof(HyRichTextBox),
                new FrameworkPropertyMetadata(null));

        public string Watermark
        {
            get => (string)GetValue(WatermarkProperty);
            set => SetValue(WatermarkProperty, value);
        }

        public static readonly DependencyProperty WatermarkForegroundProperty =
            DependencyProperty.Register(
                nameof(WatermarkForeground),
                typeof(Brush),
                typeof(HyRichTextBox),
                new FrameworkPropertyMetadata(null));

        public Brush WatermarkForeground
        {
            get => (Brush)GetValue(WatermarkForegroundProperty);
            set => SetValue(WatermarkForegroundProperty, value);
        }

        private static readonly DependencyPropertyKey IsDocumentEmptyPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(IsDocumentEmpty),
                typeof(bool),
                typeof(HyRichTextBox),
                new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty IsDocumentEmptyProperty =
            IsDocumentEmptyPropertyKey.DependencyProperty;

        public bool IsDocumentEmpty
        {
            get => (bool)GetValue(IsDocumentEmptyProperty);
            private set => SetValue(IsDocumentEmptyPropertyKey, value);
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            UpdateIsDocumentEmpty();
        }

        private void UpdateIsDocumentEmpty()
        {
            var start = Document.ContentStart;
            var end = Document.ContentEnd;
            IsDocumentEmpty = (start.GetOffsetToPosition(end) <= 4);
        }
    }
}
