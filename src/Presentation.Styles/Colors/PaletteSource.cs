using HYSoft.Presentation.Styles.Colors.HYSoft.Presentation.Styles.Colors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace HYSoft.Presentation.Styles.Colors
{
    public sealed class PaletteSource : INotifyPropertyChanged
    {
        public static PaletteSource Instance { get; } = new();

        // 인덱서: 바인딩 경로 [Key] 로 접근
        public SolidColorBrush this[EColorKeys key] => ColorPalette.GetBrush(key);

        public event PropertyChangedEventHandler PropertyChanged;
        internal void NotifyKey(EColorKeys key)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Item[{key}]"));
    }
}
