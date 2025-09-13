using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using HYSoft.Presentation.Styles.ColorTokens;

namespace Samples
{
    public class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // App.xaml에서 ResourceDictionary들이 모두 로드된 뒤에 호출해야 함
            // ButtonPrimaryBorder를 예시로 컬러를 바꿔보자.
            ColorPalette.Override(EColorKeys.ButtonPrimaryBorder, Colors.Black);
        }
    }
}
