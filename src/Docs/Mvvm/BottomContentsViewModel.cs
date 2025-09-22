using HYSoft.Presentation.Interactivity;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Docs.Mvvm.LeftMenu;
using Docs.Mvvm.Styles;

namespace Docs.Mvvm
{
    public class BottomContentsViewModel : NotifyPropertyChangedBase
    {
        public IBottomSharedContext SharedContext { get; } = new BottomSharedContext();

        public BottomContentsViewModel()
        {
            Menu = new LeftMenuViewModel(SharedContext);
        }
        
        private object _menu;
        public object Menu
        {
            get => _menu;
            set => SetProperty(ref _menu, value);
        }
    }
}
