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

        public static readonly DependencyProperty Argument3Property =
            DependencyProperty.Register(
                nameof(Argument3),
                typeof(object),
                typeof(HyTextBox),
                new FrameworkPropertyMetadata(null));

        public object? Argument3
        {
            get => GetValue(Argument3Property);
            set => SetValue(Argument3Property, value);
        }

        public static readonly DependencyProperty IsOnlyNumberProperty;

        public bool IsOnlyNumber
        {
            get => (bool)GetValue(IsOnlyNumberProperty);
            set => SetValue(IsOnlyNumberProperty, value);
        }

        // Track whether handlers are attached to prevent duplicates
        private bool _isOnlyNumberHandlersAttached;
        private bool _canPasteHandlersAttached;
        private bool _canKoreanHandlersAttached;
        private CommandBinding? _pasteBinding;
        private bool _cleanupHooked;

        static HyTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyTextBox), new FrameworkPropertyMetadata(typeof(HyTextBox)));

            WaterMarkProperty = DependencyProperty.Register("WaterMark", typeof(string), typeof(HyTextBox), new FrameworkPropertyMetadata());
            WaterMarkForegroundProperty = DependencyProperty.Register("WaterMarkForeground", typeof(SolidColorBrush), typeof(HyTextBox), new FrameworkPropertyMetadata());
            CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(HyTextBox), new FrameworkPropertyMetadata());
            CanPasteProperty = DependencyProperty.Register(nameof(CanPaste), typeof(bool), typeof(HyTextBox), new PropertyMetadata(true, OnCanPasteChanged));
            CanKoreanProperty = DependencyProperty.Register(nameof(CanKorean), typeof(bool), typeof(HyTextBox), new PropertyMetadata(true, OnCanKoreanChanged));
            IsOnlyNumberProperty = DependencyProperty.Register(
                nameof(IsOnlyNumber), typeof(bool), typeof(HyTextBox), new PropertyMetadata(false, OnIsOnlyNumberChanged));
        }

        public HyTextBox()
        {
            // Ensure cleanup when the control is unloaded from the visual tree
            Loaded += (_, __) =>
            {
                if (!_cleanupHooked)
                {
                    Unloaded += OnUnloadedCleanup;
                    _cleanupHooked = true;
                }
            };
        }

        private void OnUnloadedCleanup(object? sender, RoutedEventArgs e)
        {
            // Detach everything that may hold references to this control
            if (_isOnlyNumberHandlersAttached)
            {
                PreviewTextInput -= TbOnPreviewTextInputAllowOnlyNumber;
                DataObject.RemovePastingHandler(this, OnPasteAllowOnlyNumber);
                _isOnlyNumberHandlersAttached = false;
            }

            if (_canPasteHandlersAttached)
            {
                PreviewKeyDown -= TbOnPreviewKeyDownBlockPaste;
                DataObject.RemovePastingHandler(this, OnPasting);
                PreviewDragOver -= TbOnPreviewDragEventBlock;
                PreviewDrop -= TbOnPreviewDragEventBlock;
                AllowDrop = true;
                _canPasteHandlersAttached = false;
            }

            if (_pasteBinding is not null)
            {
                CommandBindings.Remove(_pasteBinding);
                _pasteBinding = null;
            }

            if (_canKoreanHandlersAttached)
            {
                PreviewTextInput -= TbOnPreviewTextInputBlockKorean;
                // Restore IME to default (enabled)
                InputMethod.SetIsInputMethodEnabled(this, true);
                _canKoreanHandlersAttached = false;
            }
        }

        private static void OnIsOnlyNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HyTextBox tb)
            {
                if ((bool)e.NewValue == true)
                {
                    if (!tb._isOnlyNumberHandlersAttached)
                    {
                        tb.PreviewTextInput += TbOnPreviewTextInputAllowOnlyNumber;
                        DataObject.AddPastingHandler(tb, OnPasteAllowOnlyNumber);
                        tb._isOnlyNumberHandlersAttached = true;
                    }
                }
                else
                {
                    if (tb._isOnlyNumberHandlersAttached)
                    {
                        tb.PreviewTextInput -= TbOnPreviewTextInputAllowOnlyNumber;
                        DataObject.RemovePastingHandler(tb, OnPasteAllowOnlyNumber);
                        tb._isOnlyNumberHandlersAttached = false;
                    }
                }
            }
        }

        private static void TbOnPreviewTextInputAllowOnlyNumber(object sender, TextCompositionEventArgs e)
        {
            if (!e.Text.All(char.IsDigit) && e.Text != ".")
            {
                e.Handled = true;
            }
        }

        private static void OnPasteAllowOnlyNumber(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(DataFormats.Text))
            {
                string text = e.DataObject.GetData(DataFormats.Text) as string;
                if (!string.IsNullOrEmpty(text) && !text.All(char.IsDigit))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private static void OnCanPasteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HyTextBox tb)
            {
                if ((bool)e.NewValue == false)
                {
                    // Paste command disable (avoid adding multiple times)
                    if (tb._pasteBinding is null)
                    {
                        tb._pasteBinding = new CommandBinding(ApplicationCommands.Paste,
                            (_, ee) => ee.Handled = true,
                            (_, ce) => ce.CanExecute = false);
                        tb.CommandBindings.Add(tb._pasteBinding);
                    }

                    if (!tb._canPasteHandlersAttached)
                    {
                        // Block Ctrl+V / Shift+Insert
                        tb.PreviewKeyDown += TbOnPreviewKeyDownBlockPaste;

                        // Block paste event
                        DataObject.AddPastingHandler(tb, OnPasting);

                        // Block drag and drop
                        tb.AllowDrop = false;
                        tb.PreviewDragOver += TbOnPreviewDragEventBlock;
                        tb.PreviewDrop += TbOnPreviewDragEventBlock;

                        tb._canPasteHandlersAttached = true;
                    }
                }
                else
                {
                    // Unset
                    if (tb._canPasteHandlersAttached)
                    {
                        tb.PreviewKeyDown -= TbOnPreviewKeyDownBlockPaste;
                        DataObject.RemovePastingHandler(tb, OnPasting);
                        tb.PreviewDragOver -= TbOnPreviewDragEventBlock;
                        tb.PreviewDrop -= TbOnPreviewDragEventBlock;
                        tb.AllowDrop = true;
                        tb._canPasteHandlersAttached = false;
                    }

                    if (tb._pasteBinding is not null)
                    {
                        tb.CommandBindings.Remove(tb._pasteBinding);
                        tb._pasteBinding = null;
                    }
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
                    // IME disable
                    InputMethod.SetIsInputMethodEnabled(tb, false);
                    if (!tb._canKoreanHandlersAttached)
                    {
                        tb.PreviewTextInput += TbOnPreviewTextInputBlockKorean;
                        tb._canKoreanHandlersAttached = true;
                    }
                }
                else
                {
                    InputMethod.SetIsInputMethodEnabled(tb, true);
                    if (tb._canKoreanHandlersAttached)
                    {
                        tb.PreviewTextInput -= TbOnPreviewTextInputBlockKorean;
                        tb._canKoreanHandlersAttached = false;
                    }
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
