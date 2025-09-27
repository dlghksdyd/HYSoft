using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace HYSoft.Presentation.Styles.Controls
{
    public class HyTextBox : TextBox
    {
        public static readonly DependencyProperty WaterMarkProperty;
        public static readonly DependencyProperty WaterMarkForegroundProperty;
        public static readonly DependencyProperty CornerRadiusProperty;
        public static readonly DependencyProperty CanPasteProperty;
        public static readonly DependencyProperty CanKoreanProperty;

        public string WaterMark
        {
            get { return (string)GetValue(WaterMarkProperty); }
            set { SetValue(WaterMarkProperty, value); }
        }

        public SolidColorBrush WaterMarkForeground
        {
            get { return (SolidColorBrush)GetValue(WaterMarkForegroundProperty); }
            set { SetValue(WaterMarkForegroundProperty, value); }
        }

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public bool CanPaste
        {
            get => (bool)GetValue(CanPasteProperty);
            set => SetValue(CanPasteProperty, value);
        }

        public bool CanKorean
        {
            get => (bool)GetValue(CanKoreanProperty);
            set => SetValue(CanKoreanProperty, value);
        }

        public static readonly DependencyProperty Argument1Property =
            DependencyProperty.Register(
                nameof(Argument1),
                typeof(object),
                typeof(HyTextBox),
                new FrameworkPropertyMetadata(null));

        public object? Argument1
        {
            get => GetValue(Argument1Property);
            set => SetValue(Argument1Property, value);
        }

        public static readonly DependencyProperty Argument2Property =
            DependencyProperty.Register(
                nameof(Argument2),
                typeof(object),
                typeof(HyTextBox),
                new FrameworkPropertyMetadata(null));

        public object? Argument2
        {
            get => GetValue(Argument2Property);
            set => SetValue(Argument2Property, value);
        }

        static HyTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyTextBox), new FrameworkPropertyMetadata(typeof(HyTextBox)));

            WaterMarkProperty = DependencyProperty.Register("WaterMark", typeof(string), typeof(HyTextBox), new FrameworkPropertyMetadata());
            WaterMarkForegroundProperty = DependencyProperty.Register("WaterMarkForeground", typeof(SolidColorBrush), typeof(HyTextBox), new FrameworkPropertyMetadata());
            CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(HyTextBox), new FrameworkPropertyMetadata());
            CanPasteProperty = DependencyProperty.Register(nameof(CanPaste), typeof(bool), typeof(HyTextBox), new PropertyMetadata(true, OnCanPasteChanged));
            CanKoreanProperty = DependencyProperty.Register(nameof(CanKorean), typeof(bool), typeof(HyTextBox), new PropertyMetadata(true, OnCanKoreanChanged));
        }

        private static void OnCanPasteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HyTextBox tb)
            {
                if ((bool)e.NewValue == false)
                {
                    // Paste 명령 무력화
                    var pasteBinding = new CommandBinding(ApplicationCommands.Paste,
                        (_, ee) => ee.Handled = true,
                        (_, ce) => ce.CanExecute = false);
                    tb.CommandBindings.Add(pasteBinding);

                    // Ctrl+V 직접 차단
                    tb.PreviewKeyDown += TbOnPreviewKeyDownBlockPaste;

                    // 붙여넣기 이벤트 차단
                    DataObject.AddPastingHandler(tb, OnPasting);

                    // 드래그-드롭 차단
                    tb.AllowDrop = false;
                    tb.PreviewDragOver += TbOnPreviewDragEventBlock;
                    tb.PreviewDrop += TbOnPreviewDragEventBlock;
                }
                else
                {
                    // 해제
                    tb.PreviewKeyDown -= TbOnPreviewKeyDownBlockPaste;
                    DataObject.RemovePastingHandler(tb, OnPasting);
                    tb.PreviewDragOver -= TbOnPreviewDragEventBlock;
                    tb.PreviewDrop -= TbOnPreviewDragEventBlock;

                    tb.AllowDrop = true;
                }
            }
        }

        private static void TbOnPreviewKeyDownBlockPaste(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.V) ||
                (Keyboard.Modifiers == ModifierKeys.Shift && e.Key == Key.Insert))
            {
                e.Handled = true;
            }
        }

        private static void OnPasting(object sender, DataObjectPastingEventArgs e)
        {
            e.CancelCommand();
        }

        private static void TbOnPreviewDragEventBlock(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private static void OnCanKoreanChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HyTextBox tb)
            {
                if ((bool)e.NewValue == false)
                {
                    // IME 차단
                    InputMethod.SetIsInputMethodEnabled(tb, false);
                    tb.PreviewTextInput += TbOnPreviewTextInputBlockKorean;
                }
                else
                {
                    InputMethod.SetIsInputMethodEnabled(tb, true);
                    tb.PreviewTextInput -= TbOnPreviewTextInputBlockKorean;
                }
            }
        }

        private static void TbOnPreviewTextInputBlockKorean(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if ((c >= 0xAC00 && c <= 0xD7A3) ||  // 가~힣
                    (c >= 0x1100 && c <= 0x11FF) ||  // 자모
                    (c >= 0x3130 && c <= 0x318F))    // 호환 자모
                {
                    e.Handled = true;
                    return;
                }
            }
        }
    }
}
