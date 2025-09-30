using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Data;

namespace HYSoft.Presentation.Interactivity.CommandBehaviors
{
    /// <summary>
    /// 특정 RoutedEvent와 ICommand를 바인딩하기 위한 정의를 제공합니다.
    /// - 값 직접 지정: Command / CommandParameter
    /// - 바인딩 지정:  CommandBinding / CommandParameterBinding (BindingBase)
    /// 를 분리 보관합니다.
    /// </summary>
    [XamlSetMarkupExtension("ReceiveMarkupExtension")]
    [XamlSetTypeConverter("ReceiveTypeConverter")]
    public class Event : Freezable
    {
        // ===== RoutedEvent / HandledToo =====================================
        public static readonly DependencyProperty RoutedEventProperty =
            DependencyProperty.Register(nameof(RoutedEvent), typeof(RoutedEvent), typeof(Event));

        public static readonly DependencyProperty HandledTooProperty =
            DependencyProperty.Register(nameof(HandledToo), typeof(bool), typeof(Event), new PropertyMetadata(false));

        public RoutedEvent RoutedEvent
        {
            get => (RoutedEvent)GetValue(RoutedEventProperty);
            set => SetValue(RoutedEventProperty, value);
        }

        public bool HandledToo
        {
            get => (bool)GetValue(HandledTooProperty);
            set => SetValue(HandledTooProperty, value);
        }

        // ===== 값 직접 지정용 (optional) =====================================
        // Condition의 Value에 해당하는 "직접 값" 경로
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(Event));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(Event));

        /// <summary>값을 직접 지정하는 경우 사용 (Binding이 아닌 경우)</summary>
        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        /// <summary>값을 직접 지정하는 경우 사용 (Binding이 아닌 경우)</summary>
        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        // ===== 바인딩 지정용 (Condition.Binding처럼 BindingBase 보관) =========
        public static readonly DependencyProperty CommandBindingProperty =
            DependencyProperty.Register(nameof(CommandBinding), typeof(BindingBase), typeof(Event));

        public static readonly DependencyProperty CommandParameterBindingProperty =
            DependencyProperty.Register(nameof(CommandParameterBinding), typeof(BindingBase), typeof(Event));

        /// <summary>
        /// {Binding}/{MultiBinding}/{PriorityBinding} 등 바인딩을 그대로 보관합니다.
        /// 실제 적용은 EventToCommand.Attach 단계에서 수행하세요.
        /// </summary>
        public BindingBase CommandBinding
        {
            get => (BindingBase)GetValue(CommandBindingProperty);
            set => SetValue(CommandBindingProperty, value);
        }

        /// <summary>
        /// CommandParameter용 바인딩을 보관합니다.
        /// </summary>
        public BindingBase CommandParameterBinding
        {
            get => (BindingBase)GetValue(CommandParameterBindingProperty);
            set => SetValue(CommandParameterBindingProperty, value);
        }

        // ===== XAML 훅: Binding/TypeConverter를 *Binding 속성에 흡수 ==========
        // MultiDataTrigger.Condition처럼 property-element 구문을 지원하기 위해,
        // Command/CommandParameter에 마크업 확장이 들어오면 *Binding에 담아둡니다.
        public static void ReceiveMarkupExtension(object target, XamlSetMarkupExtensionEventArgs e)
        {
            if (target is not Event ev || e.Member == null)
            {
                e.CallBase();
                return;
            }

            // Binding / MultiBinding / PriorityBinding만 가로채기
            if (e.MarkupExtension is not BindingBase bb)
            {
                e.CallBase();
                return;
            }

            switch (e.Member.Name)
            {
                case nameof(Command):
                case nameof(CommandBinding): // 둘 다 허용
                    ev.CommandBinding = bb;
                    e.Handled = true;
                    return;

                case nameof(CommandParameter):
                case nameof(CommandParameterBinding):
                    ev.CommandParameterBinding = bb;
                    e.Handled = true;
                    return;

                default:
                    e.CallBase();
                    return;
            }
        }

        public static void ReceiveTypeConverter(object target, XamlSetTypeConverterEventArgs e)
        {
            // 문자열 → 객체 변환은 기본 동작 유지
            // (특별한 커스텀 파싱이 필요하면 여기서 처리 가능)
            e.CallBase();
        }

        // ===== Freezable =====================================================
        protected override Freezable CreateInstanceCore() => new Event();
    }
}
