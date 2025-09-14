using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Markup.Primitives;

namespace HYSoft.Presentation.Styles.ColorTokens
{
    [MarkupExtensionReturnType(typeof(Brush))]
    public sealed class ColorExtension : MarkupExtension
    {
        public ColorExtension()
        {
        }

        public ColorExtension(EColorKeys inKey) => Key = inKey;

        [ConstructorArgument("inKey")]
        public EColorKeys Key { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var palette = ColorPalette.Current;

            return palette.GetBrush(Key);
        }
    }

    public class ColorPalette : Freezable
    {
        private Dictionary<EColorKeys, Brush>? _map;

        private static ColorPalette? _current;
        public static ColorPalette Current => _current = Initialize();

        public Brush GetBrush(EColorKeys key)
        {
            return _map is null ? new SolidColorBrush(Colors.Transparent) : _map[key];
        }

        private static ColorPalette Initialize()
        {
            if (_current is not null) return _current;

            var p = new ColorPalette
            {
                _map = ColorGenerator.Generate()
            };

            return p;
        }

        protected override Freezable CreateInstanceCore() => new ColorPalette();
    }
}
