using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HYSoft.Presentation.Styles.Controls
{
    public class HyBreadcrumb : ItemsControl
    {
        static HyBreadcrumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyBreadcrumb), new FrameworkPropertyMetadata(typeof(HyBreadcrumb)));
        }

        public static readonly DependencyProperty SeparatorProperty =
            DependencyProperty.Register(
                nameof(Separator),
                typeof(string),
                typeof(HyBreadcrumb),
                new FrameworkPropertyMetadata(">"));

        public string Separator
        {
            get => (string)GetValue(SeparatorProperty);
            set => SetValue(SeparatorProperty, value);
        }

        public static readonly DependencyProperty SeparatorForegroundProperty =
            DependencyProperty.Register(
                nameof(SeparatorForeground),
                typeof(Brush),
                typeof(HyBreadcrumb),
                new FrameworkPropertyMetadata(null));

        public Brush SeparatorForeground
        {
            get => (Brush)GetValue(SeparatorForegroundProperty);
            set => SetValue(SeparatorForegroundProperty, value);
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is HyBreadcrumbItem;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new HyBreadcrumbItem();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            UpdateIsLastFlags();
        }

        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            UpdateIsLastFlags();
        }

        private void UpdateIsLastFlags()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (ItemContainerGenerator.ContainerFromIndex(i) is HyBreadcrumbItem container)
                {
                    container.IsLast = (i == Items.Count - 1);
                }
            }
        }
    }

    public class HyBreadcrumbItem : ContentControl
    {
        static HyBreadcrumbItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyBreadcrumbItem), new FrameworkPropertyMetadata(typeof(HyBreadcrumbItem)));
        }

        public static readonly DependencyProperty IsLastProperty =
            DependencyProperty.Register(
                nameof(IsLast),
                typeof(bool),
                typeof(HyBreadcrumbItem),
                new FrameworkPropertyMetadata(false));

        public bool IsLast
        {
            get => (bool)GetValue(IsLastProperty);
            set => SetValue(IsLastProperty, value);
        }
    }
}
