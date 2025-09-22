using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Animation;
using HYSoft.Presentation.Interactivity;

namespace Docs.Mvvm.LeftMenu
{
    public class MenuItem : NotifyPropertyChangedBase
    {
        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private Type? _viewType = typeof(object);
        public Type? ViewType
        {
            get => _viewType;
            set => SetProperty(ref _viewType, value);
        }

        private MenuItem? _parent;
        public MenuItem? Parent
        {
            get => _parent;
            set => SetProperty(ref _parent, value);
        }

        private ObservableCollection<MenuItem> _subItems = new ObservableCollection<MenuItem>();
        public ObservableCollection<MenuItem> SubItems
        {
            get => _subItems;
            set => SetProperty(ref _subItems, value);
        }

        public bool IsSubItemExist => SubItems.ToList().Count != 0;

        private bool _isExpand;
        public bool IsExpand
        {
            get => _isExpand;
            set => SetProperty(ref _isExpand, value);
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
        
        public MenuItem AddSubItem(string title, Type? viewType)
        {
            var item = new MenuItem();
            item.Title = title;
            item.ViewType = viewType;
            item.Parent = this;
            SubItems.Add(item);

            return item;
        }

        public ICommand UnselectMenuCommand => new RelayCommand(() =>
        {
            IsSelected = false;
        });

        public ICommand SelectMenuCommand => new RelayCommand(() =>
        {
            IsSelected = true;
        });
        
        public ICommand ExpandMenuCommand => new RelayCommand(() =>
        {
            IsExpand = true;
            
            Parent?.ExpandMenuCommand.Execute(null);
        });

        public ICommand CollapseMenuCommand => new RelayCommand(() =>
        {
            IsExpand = false;
        });
    }
}
