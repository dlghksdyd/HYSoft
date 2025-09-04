using System;
using System.Windows;
using System.Windows.Media;

namespace HYSoft.Presentation.ElementTreeHelper
{
    /// <summary>
    /// WPF의 시각 트리(VisualTree) 및 논리 트리(LogicalTree)에서
    /// 특정 조건에 맞는 요소를 탐색하기 위한 유틸리티 메서드를 제공합니다.
    /// </summary>
    public static class TreeSearchHelper
    {
        /// <summary>
        /// 주어진 요소에서 시작하여, 특정 타입의 조상 요소를 찾습니다.
        /// </summary>
        /// <typeparam name="T">찾고자 하는 조상 요소의 타입.</typeparam>
        /// <param name="current">탐색을 시작할 자식 요소.</param>
        /// <returns>
        /// 조상 중 첫 번째로 발견된 <typeparamref name="T"/> 타입의 요소.
        /// 발견되지 않으면 <c>null</c>.
        /// </returns>
        /// <remarks>
        /// 일반적으로 컨트롤 템플릿 내부에서 외부 컨트롤을 찾을 때 유용합니다.
        /// </remarks>
        public static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T t) return t;
                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }

        /// <summary>
        /// 주어진 요소에서 시작하여, 특정 타입의 자식 요소를 재귀적으로 찾습니다.
        /// </summary>
        /// <typeparam name="T">찾고자 하는 자식 요소의 타입.</typeparam>
        /// <param name="current">탐색을 시작할 부모 요소.</param>
        /// <returns>
        /// 자식 트리에서 발견된 첫 번째 <typeparamref name="T"/> 타입의 요소.
        /// 발견되지 않으면 <c>null</c>.
        /// </returns>
        /// <remarks>
        /// 트리 전체를 탐색하므로, 큰 시각 트리에서는 성능에 유의해야 합니다.
        /// </remarks>
        public static T? FindChild<T>(DependencyObject? current) where T : DependencyObject
        {
            if (current == null) return null;
            
            var count = VisualTreeHelper.GetChildrenCount(current);

            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(current, i);
                if (child is T t) return t;
                var result = FindChild<T>(child);
                if (result != null) return result;
            }

            return null;
        }

        /// <summary>
        /// 주어진 요소와 동일한 DataContext 인스턴스를 공유하는 조상들 중
        /// 가장 바깥(최상위)의 <see cref="FrameworkElement"/>를 찾습니다.
        /// </summary>
        /// <typeparam name="T">반환받길 원하는 요소 타입(맞지 않으면 null).</typeparam>
        /// <param name="current">시작 요소(템플릿 내부의 아무 자식).</param>
        /// <param name="dataContextType">시작 요소의 DataContext 타입.</param>
        /// <returns>
        /// 동일한 DataContext를 유지하다가 변경되기 직전의 최상위 요소.
        /// <typeparamref name="T"/>로 캐스팅 가능한 경우 해당 요소를 반환하고,
        /// 없거나 타입 불일치 시 <c>null</c>.
        /// </returns>
        /// <remarks>
        /// MVVM 패턴에서 컨트롤 템플릿 내부의 자식 요소로부터
        /// 외부 뷰의 루트 컨트롤을 찾아야 할 때 유용합니다.
        /// </remarks>
        public static T? FindTopElementSharingDataContext<T>(FrameworkElement? current, Type? dataContextType) where T : DependencyObject
        {
            if (current == null || dataContextType == null) return null;

            var dc = current.DataContext;

            if (dc == null || !dataContextType.IsInstanceOfType(dc))
                return null;

            DependencyObject node = current;
            var lastWithSameDc = current;

            while (true)
            {
                // Visual → FrameworkElement.Parent → Logical 순으로 부모 탐색
                var parent = VisualTreeHelper.GetParent(node) ?? (node as FrameworkElement)?.Parent ?? LogicalTreeHelper.GetParent(node);

                if (parent != null)
                {
                    if (parent is FrameworkElement feParent)
                    {
                        if (!ReferenceEquals(feParent.DataContext, dc))
                        {
                            // 여기서 DC가 달라졌으니, 직전 요소가 템플릿 루트
                            return lastWithSameDc as T;
                        }

                        // 동일 DC 유지 → 갱신
                        lastWithSameDc = feParent;
                    }

                    node = parent; // 계속 위로
                }
                else
                {
                    // 더 이상 부모가 없으면, 동일 DC를 유지한 최상위 요소 반환
                    return lastWithSameDc as T;
                }
            }
        }
    }
}
