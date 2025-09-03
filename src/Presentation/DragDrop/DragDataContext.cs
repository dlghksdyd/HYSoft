using Prism.Mvvm;
using System.Windows;

namespace HYSoft.Presentation.DragDrop
{


    public class DragDataContext : BindableBase, IDragDataContext
    {
        private UIElement _dragScope = new UIElement();
        public UIElement DragScope
        {
            get => _dragScope;
            set => SetProperty(ref _dragScope, value);
        }

        private object _draggedItem;
        public object DraggedItem
        {
            get => _draggedItem;
            set => SetProperty(ref _draggedItem, value);
        }

        private string _itemAlias = string.Empty;
        public string ItemAlias
        {
            get => _itemAlias;
            set => SetProperty(ref _itemAlias, value);
        }

        private UIElement? _adornerElement;
        public UIElement? AdornerElement
        {
            get => _adornerElement;
            set => SetProperty(ref _adornerElement, value);
        }

        private double _adornerOpacity = 0.3;
        public double AdornerOpacity
        {
            get => _adornerOpacity;
            set => SetProperty(ref _adornerOpacity, value);
        }

        private Point _adornerOffset = new Point(0, 0);
        public Point AdornerOffset
        {
            get => _adornerOffset;
            set => SetProperty(ref _adornerOffset, value);
        }
    }
}
