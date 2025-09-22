using Docs.Mvvm.Popup;
using Docs.Mvvm.Styles;
using HYSoft.Presentation.Interactivity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Docs.Mvvm.LeftMenu
{
    public class LeftMenuViewModel : NotifyPropertyChangedBase
    {
        public IBottomSharedContext SharedContext { get; set; }

        /// <summary>
        /// 디자인 타임 용도로 만든 생성자
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public LeftMenuViewModel()
        {
            SharedContext = new BottomSharedContext();
            SharedContext.UpdateLeftMenuItemCommand.Execute(new EventPayload(null, null, ELeftMenuType.UiSupport));
        }

        public LeftMenuViewModel(IBottomSharedContext sharedContext)
        {
            SharedContext = sharedContext;
        }
    }
}
