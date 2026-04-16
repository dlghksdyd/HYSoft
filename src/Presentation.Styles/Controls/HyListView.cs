using System.Windows;
using System.Windows.Controls;

namespace HYSoft.Presentation.Styles.Controls
{
    /// <summary>
    /// HyListView — ListView 래퍼. HyListViewItem 컨테이너를 생성합니다.
    /// </summary>
    public class HyListView : ListView
    {
        static HyListView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyListView), new FrameworkPropertyMetadata(typeof(HyListView)));
        }

        protected override DependencyObject GetContainerForItemOverride() => new HyListViewItem();

        protected override bool IsItemItsOwnContainerOverride(object item) => item is HyListViewItem;

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(HyListView),
                new FrameworkPropertyMetadata(new CornerRadius(0)));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
    }

    /// <summary>
    /// HyListViewItem — ListViewItem 래퍼. IsMouseOver/IsSelected 시 배경색 하이라이트 제공.
    /// </summary>
    public class HyListViewItem : ListViewItem
    {
        static HyListViewItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyListViewItem), new FrameworkPropertyMetadata(typeof(HyListViewItem)));
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(HyListViewItem),
                new FrameworkPropertyMetadata(new CornerRadius(0)));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
    }
}
