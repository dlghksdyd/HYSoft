using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace HYSoft.Presentation.Styles
{
    public static class StyleColor
    {
        public static SolidColorBrush logo_primary = new SolidColorBrush(Color.FromArgb(255, 0x0B, 0x78, 0xBC));
        public static SolidColorBrush logo_secondary = new SolidColorBrush(Color.FromArgb(255, 0xe4, 0x89, 0x00));

        public static SolidColorBrush text_primary = new SolidColorBrush(Color.FromArgb(255, 0xea, 0xea, 0xea));
        public static SolidColorBrush text_white = new SolidColorBrush(Color.FromArgb(255, 0xff, 0xff, 0xff));
        public static SolidColorBrush text_watermark = new SolidColorBrush(Color.FromArgb(255, 0xbb, 0xbb, 0xbb));

        public static SolidColorBrush border_primary = new SolidColorBrush(Color.FromArgb(255, 0x99, 0x99, 0x99));

        public static SolidColorBrush surface_primary = new SolidColorBrush(Color.FromArgb(255, 0x1e, 0x27, 0x36));
        public static SolidColorBrush surface_section = new SolidColorBrush(Color.FromArgb(255, 0x2d, 0x38, 0x47));
        public static SolidColorBrush surface_title = new SolidColorBrush(Color.FromArgb(255, 0x4e, 0x59, 0x69));
        public static SolidColorBrush surface_content = new SolidColorBrush(Color.FromArgb(255, 0x34, 0x49, 0x5e));
        public static SolidColorBrush surface_hover = new SolidColorBrush(Color.FromArgb(0x4C, 0x7d, 0xad, 0xcc));
        public static SolidColorBrush surface_hover2 = new SolidColorBrush(Color.FromArgb(0xFF, 0x7d, 0xad, 0xcc));
        public static SolidColorBrush surface_selected = new SolidColorBrush(Color.FromArgb(255, 0x3f, 0x80, 0xa9));
        public static SolidColorBrush surface_info = new SolidColorBrush(Color.FromArgb(255, 0xd1, 0xe8, 0xef));
        public static SolidColorBrush surface_white = new SolidColorBrush(Color.FromArgb(255, 0xff, 0xff, 0xff));
        public static SolidColorBrush surface_transparent = new SolidColorBrush(Color.FromArgb(00, 0xff, 0xff, 0xff));
        public static SolidColorBrush surface_transparent_fill = new SolidColorBrush(Color.FromArgb(0x01, 0xff, 0xff, 0xff));
        public static SolidColorBrush surface_popup = new SolidColorBrush(Color.FromArgb(0x4c, 0xd1, 0xe8, 0xef));

    }
}
