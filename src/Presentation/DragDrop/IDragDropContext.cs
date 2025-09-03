using System.Windows;

namespace HYSoft.Presentation.DragDrop
{
    public interface IDragDataContext
    {
        UIElement? DragScope { get; set; }
        object DraggedItem { get; set; }
        string ItemAlias { get; set; }
        UIElement? AdornerElement { get; set; }
        double AdornerOpacity { get; set; }
        Point AdornerOffset { get; set; }
    }
}
