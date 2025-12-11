using HYSoft.Presentation.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace HYSoft.Presentation.Interactivity.CommandBehaviors
{
    /// <summary>
    /// RoutedEvent와 ICommand를 연결할 수 있도록 지원하는 Attached Behavior입니다.
    /// - 단일 바인딩: EventToCommand.Binding
    /// - 다중 바인딩: EventToCommand.MultiBinding (EventCollection)
    /// 
    /// Event는 MultiDataTrigger.Condition 스타일로 바뀌어
    /// Command/CommandParameter "직접 값"과
    /// CommandBinding/CommandParameterBinding(BindingBase) 를 분리 보관합니다.
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

        // 요소 Unloaded 훅 등록 여부
        private static readonly DependencyProperty CleanupHookedProperty =
            DependencyProperty.RegisterAttached(
                "CleanupHooked",
                typeof(bool),
                typeof(EventToCommand),
                new PropertyMetadata(false));

        private static bool GetCleanupHooked(DependencyObject d) => (bool)d.GetValue(CleanupHookedProperty);
        private static void SetCleanupHooked(DependencyObject d, bool value) => d.SetValue(CleanupHookedProperty, value);

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

                EnsureCleanupHook(ui);
            }
            else
            {
                EnsureCleanupHook(ui);
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

            if (ui is FrameworkElement fe)
                EnsureCleanupHook(fe);
        }

        // ==== attach/detach 공통 로직 ====
        private static void Attach(UIElement ui, Event b)
        {
            if (b?.RoutedEvent == null) return;

            // 요소별 인스턴스를 갖도록 클론 (Freezable이니 안전)
            var ev = (Event)b.Clone();

            // === 1) Command/Parameter 바인딩 적용 ===
            // Event는 BindingBase를 저장만 하므로 실제 바인딩을 ev의 DP에 단다.
            if (ev.CommandBinding is BindingBase cb)
            {
                BindingOperations.SetBinding(ev, Event.CommandProperty, cb);
                // RelativeSource/ElementName를 ui 기준으로 재해석
                BindingManager.SetBindingSource(ui, ev, Event.CommandProperty);
            }

            if (ev.CommandParameterBinding is BindingBase pb)
            {
                BindingOperations.SetBinding(ev, Event.CommandParameterProperty, pb);
                BindingManager.SetBindingSource(ui, ev, Event.CommandParameterProperty);
            }

            // 직접 값만 쓴 경우(ev.Command == null일 수 있음) MultiBinding에서도
            // 이후 데이터 갱신으로 값이 들어올 수 있으니, 핸들러는 우선 붙여준다.
            var map = GetHandlerMap(ui);
            if (map == null)
            {
                map = new Dictionary<Event, Delegate>();
                SetHandlerMap(ui, map);
            }
            if (map.ContainsKey(b)) return;

            EnsureExecutingSet(ui);

            // === 2) 안전한 핸들러 ===
            // 실행 시점마다 ev.Command/CommandParameter의 현재 값을 사용
            RoutedEventHandler handler = (s, eArgs) =>
            {
                var executing = GetExecutingSet(ui);
                if (executing.Contains(ev)) return; // 재진입 차단 (ev 기준)
                executing.Add(ev);
                try
                {
                    var cmd = ev.Command;
                    if (cmd == null) return; // 아직 바인딩이 안 풀렸을 수도 있음

                    object param = new EventPayload(s, eArgs, ev.CommandParameter);

                    if (cmd is RoutedCommand rc)
                    {
                        if (!rc.CanExecute(param, ui)) return;
                        rc.Execute(param, ui);
                    }
                    else
                    {
                        if (!cmd.CanExecute(param)) return;
                        cmd.Execute(param);
                    }
                }
                finally
                {
                    executing.Remove(ev);
                }
            };

            bool handledToo = ev.HandledToo;
            ui.AddHandler(ev.RoutedEvent, handler, handledToo);

            // 원본 Event(b) ↔︎ 핸들러 저장 (Detach 키는 b 사용)
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

            // 바인딩으로 생성된 클론 인스턴스(ev)는 로컬로만 유지되므로
            // 여기서는 원본 b의 바인딩을 건 적이 없다. 다만 잠재적 참조를 줄이기 위해
            // 값-기반 속성은 정리해준다.
            b.ClearValue(Event.CommandProperty);
            b.ClearValue(Event.CommandParameterProperty);
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

        private static void EnsureExecutingSet(DependencyObject d)
        {
            var set = d.GetValue(ExecutingSetProperty) as HashSet<Event>;
            if (set == null)
            {
                set = new HashSet<Event>();
                SetExecutingSet(d, set);
            }
        }

        private static void EnsureCleanupHook(FrameworkElement ui)
        {
            if (GetCleanupHooked(ui)) return;

            // WeakEventManager를 사용하여 Unloaded/Loaded에 대한 핸들러를 약한 참조로 등록
            System.Windows.WeakEventManager<FrameworkElement, RoutedEventArgs>.AddHandler(ui, nameof(ui.Unloaded), OnElementUnloaded);
            System.Windows.WeakEventManager<FrameworkElement, RoutedEventArgs>.AddHandler(ui, nameof(ui.Loaded), OnElementLoaded);
            SetCleanupHooked(ui, true);
        }

        private static void OnElementUnloaded(object? sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement ui) return;

            // 모든 핸들러 제거
            DetachAll(ui);

            // 컬렉션 변경 핸들러만 해제 (AP 값은 유지하여 Loaded 시 재구축 가능하게 함)
            var handlerObj = ui.GetValue(CollectionChangedHandlerProperty) as NotifyCollectionChangedEventHandler;
            var coll = GetMultiBinding(ui) as System.Collections.Specialized.INotifyCollectionChanged;
            if (handlerObj != null && coll != null)
            {
                coll.CollectionChanged -= handlerObj;
                ui.ClearValue(CollectionChangedHandlerProperty);
            }

            // 내부 상태 정리 (맵/실행셋만 정리). AP는 유지.
            ui.ClearValue(HandlerMapProperty);
            ui.ClearValue(ExecutingSetProperty);
        }

        private static void OnElementLoaded(object? sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement ui) return;

            // 이미 초기 로딩 단계에서 컬렉션 구독이 설정되어 있으면 중복 구독 방지
            var existingCollectionHandler = ui.GetValue(CollectionChangedHandlerProperty) as NotifyCollectionChangedEventHandler;

            // 현재 AP 값을 바탕으로 재구축
            var multi = GetMultiBinding(ui);
            if (multi != null && existingCollectionHandler == null)
            {
                // 컬렉션 구독 복원 (중복 방지: existingCollectionHandler 체크)
                var incc = (INotifyCollectionChanged)multi;
                NotifyCollectionChangedEventHandler handler = (s2, args) =>
                {
                    if (args.Action == NotifyCollectionChangedAction.Reset)
                    {
                        DetachAll(ui);
                        foreach (var b in multi)
                            Attach(ui, b);
                        return;
                    }
                    if (args.OldItems != null)
                        foreach (Event b in args.OldItems) Detach(ui, b);
                    if (args.NewItems != null)
                        foreach (Event b in args.NewItems) Attach(ui, b);
                };

                incc.CollectionChanged += handler;
                ui.SetValue(CollectionChangedHandlerProperty, handler);

                // 핸들러 재부착: Attach 내부에서 map.ContainsKey로 중복 방지
                foreach (var b in multi) Attach(ui, b);
            }

            var single = GetBinding(ui);
            if (single != null)
            {
                // Attach 내부 중복 방지 로직(map.ContainsKey)으로 안전
                Attach(ui, single);
            }
        }
    }
}
