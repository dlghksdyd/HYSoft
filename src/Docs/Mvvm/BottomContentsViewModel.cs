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
        public IBottomSharedContext SharedContext { get; } = new BottomSharedContext();

        public BottomContentsViewModel()
        {
            Menu = new LeftMenuViewModel(SharedContext);

            SharedContext.UpdateContent = new RelayCommand<EventPayload>((p) =>
            {
                if (p is null) return;
                if (p.Parameter is not MenuItem item) return;
                item.ExpandMenuCommand.Execute(null);
                Content = Activator.CreateInstance(item.ViewType);
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
