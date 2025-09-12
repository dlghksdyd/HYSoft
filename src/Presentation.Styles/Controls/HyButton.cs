using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HYSoft.Presentation.Styles.Controls
{
    public class HyButton : Button
    {
        public static readonly DependencyProperty CornerRadiusProperty;

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        static HyButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyButton), new FrameworkPropertyMetadata(typeof(HyButton)));
            CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(HyButton), new FrameworkPropertyMetadata());
        }
    }
}
