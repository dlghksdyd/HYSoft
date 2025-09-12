using HYSoft.Presentation.Styles.Colors.HYSoft.Presentation.Styles.Colors;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace HYSoft.Presentation.Styles.Colors
{
    [MarkupExtensionReturnType(typeof(object))]
    public sealed class ColorExtension : MarkupExtension
    {
        public ColorExtension() { }
        public ColorExtension(EColorKeys key) => Key = key;

        public EColorKeys Key { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // 키 기반 인덱서 바인딩을 만들어 반환
            var binding = new Binding($"[{Key}]")
            {
                Source = PaletteSource.Instance,
                Mode = BindingMode.OneWay
            };
            return binding.ProvideValue(serviceProvider);
        }
    }
}
