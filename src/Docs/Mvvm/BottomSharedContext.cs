using Docs.Mvvm.LeftMenu;
using HYSoft.Presentation.Interactivity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Ribbon.Primitives;
using System.Windows.Input;
using Docs.Mvvm.Styles.Components;

namespace Docs.Mvvm
{
    public interface IBottomSharedContext
    {
        // 오른쪽에 보이는 컨텐츠
        object? Content { get; set; }
        
        // 왼쪽 메뉴 아이템
        ObservableCollection<MenuItem> MenuItems { get; set; }
        public bool IsContentVisibility { get; }
        public void SelectMenuItem(Type viewType);

        // 커맨드
        ICommand UpdateLeftMenuItemCommand { get; }
        ICommand SelectMenuItemCommand { get; }
    }

    public class BottomSharedContext : NotifyPropertyChangedBase, IBottomSharedContext
    {
        public BottomSharedContext()
        {
            MenuItems.CollectionChanged += MenuItems_CollectionChanged;
        }

        private object? _content;
        public object? Content 
        { 
            get => _content;
            set => SetProperty(ref _content, value);
        }

        #region 왼쪽 메뉴 아이템

        public ObservableCollection<MenuItem> MenuItems { get; set; } = new ObservableCollection<MenuItem>();
        private void MenuItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // 아이템 추가/삭제 시 항상 알림 발생
            RaisePropertyChanged(nameof(IsContentVisibility));
        }
        public bool IsContentVisibility => MenuItems.ToList().Count != 0;
        public MenuItem? SelectedMenu { get; set; }

        public void SelectMenuItem(Type viewType)
        {
            var item = FindMenuItem(viewType);
            if (item is null) return;
            SelectMenuItemCommand.Execute(new EventPayload(null, null, item));
        }
        
        private MenuItem? FindMenuItem(Type viewType)
        {
            foreach (var item in MenuItems)
            {
                if (item.ViewType == viewType)
                {
                    return item;
                }
                else
                {
                    var subItem = FindSubMenuItem(item.SubItems, viewType);
                    if (subItem is not null) return subItem;
                }
            }

            return null;
        }

        private MenuItem? FindSubMenuItem(ObservableCollection<MenuItem> menuItems, Type viewType)
        {
            foreach (var item in menuItems)
            {
                if (item.ViewType == viewType)
                {
                    return item;
                }
                else
                {
                    var subItem = FindSubMenuItem(item.SubItems, viewType);
                    if (subItem is not null) return subItem;
                }
            }

            return null;
        }

        private MenuItem AddItem(string title, Type? viewType, bool isExpand = false)
        {
            var item = new MenuItem()
            {
                Title = title,
                ViewType = viewType,
                Parent = null,
            };
            if (isExpand)
                item.ExpandMenuCommand.Execute(null);
            
            MenuItems.Add(item);

            return item;
        }

        #endregion

        #region 커맨드

        public ICommand SelectMenuItemCommand => new RelayCommand<EventPayload>((p) =>
        {
            if (p is null) return;
            if (p.Parameter is not MenuItem item) return;

            // 기존 선택된 메뉴가 있으면 선택 해제
            SelectedMenu?.UnselectMenuCommand.Execute(null);

            // 새로 선택된 메뉴 설정
            SelectedMenu = item;
            item.ExpandMenuCommand.Execute(null);
            item.SelectMenuCommand.Execute(null);

            // 컨텐츠 변경
            if (item.ViewType is null) Content = null;
            else Content = Activator.CreateInstance(item.ViewType, this);
        });
        
        public ICommand UpdateLeftMenuItemCommand => new RelayCommand<EventPayload>((p) =>
        {
            if (p?.Parameter is not ELeftMenuType type) return;

            if (type == ELeftMenuType.UiSupport)
            {
                SetMenuItems_Styles();
            }
        });

        #endregion

        private void SetMenuItems_Styles()
        {
            MenuItems.Clear();

            var item1 = AddItem("Colors", null, true);
            item1.AddSubItem("Default Tokens", null);
            item1.AddSubItem("Custom Tokens", null);

            var item2 = AddItem("Fonts", null, true);
            item2.AddSubItem("Font Size", null);

            var item3 = AddItem("Components", null, true);
            item3.AddSubItem("Button", null);
            item3.AddSubItem("CheckBox", null);
            item3.AddSubItem("Icon", typeof(IconView));
            item3.AddSubItem("PasswordBox", null);
            item3.AddSubItem("RadioButton", null);
            item3.AddSubItem("ScrollViewer", null);
            item3.AddSubItem("TextBox", null);
            item3.AddSubItem("TitleBar", null);

            AddItem("Adorner", null, true);
            AddItem("Converter", null, true);
            AddItem("DragDrop", null, true);
            AddItem("UI Tree Helper", null, true);
            AddItem("Interactivity", null, true);
            AddItem("Modal", null, true);
        }
    }
}
