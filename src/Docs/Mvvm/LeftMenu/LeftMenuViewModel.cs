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

namespace Docs.Mvvm.LeftMenu
{
    public class LeftMenuViewModel : NotifyPropertyChangedBase
    {
        public IBottomSharedContext Context { get; set; }

        public LeftMenuViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                InitializeMenuItems(); // 디자인 타임 샘플 데이터
            }
        }

        public LeftMenuViewModel(IBottomSharedContext context)
        {
            Context = context;

            InitializeMenuItems();
        }

        private void InitializeMenuItems()
        {
            var item = new MenuItem()
            {
                Title = "Icons",
                ViewType = typeof(IconsView)
            };
            item.AddSubItem("Test1", typeof(IconsView));
            item.SubItems.Last().AddSubItem("Test3", typeof(IconsView));
            item.SubItems.Last().AddSubItem("Test4", typeof(IconsView));

            item.AddSubItem("Test2", typeof(IconsView));
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
