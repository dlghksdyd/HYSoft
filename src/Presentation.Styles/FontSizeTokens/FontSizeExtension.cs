using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Markup.Primitives;

namespace HYSoft.Presentation.Styles.FontSizeTokens
{
    [MarkupExtensionReturnType(typeof(double))]
    public sealed class FontSizeExtension : MarkupExtension
    {
        public FontSizeExtension()
        {
        }

        public FontSizeExtension(EFontSizeKeys inKey) => Key = inKey;

        [ConstructorArgument("inKey")]
        public EFontSizeKeys Key { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var palette = FontSizePalette.Current;

            return palette.GetBrush(Key);
        }
    }

    public class FontSizePalette : Freezable
    {
        private Dictionary<EFontSizeKeys, double>? _map;

        private static FontSizePalette? _current;
        public static FontSizePalette Current => _current = Initialize();

        public double GetBrush(EFontSizeKeys key)
        {
            return _map is null ? 16.0 : _map[key];
        }

        private static FontSizePalette Initialize()
        {
            if (_current is not null) return _current;

            var p = new FontSizePalette
            {
                _map = FontSizeGenerator.Generate()
            };

            return p;
        }

        protected override Freezable CreateInstanceCore() => new FontSizePalette();
    }
}
