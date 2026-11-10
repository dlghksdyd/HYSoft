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

            // 시작하자마자 빈 화면이 아니도록 UI Support 의 첫 페이지를 연다.
            SharedContext.UpdateLeftMenuItemCommand.Execute(
                new HYSoft.Presentation.Interactivity.CommandBehaviors.EventPayload(null, null, ELeftMenuType.UiSupport));
        }
        
        private object? _menu;
        public object? Menu
        {
            get => _menu;
            set => SetProperty(ref _menu, value);
        }
    }
}
