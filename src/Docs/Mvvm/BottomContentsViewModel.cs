using HYSoft.Presentation.Interactivity;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Docs.Mvvm.LeftMenu;
using Docs.Mvvm.Styles;

namespace Docs.Mvvm
{
    public class BottomContentsViewModel : NotifyPropertyChangedBase
    {
        private readonly IBottomSharedContext _sharedContext = new BottomSharedContext();

        public BottomContentsViewModel()
        {
            Menu = new LeftMenuViewModel(_sharedContext);

            _sharedContext.UpdateContent = new RelayCommand<EventPayload>((p) =>
            {
                if (p is null) return;
                if (p.Parameter is not Type type) return;
                Content = Activator.CreateInstance(type);
            });
        }
        
        private object _menu;
        public object Menu
        {
            get => _menu;
            set => SetProperty(ref _menu, value);
        }

        private object? _content;
        public object? Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }
    }
}
