using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace HYSoft.Presentation.Styles.Controls
{
    public class StyleScrollViewer : ScrollViewer
    {
        static StyleScrollViewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(StyleScrollViewer), new FrameworkPropertyMetadata(typeof(StyleScrollViewer)));
        }
    }
}
