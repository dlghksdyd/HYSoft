using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HYSoft.Presentation.Interactivity
{
    public class FocusAction : TriggerAction<FrameworkElement>
    {
        protected override void Invoke(object parameter)
        {
            if (AssociatedObject == null)
                return;

            // 포커스 가능하도록 보장
            AssociatedObject.Focusable = true;

            // 키보드 포커스 + 논리 포커스
            AssociatedObject.Focus();
            Keyboard.Focus(AssociatedObject);
        }
    }
}
