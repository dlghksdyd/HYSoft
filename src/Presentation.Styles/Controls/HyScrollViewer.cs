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
    public class HyScrollViewer : ScrollViewer
    {
        static HyScrollViewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HyScrollViewer), new FrameworkPropertyMetadata(typeof(HyScrollViewer)));
        }
    }
}
