using System.Windows;
using System.Windows.Controls;

namespace HYSoft.Presentation.Styles.Controls
{
    public class HyTreeView : TreeView
    {
        static HyTreeView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyTreeView), new FrameworkPropertyMetadata(typeof(HyTreeView)));
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(HyTreeView),
                new FrameworkPropertyMetadata(new CornerRadius(0)));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
    }

    public class HyTreeViewItem : TreeViewItem
    {
        static HyTreeViewItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyTreeViewItem), new FrameworkPropertyMetadata(typeof(HyTreeViewItem)));
        }
    }
}
