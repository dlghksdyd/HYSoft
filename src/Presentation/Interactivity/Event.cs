using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HYSoft.Presentation.Interactivity
{
    /// <summary>
    /// 특정 RoutedEvent와 ICommand를 바인딩하기 위한 정의를 제공합니다.
    /// </summary>
    public class Event : Freezable
    {
        /// <summary>
        /// 바인딩할 RoutedEvent를 나타내는 DependencyProperty입니다.
        /// </summary>
        public static readonly DependencyProperty RoutedEventProperty =
            DependencyProperty.Register(nameof(RoutedEvent), typeof(RoutedEvent), typeof(Event));

        /// <summary>
        /// 실행할 ICommand를 나타내는 DependencyProperty입니다.
        /// </summary>
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(Event));

        /// <summary>
        /// 명령 실행 시 전달할 추가 파라미터를 나타내는 DependencyProperty입니다.
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(Event));

        public static readonly DependencyProperty HandledTooProperty =
            DependencyProperty.Register(
                nameof(HandledToo),
                typeof(bool),
                typeof(Event),
                new PropertyMetadata(false));

        /// <summary>
        /// 바인딩할 RoutedEvent를 가져오거나 설정합니다.
        /// </summary>
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

        /// <summary>
        /// 바인딩할 ICommand를 가져오거나 설정합니다.
        /// </summary>
        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        /// <summary>
        /// ICommand 실행 시 전달할 매개변수를 가져오거나 설정합니다.
        /// </summary>
        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        /// <inheritdoc/>
        protected override Freezable CreateInstanceCore() => new Event();
    }
}
