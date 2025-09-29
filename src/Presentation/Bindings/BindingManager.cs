using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace HYSoft.Presentation.Bindings
{
    public static class BindingManager
    {
        public static void SetBindingSource(DependencyObject attachedObject,
                                            DependencyObject target,
                                            DependencyProperty dp)
        {
            if (attachedObject == null || target == null || dp == null) return;

            // 1) Binding
            var b = BindingOperations.GetBinding(target, dp);
            if (b != null)
            {
                var nb = BuildRebasedBinding(attachedObject, target, b);
                BindingOperations.SetBinding(target, dp, nb);
                return;
            }

            // 2) MultiBinding
            var mb = BindingOperations.GetMultiBinding(target, dp);
            if (mb != null)
            {
                var nmb = new MultiBinding
                {
                    Mode = mb.Mode,
                    UpdateSourceTrigger = mb.UpdateSourceTrigger,
                    Converter = mb.Converter,
                    ConverterParameter = mb.ConverterParameter,
                    StringFormat = mb.StringFormat,
                    FallbackValue = mb.FallbackValue,
                    TargetNullValue = mb.TargetNullValue,
                    NotifyOnSourceUpdated = mb.NotifyOnSourceUpdated,
                    NotifyOnTargetUpdated = mb.NotifyOnTargetUpdated,
                    ValidatesOnDataErrors = mb.ValidatesOnDataErrors,
                    ValidatesOnExceptions = mb.ValidatesOnExceptions
                };

                foreach (var child in mb.Bindings)
                {
                    switch (child)
                    {
                        case Binding cb:
                            nmb.Bindings.Add(BuildRebasedBinding(attachedObject, target, cb));
                            break;

                        case PriorityBinding cpb:
                            var npb = new PriorityBinding
                            {
                                FallbackValue = cpb.FallbackValue,
                                StringFormat = cpb.StringFormat,
                                TargetNullValue = cpb.TargetNullValue
                            };
                            foreach (var inner in cpb.Bindings.OfType<Binding>())
                                npb.Bindings.Add(BuildRebasedBinding(attachedObject, target, inner));
                            nmb.Bindings.Add(npb);
                            break;
                    }
                }

                foreach (var rule in mb.ValidationRules)
                    nmb.ValidationRules.Add(rule);

                BindingOperations.SetBinding(target, dp, nmb);
                return;
            }

            // 3) PriorityBinding
            var pb = BindingOperations.GetPriorityBinding(target, dp);
            if (pb != null)
            {
                var npb = new PriorityBinding
                {
                    FallbackValue = pb.FallbackValue,
                    StringFormat = pb.StringFormat,
                    TargetNullValue = pb.TargetNullValue
                };
                foreach (var child in pb.Bindings.OfType<Binding>())
                    npb.Bindings.Add(BuildRebasedBinding(attachedObject, target, child));
                BindingOperations.SetBinding(target, dp, npb);
            }
        }

        // === 새 바인딩 생성(위치지정자 제거) ===============================

        private static Binding BuildRebasedBinding(DependencyObject attachedObject,
                                                   DependencyObject target,
                                                   Binding src)
        {
            // 1) ElementName / RelativeSource / Source가 있으면 기존 로직대로 해석
            object resolvedSource = null;

            if (!string.IsNullOrEmpty(src.ElementName))
            {
                resolvedSource = FindByName(attachedObject, src.ElementName);
            }
            else if (src.RelativeSource != null)
            {
                var rs = src.RelativeSource;
                if (rs.Mode == RelativeSourceMode.Self)
                    resolvedSource = target;
                else if (rs.Mode == RelativeSourceMode.FindAncestor)
                    resolvedSource = FindAncestorByTypeAndLevel(attachedObject ?? target,
                                                                rs.AncestorType,
                                                                Math.Max(1, rs.AncestorLevel));
                else if (rs.Mode == RelativeSourceMode.TemplatedParent)
                    resolvedSource = (attachedObject as FrameworkElement)?.TemplatedParent
                                  ?? (attachedObject as FrameworkContentElement)?.TemplatedParent;
            }
            else if (src.Source != null)
            {
                resolvedSource = src.Source;
            }

            // 2) 새 바인딩 생성
            Binding b;

            if (resolvedSource != null)
            {
                // 기존처럼 Source를 명시적으로 지정
                b = new Binding
                {
                    Path = src.Path != null ? new PropertyPath(src.Path.Path, src.Path.PathParameters) : null,
                    Source = resolvedSource
                };
            }
            else
            {
                // ★ 핵심: 순수 {Binding} → attachedObject.DataContext를 추적하도록 리베이스
                // 원래 경로가 없으면 현재 DataContext 자체를 사용
                string userPath = src.Path?.Path;

                string path = string.IsNullOrEmpty(userPath)
                    ? "DataContext"
                    : $"DataContext.{userPath}";

                b = new Binding
                {
                    // Source를 DataContext 값으로 박아두지 말고 attachedObject로 잡고
                    // Path로 DataContext(.[원래경로])를 타게 하면 DataContext 변경도 추적됨
                    Source = attachedObject,
                    Path = new PropertyPath(path, src.Path?.PathParameters)
                };
            }

            // 공통 속성 복사
            b.Mode = src.Mode;
            b.UpdateSourceTrigger = src.UpdateSourceTrigger;
            b.Converter = src.Converter;
            b.ConverterParameter = src.ConverterParameter;
            b.ConverterCulture = src.ConverterCulture;
            b.StringFormat = src.StringFormat;
            b.FallbackValue = src.FallbackValue;
            b.TargetNullValue = src.TargetNullValue;
            b.BindsDirectlyToSource = src.BindsDirectlyToSource;
            b.IsAsync = src.IsAsync;
            b.XPath = src.XPath;
            b.Delay = src.Delay;
            b.BindingGroupName = src.BindingGroupName;
            b.ValidatesOnDataErrors = src.ValidatesOnDataErrors;
            b.ValidatesOnExceptions = src.ValidatesOnExceptions;
            b.NotifyOnValidationError = src.NotifyOnValidationError;

            foreach (var rule in src.ValidationRules)
                b.ValidationRules.Add(rule);

            return b;
        }

        // === 트리 유틸 =======================================================

        private static DependencyObject FindAncestorByTypeAndLevel(DependencyObject start, Type type, int level)
        {
            if (start == null || type == null || level <= 0) return null;
            int found = 0;
            var current = start;
            while (current != null)
            {
                if (type.IsInstanceOfType(current) && ++found == level)
                    return current;
                current = GetParent(current);
            }
            return null;
        }

        private static DependencyObject GetParent(DependencyObject d)
        {
            if (d == null) return null;

            if (d is Visual || d is Visual3D)
            {
                var p = VisualTreeHelper.GetParent(d);
                if (p != null) return p;
            }

            if (d is FrameworkElement fe)
            {
                if (fe.Parent != null) return fe.Parent;
                return LogicalTreeHelper.GetParent(d);
            }

            if (d is FrameworkContentElement fce)
            {
                if (fce.Parent != null) return fce.Parent;
            }

            return LogicalTreeHelper.GetParent(d);
        }

        private static object FindByName(DependencyObject scope, string name)
        {
            // 1) 현재 요소 네임스코프에서 찾기
            if (scope is FrameworkElement fe)
            {
                var found = fe.FindName(name);
                if (found != null) return found;

                // 2) ControlTemplate 네임스코프
                if (fe is Control ctrl && ctrl.Template != null)
                {
                    var t = ctrl.Template.FindName(name, ctrl);
                    if (t != null) return t;
                }

                // 3) DataTemplate(ContentPresenter) 네임스코프
                if (fe is ContentPresenter cp && cp.ContentTemplate != null)
                {
                    var dt = cp.ContentTemplate.FindName(name, cp);
                    if (dt != null) return dt;
                }
            }

            // 4) 위로 올라가며 반복 시도 (각 조상마다 동일 로직 적용)
            var cur = GetParent(scope);
            while (cur != null)
            {
                if (cur is FrameworkElement fe2)
                {
                    var found = fe2.FindName(name);
                    if (found != null) return found;

                    if (fe2 is Control ctrl2 && ctrl2.Template != null)
                    {
                        var t2 = ctrl2.Template.FindName(name, ctrl2);
                        if (t2 != null) return t2;
                    }

                    if (fe2 is ContentPresenter cp2 && cp2.ContentTemplate != null)
                    {
                        var dt2 = cp2.ContentTemplate.FindName(name, cp2);
                        if (dt2 != null) return dt2;
                    }
                }
                cur = GetParent(cur);
            }

            return null;
        }
    }
}
