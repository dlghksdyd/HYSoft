using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Markup;

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

            return palette.GetFontSize(Key);
        }
    }

    public class FontSizePalette : Freezable
    {
        private Dictionary<EFontSizeKeys, double>? _map;

        private static FontSizePalette? _current;
        public static FontSizePalette Current => _current ??= Initialize();

        public double GetFontSize(EFontSizeKeys key)
        {
            if (_map is null) return 16.0;
            return _map.TryGetValue(key, out var size) ? size : 16.0;
        }

        private static FontSizePalette Initialize()
        {
            return new FontSizePalette
            {
                _map = FontSizeGenerator.Generate()
            };
        }

        protected override Freezable CreateInstanceCore() => new FontSizePalette();
    }
}
