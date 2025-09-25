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
        /// (여러 Event를 한 번에 부착)
        /// </summary>
        public static readonly DependencyProperty MultiBindingProperty =
            DependencyProperty.RegisterAttached(
                "MultiBinding",
                typeof(EventCollection),
                typeof(EventToCommand),
                new PropertyMetadata(null, OnBindingsChanged));

        public static void SetMultiBinding(DependencyObject d, EventCollection value) => d.SetValue(MultiBindingProperty, value);
        public static EventCollection GetMultiBinding(DependencyObject d) => (EventCollection)d.GetValue(MultiBindingProperty);

        /// <summary>
        /// 단일 Event 바인딩용 Attached Property
        /// </summary>
        public static readonly DependencyProperty BindingProperty =
            DependencyProperty.RegisterAttached(
                "Binding",
                typeof(Event),
                typeof(EventToCommand),
                new PropertyMetadata(null, OnBindingChanged));

        public static void SetBinding(DependencyObject d, Event value) => d.SetValue(BindingProperty, value);
        public static Event GetBinding(DependencyObject d) => (Event)d.GetValue(BindingProperty);

        // 내부: UIElement별로 부착된 Event→Delegate 매핑 저장
        private static readonly DependencyProperty HandlerMapProperty =
            DependencyProperty.RegisterAttached("HandlerMap", typeof(Dictionary<Event, Delegate>), typeof(EventToCommand));
        private static Dictionary<Event, Delegate> GetHandlerMap(DependencyObject d)
            => (Dictionary<Event, Delegate>)d.GetValue(HandlerMapProperty);
        private static void SetHandlerMap(DependencyObject d, Dictionary<Event, Delegate> map)
            => d.SetValue(HandlerMapProperty, map);

        private static readonly DependencyProperty ExecutingSetProperty =
            DependencyProperty.RegisterAttached("ExecutingSet",
                typeof(HashSet<Event>), typeof(EventToCommand));
        private static HashSet<Event> GetExecutingSet(DependencyObject d) =>
            (HashSet<Event>)(d.GetValue(ExecutingSetProperty) ?? new HashSet<Event>());
        private static void SetExecutingSet(DependencyObject d, HashSet<Event> set) =>
            d.SetValue(ExecutingSetProperty, set);

        // MultiBindings(INotifyCollectionChanged) 구독 핸들러 저장
        private static readonly DependencyProperty CollectionChangedHandlerProperty =
            DependencyProperty.RegisterAttached(
                "CollectionChangedHandler",
                typeof(NotifyCollectionChangedEventHandler),
                typeof(EventToCommand),
                new PropertyMetadata(null));

        // ==== MultiBindings 변경 처리 ====
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
                var incc = (INotifyCollectionChanged)coll;

                NotifyCollectionChangedEventHandler handler = (s, args) =>
                {
                    if (args.Action == NotifyCollectionChangedAction.Reset)
                    {
                        DetachAll(ui);
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

        // ==== 단일 Binding 변경 처리 ====
        private static void OnBindingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not UIElement ui) return;

            // 이전 단일 바인딩 Detach
            if (e.OldValue is Event oldEv) Detach(ui, oldEv);

            // 새 단일 바인딩 Attach
            if (e.NewValue is Event newEv) Attach(ui, newEv);
        }

        // ==== attach/detach 공통 로직 ====
        private static void Attach(UIElement ui, Event b)
        {
            if (b?.RoutedEvent == null || b.Command == null) return;

            var map = GetHandlerMap(ui);
            if (map == null)
            {
                map = new Dictionary<Event, Delegate>();
                SetHandlerMap(ui, map);
            }
            if (map.ContainsKey(b)) return;

            // 2) 안전한 핸들러
            RoutedEventHandler handler = (s, e) =>
            {
                var executing = GetExecutingSet(ui);
                if (executing.Contains(b)) return; // 재진입 차단
                executing.Add(b);
                try
                {
                    object param = new EventPayload(s, e, b.CommandParameter);

                    if (b.Command is RoutedCommand rc)
                    {
                        if (!rc.CanExecute(param, ui)) return;
                        rc.Execute(param, ui);
                    }
                    else
                    {
                        if (!b.Command.CanExecute(param)) return;

                        b.Command.Execute(param);
                    }
                }
                finally
                {
                    executing.Remove(b);
                }
            };

            bool handledToo = true;
            ui.AddHandler(b.RoutedEvent, handler, handledToo);

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
