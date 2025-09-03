using HYSoft.Presentation.Adorners;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace HYSoft.Presentation.DragDrop
{
    public class DragDrop
    {
        // Handler
        private readonly ObservableCollection<DragEventHandler> _dragOverEventHandlers = [];
        private readonly ObservableCollection<DragEventHandler> _dragLeaveEventHandlers = [];
        private readonly ObservableCollection<DragEventHandler> _dragEnterEventHandlers = [];

        // DataContext
        private IDragDataContext? _dragDataContext;

        private Point _mouseDownStartPoint;

        // Adorner
        private AdornerLayer? _adornerLayer;
        private GhostAdorner? _dragAdorner;

        public DragDrop()
        {
            InitializeVariables();
        }

        private void InitializeVariables()
        {
            _mouseDownStartPoint = new Point(0, 0);

            // Adorner
            _adornerLayer = null;
            _dragAdorner = null;
        }

        public void AddDragOverEventHandler(DragEventHandler handler)
        {
            _dragOverEventHandlers.Add(handler);
        }

        public void AddDragLeaveEventHandler(DragEventHandler handler)
        {
            _dragLeaveEventHandlers.Add(handler);
        }

        public void AddDragEnterEventHandler(DragEventHandler handler)
        {
            _dragEnterEventHandlers.Add(handler);
        }

        public void PreviewMouseLeftButtonDown_InitData(MouseEventArgs e, IDragDataContext dataContext)
        {
            if (dataContext.DragScope == null || string.IsNullOrEmpty(dataContext.ItemAlias))
                return;

            _dragDataContext = dataContext;

            _mouseDownStartPoint = e.GetPosition(null);
        }

        public void PreviewMouseMove_StartDragDrop(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            if (_dragDataContext?.DraggedItem == null) return;

            var currentPoint = e.GetPosition(null);
            var diff = _mouseDownStartPoint - currentPoint;

            if (!(Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance) &&
                !(Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)) return;

            try
            {
                if (_dragDataContext.DragScope != null)
                {
                    if (_dragDataContext.AdornerElement != null)
                    {
                        _adornerLayer = AdornerLayer.GetAdornerLayer(_dragDataContext.DragScope);
                        _dragAdorner = new GhostAdorner(_dragDataContext.DragScope, _dragDataContext.AdornerElement, _dragDataContext.AdornerOpacity);
                        if (_dragAdorner != null) _adornerLayer?.Add(_dragAdorner);
                    }

                    _dragDataContext.DragScope.DragOver += DragScope_DragOver;
                    _dragDataContext.DragScope.QueryContinueDrag += DragScope_QueryContinueDrag;
                    _dragDataContext.DragScope.DragLeave += DragScope_DragLeave;
                    _dragDataContext.DragScope.DragEnter += DragScope_DragEnter;
                    _dragDataContext.DragScope.AllowDrop = true;

                    foreach (var handler in _dragOverEventHandlers)
                    {
                        _dragDataContext.DragScope.DragOver += handler;
                    }
                    foreach (var handler in _dragLeaveEventHandlers)
                    {
                        _dragDataContext.DragScope.DragLeave += handler;
                    }
                    foreach (var handler in _dragEnterEventHandlers)
                    {
                        _dragDataContext.DragScope.DragEnter += handler;
                    }
                }

                var data = new DataObject(_dragDataContext.ItemAlias, _dragDataContext.DraggedItem);
                System.Windows.DragDrop.DoDragDrop((DependencyObject)sender, data, DragDropEffects.Move);
            }
            finally
            {
                Clear();
            }
        }

        public T? Drop_GetDropData<T>(DragEventArgs e)
        {
            return !e.Data.GetDataPresent(_dragDataContext?.ItemAlias) ? default : (T)e.Data.GetData(_dragDataContext?.ItemAlias);
        }

        private void DragScope_DragOver(object sender, DragEventArgs e)
        {
            if (_dragDataContext?.DragScope == null) return;
            
            // 스코프 좌표계에서 마우스 위치
            var p = e.GetPosition(_dragDataContext.DragScope);

            // 클릭한 지점이 행 내부 어디였는지 보정해 자연스럽게 보이도록
            _dragAdorner?.SetPosition(p.X - _dragDataContext.AdornerOffset.X, p.Y - _dragDataContext.AdornerOffset.Y);
        }

        private void DragScope_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            // ESC로 취소되거나 드래그 종료시 정리
            if (e.EscapePressed || e.Action == DragAction.Cancel || e.Action == DragAction.Drop)
            {
                Clear();
            }
        }

        private void DragScope_DragLeave(object sender, DragEventArgs e)
        {
            if (_adornerLayer == null || _dragAdorner == null) return;
            if (_dragDataContext?.DragScope == null) return;
            
            var adorners = _adornerLayer.GetAdorners(_dragDataContext.DragScope);
            if (adorners != null && adorners.Contains(_dragAdorner))
            {
                _adornerLayer.Remove(_dragAdorner);
            }
        }

        private void DragScope_DragEnter(object sender, DragEventArgs e)
        {
            if (_adornerLayer == null || _dragAdorner == null) return;
            if (_dragDataContext?.DragScope == null) return;
            
            var adorners = _adornerLayer.GetAdorners(_dragDataContext.DragScope);
            if (adorners == null)
            {
                _adornerLayer.Add(_dragAdorner);
            }
        }

        private void Clear()
        {
            if (_dragDataContext?.DragScope != null)
            {
                _dragDataContext.DragScope.DragOver -= DragScope_DragOver;
                _dragDataContext.DragScope.QueryContinueDrag -= DragScope_QueryContinueDrag;
                _dragDataContext.DragScope.DragLeave -= DragScope_DragLeave;
                _dragDataContext.DragScope.DragEnter -= DragScope_DragEnter;
                _dragDataContext.DragScope.AllowDrop = true;

                foreach (var handler in _dragOverEventHandlers)
                {
                    _dragDataContext.DragScope.DragOver -= handler;
                }
                foreach (var handler in _dragLeaveEventHandlers)
                {
                    _dragDataContext.DragScope.DragLeave -= handler;
                }
                foreach (var handler in _dragEnterEventHandlers)
                {
                    _dragDataContext.DragScope.DragEnter -= handler;
                }
            }

            if (_adornerLayer != null && _dragAdorner != null)
            {
                _adornerLayer.Remove(_dragAdorner);
            }

            InitializeVariables();
        }
    }


}
