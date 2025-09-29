// ======================== HyTemplateControl.cs ========================
// Adds: Composite CommandParameter payload, input kind & constraints, keyboard/focus UX.
// Namespace: HYSoft.Presentation.Styles.Controls

using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace HYSoft.Presentation.Styles.Controls
{
    [TemplatePart(Name = PartTextBox, Type = typeof(TextBox))]
    [TemplatePart(Name = PartPasswordBox, Type = typeof(PasswordBox))]
    [TemplatePart(Name = PartDatePicker, Type = typeof(DatePicker))]
    [TemplateVisualState(GroupName = "EditStates", Name = "View")]
    [TemplateVisualState(GroupName = "EditStates", Name = "Edit")]
    public class HyTemplateControl : Control
    {
        public const string PartTextBox = "PART_TextBox";
        public const string PartPasswordBox = "PART_Password";
        public const string PartDatePicker = "PART_DatePicker";

        private TextBox? _textBox;
        private PasswordBox? _passwordBox;
        private DatePicker? _datePicker;

        static HyTemplateControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(HyTemplateControl),
                new FrameworkPropertyMetadata(typeof(HyTemplateControl)));
        }

        public HyTemplateControl()
        {
            
        }

        #region DATA / CONTENT

        public object? Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(object), typeof(HyTemplateControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

        public string? Header
        {
            get => (string?)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(string), typeof(HyTemplateControl));

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

        #region STATE

        public bool IsEditing
        {
            get => (bool)GetValue(IsEditingProperty);
            set => SetValue(IsEditingProperty, value);
        }
        public static readonly DependencyProperty IsEditingProperty =
            DependencyProperty.Register(nameof(IsEditing), typeof(bool), typeof(HyTemplateControl),
                new PropertyMetadata(false, OnIsEditingChanged));

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

        public string? ErrorMessage
        {
            get => (string?)GetValue(ErrorMessageProperty);
            set => SetValue(ErrorMessageProperty, value);
        }
        public static readonly DependencyProperty ErrorMessageProperty =
            DependencyProperty.Register(nameof(ErrorMessage), typeof(string), typeof(HyTemplateControl));

        #endregion

        #region VISUAL HINTS

        public object? PrefixIcon
        {
            get => GetValue(PrefixIconProperty);
            set => SetValue(PrefixIconProperty, value);
        }
        public static readonly DependencyProperty PrefixIconProperty =
            DependencyProperty.Register(nameof(PrefixIcon), typeof(object), typeof(HyTemplateControl));

        public object? SuffixIcon
        {
            get => GetValue(SuffixIconProperty);
            set => SetValue(SuffixIconProperty, value);
        }
        public static readonly DependencyProperty SuffixIconProperty =
            DependencyProperty.Register(nameof(SuffixIcon), typeof(object), typeof(HyTemplateControl));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(HyTemplateControl), new PropertyMetadata(new CornerRadius(4)));

        #endregion

        #region INPUT KIND & CONSTRAINTS

        public InputKind Kind
        {
            get => (InputKind)GetValue(KindProperty);
            set => SetValue(KindProperty, value);
        }
        public static readonly DependencyProperty KindProperty =
            DependencyProperty.Register(nameof(Kind), typeof(InputKind), typeof(HyTemplateControl), new PropertyMetadata(InputKind.Text));

        // Number constraints
        public double? Min
        {
            get => (double?)GetValue(MinProperty);
            set => SetValue(MinProperty, value);
        }
        public static readonly DependencyProperty MinProperty =
            DependencyProperty.Register(nameof(Min), typeof(double?), typeof(HyTemplateControl));

        public double? Max
        {
            get => (double?)GetValue(MaxProperty);
            set => SetValue(MaxProperty, value);
        }
        public static readonly DependencyProperty MaxProperty =
            DependencyProperty.Register(nameof(Max), typeof(double?), typeof(HyTemplateControl));

        public double Step
        {
            get => (double)GetValue(StepProperty);
            set => SetValue(StepProperty, value);
        }
        public static readonly DependencyProperty StepProperty =
            DependencyProperty.Register(nameof(Step), typeof(double), typeof(HyTemplateControl), new PropertyMetadata(1d));

        public int DecimalPlaces
        {
            get => (int)GetValue(DecimalPlacesProperty);
            set => SetValue(DecimalPlacesProperty, value);
        }
        public static readonly DependencyProperty DecimalPlacesProperty =
            DependencyProperty.Register(nameof(DecimalPlaces), typeof(int), typeof(HyTemplateControl), new PropertyMetadata(0));

        // Date constraints
        public DateTime? MinDate
        {
            get => (DateTime?)GetValue(MinDateProperty);
            set => SetValue(MinDateProperty, value);
        }
        public static readonly DependencyProperty MinDateProperty =
            DependencyProperty.Register(nameof(MinDate), typeof(DateTime?), typeof(HyTemplateControl));

        public DateTime? MaxDate
        {
            get => (DateTime?)GetValue(MaxDateProperty);
            set => SetValue(MaxDateProperty, value);
        }
        public static readonly DependencyProperty MaxDateProperty =
            DependencyProperty.Register(nameof(MaxDate), typeof(DateTime?), typeof(HyTemplateControl));

        // Masking
        public string? RegexPattern
        {
            get => (string?)GetValue(RegexPatternProperty);
            set => SetValue(RegexPatternProperty, value);
        }
        public static readonly DependencyProperty RegexPatternProperty =
            DependencyProperty.Register(nameof(RegexPattern), typeof(string), typeof(HyTemplateControl));

        public char PasswordChar
        {
            get => (char)GetValue(PasswordCharProperty);
            set => SetValue(PasswordCharProperty, value);
        }
        public static readonly DependencyProperty PasswordCharProperty =
            DependencyProperty.Register(nameof(PasswordChar), typeof(char), typeof(HyTemplateControl), new PropertyMetadata('•'));

        #endregion

        #region COMMANDS & COMPOSITE PARAMETER

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

        public object? CommandArgument
        {
            get => GetValue(CommandArgumentProperty);
            set => SetValue(CommandArgumentProperty, value);
        }
        public static readonly DependencyProperty CommandArgumentProperty =
            DependencyProperty.Register(nameof(CommandArgument), typeof(object), typeof(HyTemplateControl));

        #endregion

        #region TEMPLATE SETTINGS

        public HyTemplateSettings TemplateSettings
        {
            get => (HyTemplateSettings)GetValue(TemplateSettingsProperty);
            private set => SetValue(TemplateSettingsPropertyKey, value);
        }

        private static readonly DependencyPropertyKey TemplateSettingsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(TemplateSettings), typeof(HyTemplateSettings), typeof(HyTemplateControl),
                new PropertyMetadata(new HyTemplateSettings()));

        public static readonly DependencyProperty TemplateSettingsProperty = TemplateSettingsPropertyKey.DependencyProperty;

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HyTemplateControl c)
            {
                c.TemplateSettings = c.TemplateSettings with
                {
                    IsEmpty = e.NewValue is null || (e.NewValue is string s && string.IsNullOrEmpty(s))
                };
            }
        }

        private static void OnIsEditingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HyTemplateControl c)
            {
                c.UpdateVisualState(true);
                if (c.IsEditing)
                {
                    c.Dispatcher.InvokeAsync(() => c.FocusEditor());
                }
            }
        }

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _textBox = GetTemplateChild(PartTextBox) as TextBox;
            _passwordBox = GetTemplateChild(PartPasswordBox) as PasswordBox;
            _datePicker = GetTemplateChild(PartDatePicker) as DatePicker;

            HookupEditorEvents();
            UpdateVisualState(false);
        }

        private void HookupEditorEvents()
        {
            // Clear prior
            if (_textBox != null)
            {
                _textBox.PreviewTextInput -= TextBox_OnPreviewTextInput_NumberFilter;
                DataObject.RemovePastingHandler(_textBox, OnTextBoxPaste);
            }

            if (_textBox != null)
            {
                if (Kind == InputKind.Number)
                {
                    _textBox.PreviewTextInput += TextBox_OnPreviewTextInput_NumberFilter;
                    DataObject.AddPastingHandler(_textBox, OnTextBoxPaste);
                }
            }

            if (_passwordBox != null)
            {
                // nothing special; template masks via PasswordChar DP if using a custom presenter
            }

            if (_datePicker != null)
            {
                _datePicker.SelectedDateChanged -= DatePicker_SelectedDateChanged;
                _datePicker.SelectedDateChanged += DatePicker_SelectedDateChanged;
            }
        }

        private void DatePicker_SelectedDateChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (_datePicker?.SelectedDate is DateTime dt)
            {
                if (MinDate is { } min && dt < min) _datePicker.SelectedDate = min;
                if (MaxDate is { } max && dt > max) _datePicker.SelectedDate = max;
                Value = _datePicker.SelectedDate;
            }
        }

        private void TextBox_OnPreviewTextInput_NumberFilter(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowedForNumber(e.Text, (sender as TextBox)?.Text, DecimalPlaces);
        }

        private void OnTextBoxPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (Kind != InputKind.Number) return;
            if (!e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText)) return;
            var paste = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string;
            if (!IsTextAllowedForNumber(paste ?? string.Empty, (sender as TextBox)?.Text, DecimalPlaces))
                e.CancelCommand();
        }

        private static bool IsTextAllowedForNumber(string incoming, string? existing, int decimals)
        {
            // Allow sign, digits, decimal point depending on decimals
            var dec = decimals > 0;
            var pattern = dec ? "^[+-]?[0-9]*([.][0-9]*)?$" : "^[+-]?[0-9]*$";
            var next = (existing ?? string.Empty) + incoming;
            return Regex.IsMatch(next, pattern);
        }

        private void FocusEditor()
        {
            if (Kind == InputKind.Password && _passwordBox != null)
            {
                _passwordBox.Focus();
                _passwordBox.SelectAll();
                return;
            }
            if (Kind == InputKind.Date && _datePicker != null)
            {
                _datePicker.Focus();
                return;
            }
            if (_textBox != null)
            {
                _textBox.Focus();
                _textBox.SelectAll();
            }
        }

        private void UpdateVisualState(bool useTransition)
        {
            VisualStateManager.GoToState(this, IsEditing ? "Edit" : "View", useTransition);
        }
    }

    public enum InputKind { Text, Number, Date, Password }

    public enum CommandAction { Enter, Leave, Confirm, Cancel }

    public record CommandPayload(object? Key, object? Value, object? Argument, CommandAction Action);

    // MultiValueConverter to construct CommandPayload from bindings
    public sealed class CommandPayloadMultiConverter : System.Windows.Data.IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // values: [Key, Value, Argument, Action(optional)]
            var key = values.Length > 0 ? values[0] : null;
            var val = values.Length > 1 ? values[1] : null;
            var arg = values.Length > 2 ? values[2] : null;

            CommandAction action = CommandAction.Confirm;
            if (parameter is CommandAction pAct)
                action = pAct;
            else if (parameter is string s && Enum.TryParse<CommandAction>(s, true, out var e))
                action = e;
            else if (values.Length > 3 && values[3] is CommandAction a2)
                action = a2;

            return new CommandPayload(key, val, arg, action);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public record HyTemplateSettings
    {
        public bool IsEmpty { get; init; } = true;
    }
}


/* ======================== Themes/generic.xaml ========================
<ResourceDictionary
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:local="clr-namespace:HYSoft.Presentation.Styles.Controls">

    <!-- Converter instance -->
    <local:CommandPayloadMultiConverter x:Key="CommandPayloadMultiConverter"/>

    <Style TargetType="{x:Type local:HyTemplateControl}">
        <Setter Property="KeyboardNavigation.TabNavigation" Value="Local"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="{x:Type local:HyTemplateControl}">
                    <Border x:Name="Root" CornerRadius="{TemplateBinding CornerRadius}">
                        <Grid>
                            <!-- DISPLAY VIEW -->
                            <Grid x:Name="DisplayView">
                                <Grid.ColumnDefinitions>
                                    <ColumnDefinition Width="Auto"/>
                                    <ColumnDefinition Width="*"/>
                                    <ColumnDefinition Width="Auto"/>
                                </Grid.ColumnDefinitions>
                                <ContentPresenter Grid.Column="0" Content="{TemplateBinding PrefixIcon}"/>
                                <TextBlock Grid.Column="1" Text="{TemplateBinding Value}"/>
                                <ContentPresenter Grid.Column="2" Content="{TemplateBinding SuffixIcon}"/>
                            </Grid>

                            <!-- EDIT VIEW: switch by Kind -->
                            <Grid x:Name="EditView" Visibility="Collapsed">
                                <Grid>
                                    <!-- Text / Number share TextBox, number is filtered in code-behind -->
                                    <TextBox x:Name="PART_TextBox"
                                             Visibility="Collapsed"
                                             IsReadOnly="{TemplateBinding IsReadOnly}"
                                             Text="{Binding Value, RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}">
                                        <TextBox.Style>
                                            <Style TargetType="TextBox">
                                                <Setter Property="Template">
                                                    <Setter.Value>
                                                        <ControlTemplate TargetType="TextBox">
                                                            <Grid>
                                                                <ScrollViewer x:Name="PART_ContentHost"/>
                                                                <TextBlock x:Name="Watermark" IsHitTestVisible="False" Margin="4,2,4,2" Foreground="Gray"
                                                                           Text="{Binding Placeholder, RelativeSource={RelativeSource TemplatedParent}}"
                                                                           Visibility="Collapsed"/>
                                                            </Grid>
                                                            <ControlTemplate.Triggers>
                                                                <DataTrigger Value="True">
                                                                    <DataTrigger.Binding>
                                                                        <Binding Path="TemplateSettings.IsEmpty" RelativeSource="{RelativeSource TemplatedParent}"/>
                                                                    </DataTrigger.Binding>
                                                                    <Setter TargetName="Watermark" Property="Visibility" Value="Visible"/>
                                                                </DataTrigger>
                                                            </ControlTemplate.Triggers>
                                                        </ControlTemplate>
                                                    </Setter.Value>
                                                </Setter>
                                            </Style>
                                        </TextBox.Style>
                                    </TextBox>

                                    <!-- Password -->
                                    <PasswordBox x:Name="PART_Password" Visibility="Collapsed"/>

                                    <!-- Date -->
                                    <DatePicker x:Name="PART_DatePicker" Visibility="Collapsed"
                                                SelectedDate="{Binding Value, RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                                                DisplayDateStart="{TemplateBinding MinDate}"
                                                DisplayDateEnd="{TemplateBinding MaxDate}"/>
                                </Grid>
                            </Grid>

                            <!-- Description & Error -->
                            <StackPanel x:Name="HelpAndError" Margin="0,4,0,0">
                                <TextBlock Foreground="DarkGray" Text="{TemplateBinding Description}"/>
                                <TextBlock x:Name="ErrorText" Foreground="DarkRed" Text="{TemplateBinding ErrorMessage}" Visibility="Collapsed"/>
                            </StackPanel>
                        </Grid>
                    </Border>

                    <!-- VISUAL STATES -->
                    <VisualStateManager.VisualStateGroups>
                        <VisualStateGroup x:Name="EditStates">
                            <VisualState x:Name="View">
                                <Storyboard>
                                    <ObjectAnimationUsingKeyFrames Storyboard.TargetName="DisplayView" Storyboard.TargetProperty="Visibility">
                                        <DiscreteObjectKeyFrame KeyTime="0:0:0" Value="Visible"/>
                                    </ObjectAnimationUsingKeyFrames>
                                    <ObjectAnimationUsingKeyFrames Storyboard.TargetName="EditView" Storyboard.TargetProperty="Visibility">
                                        <DiscreteObjectKeyFrame KeyTime="0:0:0" Value="Collapsed"/>
                                    </ObjectAnimationUsingKeyFrames>
                                </Storyboard>
                            </VisualState>
                            <VisualState x:Name="Edit">
                                <Storyboard>
                                    <ObjectAnimationUsingKeyFrames Storyboard.TargetName="DisplayView" Storyboard.TargetProperty="Visibility">
                                        <DiscreteObjectKeyFrame KeyTime="0:0:0" Value="Collapsed"/>
                                    </ObjectAnimationUsingKeyFrames>
                                    <ObjectAnimationUsingKeyFrames Storyboard.TargetName="EditView" Storyboard.TargetProperty="Visibility">
                                        <DiscreteObjectKeyFrame KeyTime="0:0:0" Value="Visible"/>
                                    </ObjectAnimationUsingKeyFrames>
                                </Storyboard>
                            </VisualState>
                        </VisualStateGroup>
                    </VisualStateManager.VisualStateGroups>

                    <!-- ERROR TRIGGER -->
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsValid" Value="False">
                            <Setter TargetName="ErrorText" Property="Visibility" Value="Visible"/>
                        </Trigger>

                        <!-- Kind switchers -->
                        <Trigger Property="Kind" Value="Text">
                            <Setter TargetName="PART_TextBox" Property="Visibility" Value="Visible"/>
                        </Trigger>
                        <Trigger Property="Kind" Value="Number">
                            <Setter TargetName="PART_TextBox" Property="Visibility" Value="Visible"/>
                        </Trigger>
                        <Trigger Property="Kind" Value="Password">
                            <Setter TargetName="PART_Password" Property="Visibility" Value="Visible"/>
                        </Trigger>
                        <Trigger Property="Kind" Value="Date">
                            <Setter TargetName="PART_DatePicker" Property="Visibility" Value="Visible"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

</ResourceDictionary>
*/
