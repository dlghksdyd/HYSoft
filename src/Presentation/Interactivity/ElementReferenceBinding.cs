using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MLib.Mvvm
{
    /// <summary>
    /// <see cref="UIElement"/> (또는 <see cref="FrameworkElement"/>) 자신의 참조를
    /// 바인딩된 ViewModel 속성에 주입할 수 있도록 지원하는 Attached Property를 제공합니다.
    /// </summary>
    /// <remarks>
    /// MVVM 패턴에서 ViewModel이 특정 UI 요소(예: <see cref="WeakReference{T}"/> 또는 직접 <see cref="UIElement"/>)
    /// 에 대한 참조를 가져야 할 때 사용할 수 있습니다.
    /// View와 ViewModel 간의 분리 원칙을 유지하면서도, 필요한 경우 뷰 요소를 안전하게 전달할 수 있습니다.
    ///
    /// 사용 예:
    /// <code language="xml">
    /// &lt;Grid
    ///     xmlns:mvvm="clr-namespace:MLib.Mvvm;assembly=MLib.Mvvm"
    ///     mvvm:ElementReferenceBinding.Target="{Binding DragScope, Mode=TwoWay}" /&gt;
    /// </code>
    ///
    /// ViewModel 예:
    /// <code language="csharp">
    /// public UIElement DragScope { get; set; }
    /// // 또는
    /// public WeakReference&lt;UIElement&gt; DragScope { get; set; }
    /// </code>
    /// </remarks>
    public static class ElementReferenceBinding
    {
        private static readonly DependencyProperty IsSubscribedProperty =
            DependencyProperty.RegisterAttached(
                "IsSubscribed", typeof(bool), typeof(ElementReferenceBinding), new PropertyMetadata(false));

        static ElementReferenceBinding()
        {
            EventManager.RegisterClassHandler(
                typeof(FrameworkElement),
                FrameworkElement.LoadedEvent,
                new RoutedEventHandler(OnAnyLoaded));

            EventManager.RegisterClassHandler(
                typeof(FrameworkElement),
                FrameworkElement.UnloadedEvent,
                new RoutedEventHandler(OnAnyUnloaded));
        }

        /// <summary>
        /// <see cref="TargetProperty"/> Attached Property를 식별합니다.
        /// 이 프로퍼티는 대상 UI 요소의 참조를 ViewModel의 속성에 바인딩할 때 사용됩니다.
        /// </summary>
        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.RegisterAttached(
                "Target", typeof(object), typeof(ElementReferenceBinding),
                new PropertyMetadata(null, OnTargetChanged));
        
        /// <summary>
        /// 지정된 <see cref="DependencyObject"/>에 Target 바인딩 값을 설정합니다.
        /// </summary>
        /// <param name="d">속성을 설정할 대상 객체</param>
        /// <param name="value">바인딩된 값(일반적으로 바인딩 엔진에 의해 제공됨)</param>
        public static void SetTarget(DependencyObject d, object value)
            => d.SetValue(TargetProperty, value);

        /// <summary>
        /// 지정된 <see cref="DependencyObject"/>에서 Target 바인딩 값을 가져옵니다.
        /// </summary>
        /// <param name="d">속성을 읽어올 대상 객체</param>
        /// <returns><see cref="TargetProperty"/>의 현재 값</returns>
        public static object GetTarget(DependencyObject d)
            => d.GetValue(TargetProperty);

        // 바인딩 값이 변할 때(초기 null 포함) → 소스 프로퍼티에 UI 자신을 주입
        private static void OnTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not UIElement ui) return;

            var expr = BindingOperations.GetBindingExpression(d, TargetProperty);
            if (expr == null) return; // 바인딩이 아니면 패스

            TryPushSelfToSource(expr, ui);
        }

        private static void OnAnyLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement fe) return;

            var be = BindingOperations.GetBinding(fe, TargetProperty);
            if (be == null) return;
            if (be.Mode != BindingMode.OneWay && be.Mode != BindingMode.TwoWay) return;

            // Loaded 시점에도 한 번 더 시도 (초기 DataContext null 보완)
            var expr = BindingOperations.GetBindingExpression(fe, TargetProperty);
            if (expr != null)
                TryPushSelfToSource(expr, fe);

            // DataContext 나중 세팅/교체 대응
            if (!(bool)fe.GetValue(IsSubscribedProperty))
            {
                fe.DataContextChanged += Fe_DataContextChanged;
                fe.SetValue(IsSubscribedProperty, true);
            }
        }

        private static void OnAnyUnloaded(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement fe) return;

            if ((bool)fe.GetValue(IsSubscribedProperty))
            {
                fe.DataContextChanged -= Fe_DataContextChanged;
                fe.SetValue(IsSubscribedProperty, false);
            }
        }

        private static void Fe_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not FrameworkElement fe) return;

            var be = BindingOperations.GetBinding(fe, TargetProperty);
            if (be == null) return;
            if (be.Mode != BindingMode.OneWay && be.Mode != BindingMode.TwoWay) return;

            var expr = BindingOperations.GetBindingExpression(fe, TargetProperty);
            if (expr != null)
                TryPushSelfToSource(expr, fe);
        }

        // === 핵심: 바인딩 소스 객체의 해당 프로퍼티에 UIElement 자신을 넣는다 ===
        private static void TryPushSelfToSource(BindingExpression expr, UIElement self)
        {
            // 소스 객체(DragDataContext 등)와 최종 프로퍼티명(DragScope)을 얻는다.
            object source = expr.ResolvedSource;
            string propName = expr.ResolvedSourcePropertyName;

            if (source == null || string.IsNullOrEmpty(propName)) return;

            var srcType = source.GetType();
            var prop = srcType.GetProperty(propName, BindingFlags.Instance | BindingFlags.Public);
            if (prop == null || !prop.CanWrite) return;

            var targetType = prop.PropertyType;

            // 타입에 맞춰 주입: UIElement/FrameworkElement/object/WeakReference<UIElement> 등을 지원
            object valueToSet = null;

            if (targetType.IsAssignableFrom(self.GetType()))
            {
                valueToSet = self; // UIElement/FrameworkElement/object 등
            }
            else if (targetType.IsGenericType &&
                     targetType.GetGenericTypeDefinition() == typeof(WeakReference<>) &&
                     targetType.GetGenericArguments()[0].IsAssignableFrom(self.GetType()))
            {
                // WeakReference<UIElement> 같은 경우
                var weakRefCtor = targetType.GetConstructor(new[] { targetType.GetGenericArguments()[0] });
                valueToSet = weakRefCtor?.Invoke(new object[] { self });
            }
            else if (targetType == typeof(object))
            {
                valueToSet = self;
            }
            else
            {
                // 필요시 더 많은 케이스(예: string = Name 등) 확장 가능
                return;
            }

            try
            {
                prop.SetValue(source, valueToSet);
            }
            catch (Exception)
            {
                // 무시 (쓰기 실패 시 조용히 패스)
            }
        }
    }
}
