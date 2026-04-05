using System.Windows;
using System.Windows.Controls;

namespace HYSoft.Presentation.Styles.Controls
{
    public class HyDialog : Window
    {
        static HyDialog()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyDialog), new FrameworkPropertyMetadata(typeof(HyDialog)));
        }

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(HyDialog),
                new PropertyMetadata(new CornerRadius(8)));

        public string DialogTitle
        {
            get => (string)GetValue(DialogTitleProperty);
            set => SetValue(DialogTitleProperty, value);
        }
        public static readonly DependencyProperty DialogTitleProperty =
            DependencyProperty.Register(nameof(DialogTitle), typeof(string), typeof(HyDialog),
                new PropertyMetadata(null));

        public string DialogMessage
        {
            get => (string)GetValue(DialogMessageProperty);
            set => SetValue(DialogMessageProperty, value);
        }
        public static readonly DependencyProperty DialogMessageProperty =
            DependencyProperty.Register(nameof(DialogMessage), typeof(string), typeof(HyDialog),
                new PropertyMetadata(null));

        public new bool? DialogResult
        {
            get => (bool?)GetValue(DialogResultProperty);
            set => SetValue(DialogResultProperty, value);
        }
        public static readonly DependencyProperty DialogResultProperty =
            DependencyProperty.Register(nameof(DialogResult), typeof(bool?), typeof(HyDialog),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string ConfirmButtonText
        {
            get => (string)GetValue(ConfirmButtonTextProperty);
            set => SetValue(ConfirmButtonTextProperty, value);
        }
        public static readonly DependencyProperty ConfirmButtonTextProperty =
            DependencyProperty.Register(nameof(ConfirmButtonText), typeof(string), typeof(HyDialog),
                new PropertyMetadata("OK"));

        public string CancelButtonText
        {
            get => (string)GetValue(CancelButtonTextProperty);
            set => SetValue(CancelButtonTextProperty, value);
        }
        public static readonly DependencyProperty CancelButtonTextProperty =
            DependencyProperty.Register(nameof(CancelButtonText), typeof(string), typeof(HyDialog),
                new PropertyMetadata("Cancel"));

        public bool ShowCancelButton
        {
            get => (bool)GetValue(ShowCancelButtonProperty);
            set => SetValue(ShowCancelButtonProperty, value);
        }
        public static readonly DependencyProperty ShowCancelButtonProperty =
            DependencyProperty.Register(nameof(ShowCancelButton), typeof(bool), typeof(HyDialog),
                new PropertyMetadata(true));

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("PART_ConfirmButton") is Button confirmButton)
            {
                confirmButton.Click += (s, e) =>
                {
                    DialogResult = true;
                    Close();
                };
            }

            if (GetTemplateChild("PART_CancelButton") is Button cancelButton)
            {
                cancelButton.Click += (s, e) =>
                {
                    DialogResult = false;
                    Close();
                };
            }
        }
    }
}
