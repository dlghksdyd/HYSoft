using Docs.Mvvm.Popup;
using Docs.Mvvm.Styles;
using HYSoft.Presentation.Interactivity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public LeftMenuViewModel()
        {
            SharedContext = new BottomSharedContext();

            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                SetMenuItems_Styles(); // 디자인 타임 샘플 데이터
            }
            else
            {
                throw new InvalidOperationException("반드시 sharedContext를 파라미터에 넣어줘야 합니다.");
            }
        }

        public LeftMenuViewModel(IBottomSharedContext sharedContext)
        {
            SharedContext = sharedContext;

            SetMenuItems_Styles();
        }

        private void SetMenuItems_Styles()
        {
            MenuItems.Clear();

            var item = new MenuItem()
            {
                Title = "Icons",
                ViewType = typeof(IconsView)
            };
            MenuItems.Add(item);
        }
        
        private ObservableCollection<MenuItem> _menuItems = new ObservableCollection<MenuItem>();
        public ObservableCollection<MenuItem> MenuItems
        {
            get => _menuItems;
            set => SetProperty(ref _menuItems, value);
        }
    }
}
