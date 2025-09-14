using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace HYSoft.Presentation.Styles.Icons
{
    [MarkupExtensionReturnType(typeof(Brush))]
    public sealed class IconExtension : MarkupExtension
    {
        public IconExtension()
        {
        }

        public IconExtension(object itemKey) => ItemKey = itemKey;

        [ConstructorArgument("itemKey")]
        public object ItemKey { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var palette = IconCollection.Current;

            return palette.GetBrush(ItemKey);
        }
    }

    public class IconCollection : Freezable
    {
        private Dictionary<object, Brush>? _map;

        private static IconCollection? _current;
        public static IconCollection Current => _current = Initialize();

        public Brush GetBrush(object key)
        {
            return _map is null ? new SolidColorBrush(Colors.Transparent) : _map[key];
        }

        private static IconCollection Initialize()
        {
            if (_current is not null) return _current;

            //var p = new IconCollection
            //{
            //    _map = ColorGenerator.Generate()
            //};
            //return p;
            return new IconCollection();
        }

        protected override Freezable CreateInstanceCore() => new IconCollection();
    }
}
