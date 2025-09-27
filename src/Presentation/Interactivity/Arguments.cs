using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HYSoft.Presentation.Interactivity
{
    public static class Argument1
    {
        public static DependencyProperty ValueProperty =
            DependencyProperty.RegisterAttached(
                "Value",
                typeof(object),
                typeof(Argument1),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        public static void SetValue(DependencyObject d, object v) => d.SetValue(ValueProperty, v);
        public static object GetValue(DependencyObject d) => d.GetValue(ValueProperty);
    }

    public static class Argument2
    {
        public static DependencyProperty ValueProperty =
            DependencyProperty.RegisterAttached(
                "Value",
                typeof(object),
                typeof(Argument2),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        public static void SetValue(DependencyObject d, object v) => d.SetValue(ValueProperty, v);
        public static object GetValue(DependencyObject d) => d.GetValue(ValueProperty);
    }

    public static class Argument3
    {
        public static DependencyProperty ValueProperty =
            DependencyProperty.RegisterAttached(
                "Value",
                typeof(object),
                typeof(Argument3),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        public static void SetValue(DependencyObject d, object v) => d.SetValue(ValueProperty, v);
        public static object GetValue(DependencyObject d) => d.GetValue(ValueProperty);
    }

    public static class Argument4
    {
        public static DependencyProperty ValueProperty =
            DependencyProperty.RegisterAttached(
                "Value",
                typeof(object),
                typeof(Argument4),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        public static void SetValue(DependencyObject d, object v) => d.SetValue(ValueProperty, v);
        public static object GetValue(DependencyObject d) => d.GetValue(ValueProperty);
    }
}
