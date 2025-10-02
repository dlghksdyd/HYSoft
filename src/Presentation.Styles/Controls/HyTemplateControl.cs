// ======================== HyTemplateControl.cs ========================
// Adds: Composite CommandParameter payload, input kind & constraints, keyboard/focus UX.
// Namespace: HYSoft.Presentation.Styles.Controls

using HYSoft.Presentation.Styles.Icons;
using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace HYSoft.Presentation.Styles.Controls
{
    public enum EComponentType
    {
        None,
        TextBox,
        ComboBox,
        CheckBox,
        RadioButton,
        DatePicker,
        NumericUpDown,
        Custom // 확장용
    }

    [ContentProperty(nameof(Content))]
    public class HyTemplateControl : Control
    {
        static HyTemplateControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(HyTemplateControl),
                new FrameworkPropertyMetadata(typeof(HyTemplateControl)));
        }

        #region DATA / CONTENT

        public int Column
        {
            get => (int)GetValue(ColumnProperty);
            set => SetValue(ColumnProperty, value);
        }

        public static readonly DependencyProperty ColumnProperty =
            DependencyProperty.Register(
                nameof(Column),
                typeof(int),
                typeof(HyTemplateControl),
                new FrameworkPropertyMetadata(
                    0, // 기본값
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange |
                    FrameworkPropertyMetadataOptions.AffectsParentArrange));

        public object? Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(
                nameof(Content),
                typeof(object),
                typeof(HyTemplateControl),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange |
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public DataTemplate? ContentTemplate
        {
            get => (DataTemplate?)GetValue(ContentTemplateProperty);
            set => SetValue(ContentTemplateProperty, value);
        }
        public static readonly DependencyProperty ContentTemplateProperty =
            DependencyProperty.Register(
                nameof(ContentTemplate),
                typeof(DataTemplate),
                typeof(HyTemplateControl),
                new FrameworkPropertyMetadata(null));

        public IEnumerable? ItemsSource
        {
            get => (IEnumerable?)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(IEnumerable),
                typeof(HyTemplateControl),
                new FrameworkPropertyMetadata(null));

        public object? SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                nameof(SelectedItem),
                typeof(object),
                typeof(HyTemplateControl),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string? Text
        {
            get => (string?)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(HyTemplateControl),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public object? Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(object), typeof(HyTemplateControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public object? Value2
        {
            get => GetValue(Value2Property);
            set => SetValue(Value2Property, value);
        }
        public static readonly DependencyProperty Value2Property =
            DependencyProperty.Register(nameof(Value2), typeof(object), typeof(HyTemplateControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string? Header
        {
            get => (string?)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(string), typeof(HyTemplateControl));

        public object? HeaderContent
        {
            get => GetValue(HeaderContentProperty);
            set => SetValue(HeaderContentProperty, value);
        }
        public static readonly DependencyProperty HeaderContentProperty =
            DependencyProperty.Register(
                nameof(HeaderContent),
                typeof(object),
                typeof(HyTemplateControl),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange |
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public DataTemplate? HeaderTemplate
        {
            get => (DataTemplate?)GetValue(HeaderTemplateProperty);
            set => SetValue(HeaderTemplateProperty, value);
        }
        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.Register(
                nameof(HeaderTemplate),
                typeof(DataTemplate),
                typeof(HyTemplateControl),
                new FrameworkPropertyMetadata(null));

        public string? Description
        {
            get => (string?)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(nameof(Description), typeof(string), typeof(HyTemplateControl));

        public string? Placeholder
        {
            get => (string?)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register(nameof(Placeholder), typeof(string), typeof(HyTemplateControl));

        public string? FormatString
        {
            get => (string?)GetValue(FormatStringProperty);
            set => SetValue(FormatStringProperty, value);
        }
        public static readonly DependencyProperty FormatStringProperty =
            DependencyProperty.Register(nameof(FormatString), typeof(string), typeof(HyTemplateControl));

        public IFormatProvider? FormatProvider
        {
            get => (IFormatProvider?)GetValue(FormatProviderProperty);
            set => SetValue(FormatProviderProperty, value);
        }
        public static readonly DependencyProperty FormatProviderProperty =
            DependencyProperty.Register(nameof(FormatProvider), typeof(IFormatProvider), typeof(HyTemplateControl));

        #endregion

        #region INPUT BEHAVIOR
        
        /// <summary>
        /// 한글 입력 허용 여부 (기본값: true)
        /// </summary>
        public bool CanKorean
        {
            get => (bool)GetValue(CanKoreanProperty);
            set => SetValue(CanKoreanProperty, value);
        }

        public static readonly DependencyProperty CanKoreanProperty =
            DependencyProperty.Register(
                nameof(CanKorean),
                typeof(bool),
                typeof(HyTemplateControl),
                new PropertyMetadata(true));

        /// <summary>
        /// 붙여넣기 허용 여부 (기본값: true)
        /// </summary>
        public bool CanPaste
        {
            get => (bool)GetValue(CanPasteProperty);
            set => SetValue(CanPasteProperty, value);
        }

        public static readonly DependencyProperty CanPasteProperty =
            DependencyProperty.Register(
                nameof(CanPaste),
                typeof(bool),
                typeof(HyTemplateControl),
                new PropertyMetadata(true));

        /// <summary>
        /// 숫자만 입력 허용 여부 (기본값: false)
        /// </summary>
        public bool IsOnlyNumber
        {
            get => (bool)GetValue(IsOnlyNumberProperty);
            set => SetValue(IsOnlyNumberProperty, value);
        }

        public static readonly DependencyProperty IsOnlyNumberProperty =
            DependencyProperty.Register(
                nameof(IsOnlyNumber),
                typeof(bool),
                typeof(HyTemplateControl),
                new PropertyMetadata(false));

        #endregion

        #region STATE

        public EComponentType ComponentType
        {
            get => (EComponentType)GetValue(ComponentTypeProperty);
            set => SetValue(ComponentTypeProperty, value);
        }

        public static readonly DependencyProperty ComponentTypeProperty =
            DependencyProperty.Register(
                nameof(ComponentType),
                typeof(EComponentType),
                typeof(HyTemplateControl),
                new FrameworkPropertyMetadata(EComponentType.None));

        public bool IsEditable
        {
            get => (bool)GetValue(IsEditableProperty);
            set => SetValue(IsEditableProperty, value);
        }
        public static readonly DependencyProperty IsEditableProperty =
            DependencyProperty.Register(nameof(IsEditable), typeof(bool), typeof(HyTemplateControl), new PropertyMetadata(false));

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(HyTemplateControl), new PropertyMetadata(false));

        public bool IsRequired
        {
            get => (bool)GetValue(IsRequiredProperty);
            set => SetValue(IsRequiredProperty, value);
        }
        public static readonly DependencyProperty IsRequiredProperty =
            DependencyProperty.Register(nameof(IsRequired), typeof(bool), typeof(HyTemplateControl), new PropertyMetadata(false));

        public bool IsValid
        {
            get => (bool)GetValue(IsValidProperty);
            set => SetValue(IsValidProperty, value);
        }
        public static readonly DependencyProperty IsValidProperty =
            DependencyProperty.Register(nameof(IsValid), typeof(bool), typeof(HyTemplateControl), new PropertyMetadata(true));

        public bool IsModifying
        {
            get => (bool)GetValue(IsModifyingProperty);
            set => SetValue(IsModifyingProperty, value);
        }
        public static readonly DependencyProperty IsModifyingProperty =
            DependencyProperty.Register(
                nameof(IsModifying),
                typeof(bool),
                typeof(HyTemplateControl),
                new FrameworkPropertyMetadata(false));

        public bool IsCreating
        {
            get => (bool)GetValue(IsCreatingProperty);
            set => SetValue(IsCreatingProperty, value);
        }
        public static readonly DependencyProperty IsCreatingProperty =
            DependencyProperty.Register(
                nameof(IsCreating),
                typeof(bool),
                typeof(HyTemplateControl),
                new FrameworkPropertyMetadata(false));

        #endregion

        #region VISUAL

        public EIconKeys? Icon
        {
            get => (EIconKeys?)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(
                nameof(Icon),
                typeof(EIconKeys?),
                typeof(HyTemplateControl),
                new FrameworkPropertyMetadata());

        public EIconKeys? PrefixIcon
        {
            get => (EIconKeys?)GetValue(PrefixIconProperty);
            set => SetValue(PrefixIconProperty, value);
        }
        public static readonly DependencyProperty PrefixIconProperty =
            DependencyProperty.Register(
                nameof(PrefixIcon),
                typeof(EIconKeys?),
                typeof(HyTemplateControl),
                new FrameworkPropertyMetadata());

        public EIconKeys? SuffixIcon
        {
            get => (EIconKeys?)GetValue(SuffixIconProperty);
            set => SetValue(SuffixIconProperty, value);
        }
        public static readonly DependencyProperty SuffixIconProperty =
            DependencyProperty.Register(
                nameof(SuffixIcon),
                typeof(EIconKeys?),
                typeof(HyTemplateControl),
                new FrameworkPropertyMetadata());
        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(HyTemplateControl), new PropertyMetadata(new CornerRadius(4)));
        public Visibility DisplayVisibility
        {
            get => (Visibility)GetValue(DisplayVisibilityProperty);
            set => SetValue(DisplayVisibilityProperty, value);
        }
        public static readonly DependencyProperty DisplayVisibilityProperty =
            DependencyProperty.Register(
                nameof(DisplayVisibility),
                typeof(Visibility),
                typeof(HyTemplateControl),
                new FrameworkPropertyMetadata(Visibility.Visible, FrameworkPropertyMetadataOptions.AffectsRender));

        public Visibility EditVisibility
        {
            get => (Visibility)GetValue(EditVisibilityProperty);
            set => SetValue(EditVisibilityProperty, value);
        }
        public static readonly DependencyProperty EditVisibilityProperty =
            DependencyProperty.Register(
                nameof(EditVisibility),
                typeof(Visibility),
                typeof(HyTemplateControl),
                new FrameworkPropertyMetadata(Visibility.Collapsed, FrameworkPropertyMetadataOptions.AffectsRender));

        public Visibility HelpVisibility
        {
            get => (Visibility)GetValue(HelpVisibilityProperty);
            set => SetValue(HelpVisibilityProperty, value);
        }
        public static readonly DependencyProperty HelpVisibilityProperty =
            DependencyProperty.Register(
                nameof(HelpVisibility),
                typeof(Visibility),
                typeof(HyTemplateControl),
                new FrameworkPropertyMetadata(Visibility.Visible, FrameworkPropertyMetadataOptions.AffectsRender));

        #endregion

        #region COMMANDS PARAMETER

        public ICommand? EnterCommand
        {
            get => (ICommand?)GetValue(EnterCommandProperty);
            set => SetValue(EnterCommandProperty, value);
        }
        public static readonly DependencyProperty EnterCommandProperty =
            DependencyProperty.Register(nameof(EnterCommand), typeof(ICommand), typeof(HyTemplateControl));

        public ICommand? LeaveCommand
        {
            get => (ICommand?)GetValue(LeaveCommandProperty);
            set => SetValue(LeaveCommandProperty, value);
        }
        public static readonly DependencyProperty LeaveCommandProperty =
            DependencyProperty.Register(nameof(LeaveCommand), typeof(ICommand), typeof(HyTemplateControl));

        public ICommand? ConfirmCommand
        {
            get => (ICommand?)GetValue(ConfirmCommandProperty);
            set => SetValue(ConfirmCommandProperty, value);
        }
        public static readonly DependencyProperty ConfirmCommandProperty =
            DependencyProperty.Register(nameof(ConfirmCommand), typeof(ICommand), typeof(HyTemplateControl));

        public ICommand? CancelCommand
        {
            get => (ICommand?)GetValue(CancelCommandProperty);
            set => SetValue(CancelCommandProperty, value);
        }
        public static readonly DependencyProperty CancelCommandProperty =
            DependencyProperty.Register(nameof(CancelCommand), typeof(ICommand), typeof(HyTemplateControl));

        public ICommand? CreateCommand
        {
            get => (ICommand?)GetValue(CreateCommandProperty);
            set => SetValue(CreateCommandProperty, value);
        }
        public static readonly DependencyProperty CreateCommandProperty =
            DependencyProperty.Register(
                nameof(CreateCommand),
                typeof(ICommand),
                typeof(HyTemplateControl));

        public ICommand? ModifyCommand
        {
            get => (ICommand?)GetValue(ModifyCommandProperty);
            set => SetValue(ModifyCommandProperty, value);
        }
        public static readonly DependencyProperty ModifyCommandProperty =
            DependencyProperty.Register(
                nameof(ModifyCommand),
                typeof(ICommand),
                typeof(HyTemplateControl));

        public ICommand? DeleteCommand
        {
            get => (ICommand?)GetValue(DeleteCommandProperty);
            set => SetValue(DeleteCommandProperty, value);
        }

        public ICommand? SaveCommand
        {
            get => (ICommand?)GetValue(SaveCommandProperty);
            set => SetValue(SaveCommandProperty, value);
        }

        public static readonly DependencyProperty SaveCommandProperty =
            DependencyProperty.Register(
                nameof(SaveCommand),
                typeof(ICommand),
                typeof(HyTemplateControl));


        public static readonly DependencyProperty DeleteCommandProperty =
            DependencyProperty.Register(
                nameof(DeleteCommand),
                typeof(ICommand),
                typeof(HyTemplateControl));


        public object? CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(HyTemplateControl));

        #endregion

        #region Events
        
        public RoutedEvent TriggerEvent
        {
            get => (RoutedEvent)GetValue(TriggerEventProperty);
            set => SetValue(TriggerEventProperty, value);
        }
        public static readonly DependencyProperty TriggerEventProperty =
            DependencyProperty.Register(
                nameof(TriggerEvent),
                typeof(RoutedEvent),
                typeof(HyTemplateControl),
                new FrameworkPropertyMetadata());
        
        #endregion
    }
}

