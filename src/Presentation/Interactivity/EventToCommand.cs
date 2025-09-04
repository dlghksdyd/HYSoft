using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace HYSoft.Presentation.Interactivity
{
    /// <summary>
    /// RoutedEvent와 ICommand를 연결할 수 있도록 지원하는 Attached Behavior입니다.
    /// </summary>
    public static class EventToCommand
    {
        /// <summary>
        /// UIElement에 바인딩할 EventCollection을 나타내는 Attached Property입니다.
        /// </summary>
        public static readonly DependencyProperty BindingsProperty =
            DependencyProperty.RegisterAttached(
                "Bindings",
                typeof(EventCollection),
                typeof(EventToCommand),
                new PropertyMetadata(null, OnBindingsChanged));

        /// <summary>
        /// 지정된 DependencyObject에 EventCollection을 설정합니다.
        /// </summary>
        /// <param name="d">대상 객체</param>
        /// <param name="value">바인딩할 EventCollection</param>
        public static void SetBindings(DependencyObject d, EventCollection value) => d.SetValue(BindingsProperty, value);

        /// <summary>
        /// 지정된 DependencyObject에서 EventCollection을 가져옵니다.
        /// </summary>
        /// <param name="d">대상 객체</param>
        /// <returns>EventCollection</returns>
        public static EventCollection GetBindings(DependencyObject d) => (EventCollection)d.GetValue(BindingsProperty);

        private static readonly DependencyProperty HandlerMapProperty =
            DependencyProperty.RegisterAttached("HandlerMap", typeof(Dictionary<Event, Delegate>), typeof(EventToCommand));
        private static Dictionary<Event, Delegate> GetHandlerMap(DependencyObject d)
            => (Dictionary<Event, Delegate>)d.GetValue(HandlerMapProperty);
        private static void SetHandlerMap(DependencyObject d, Dictionary<Event, Delegate> map)
            => d.SetValue(HandlerMapProperty, map);

        private static readonly DependencyProperty CollectionChangedHandlerProperty =
            DependencyProperty.RegisterAttached(
                "CollectionChangedHandler",
                typeof(NotifyCollectionChangedEventHandler),
                typeof(EventToCommand),
                new PropertyMetadata(null));

        private static void OnBindingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not FrameworkElement ui) return;

            // 이전 것들 Detach
            DetachAll(ui);

            // 이전 컬렉션 이벤트 해제
            if (e.OldValue is INotifyCollectionChanged oldIncc &&
                d.GetValue(CollectionChangedHandlerProperty) is NotifyCollectionChangedEventHandler oldHandler)
            {
                oldIncc.CollectionChanged -= oldHandler;
                d.ClearValue(CollectionChangedHandlerProperty);
            }

            if (e.NewValue is EventCollection coll)
            {
                // ★ 명시적 인터페이스 구현 → 캐스팅 필요
                var incc = (INotifyCollectionChanged)coll;

                NotifyCollectionChangedEventHandler handler = (s, args) =>
                {
                    if (args.Action == NotifyCollectionChangedAction.Reset)
                    {
                        DetachAll(ui);
                        if (coll != null)
                            foreach (var b in coll)
                                Attach(ui, b);
                        return;
                    }
                    if (args.OldItems != null)
                        foreach (Event b in args.OldItems) Detach(ui, b);
                    if (args.NewItems != null)
                        foreach (Event b in args.NewItems) Attach(ui, b);
                };

                incc.CollectionChanged += handler;
                d.SetValue(CollectionChangedHandlerProperty, handler);

                // 초기 attach
                foreach (var b in coll) Attach(ui, b);
            }
        }

        private static void Attach(UIElement ui, Event b)
        {
            if (b?.RoutedEvent == null || b.Command == null) return;

            // 1) 맵 확보
            var map = GetHandlerMap(ui);
            if (map == null)
            {
                map = new Dictionary<Event, Delegate>();
                SetHandlerMap(ui, map);
            }

            // 2) 이미 같은 Event에 등록되어 있으면 중복 방지
            if (map.ContainsKey(b)) return;

            // 3) 핸들러 생성
            RoutedEventHandler handler = (s, e) =>
            {
                object param = new EventPayload(s, e, b.CommandParameter);
                if (b.Command.CanExecute(param))
                    b.Command.Execute(param);
            };

            // 4) UI에 add + 맵에 저장
            ui.AddHandler(b.RoutedEvent, handler, handledEventsToo: true);
            map[b] = handler;
        }

        private static void Detach(UIElement ui, Event b)
        {
            if (b == null) return;
            var map = GetHandlerMap(ui);
            if (map != null && map.TryGetValue(b, out var handler) && b.RoutedEvent != null)
            {
                ui.RemoveHandler(b.RoutedEvent, handler);
                map.Remove(b);
            }
        }

        private static void DetachAll(UIElement ui)
        {
            var map = GetHandlerMap(ui);
            if (map == null) return;

            // 사본을 돌면서 안전하게 제거
            foreach (var kv in map.ToArray())
                if (kv.Key?.RoutedEvent != null)
                    ui.RemoveHandler(kv.Key.RoutedEvent, kv.Value);

            map.Clear();
        }
    }
}
