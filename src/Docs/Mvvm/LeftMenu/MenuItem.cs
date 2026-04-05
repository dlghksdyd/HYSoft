using HYSoft.Presentation.Interactivity;
using System.Collections.ObjectModel;
using System.Windows.Input;
using HYSoft.Presentation.Interactivity.CommandBehaviors;

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
        
        public MenuItem AddSubItem(string title, Type? viewType, bool isExpand = false)
        {
            var item = new MenuItem();
            item.Title = title;
            item.ViewType = viewType;
            item.Parent = this;

            if (isExpand)
                ExpandMenuCommand.Execute(null);
            
            SubItems.Add(item);

            return item;
        }

        private ICommand? _unselectMenuCommand;
        public ICommand UnselectMenuCommand => _unselectMenuCommand ??= new RelayCommand(() =>
        {
            IsSelected = false;
        });

        private ICommand? _selectMenuCommand;
        public ICommand SelectMenuCommand => _selectMenuCommand ??= new RelayCommand(() =>
        {
            IsSelected = true;
        });

        private ICommand? _expandMenuCommand;
        public ICommand ExpandMenuCommand => _expandMenuCommand ??= new RelayCommand(() =>
        {
            IsExpand = true;

            Parent?.ExpandMenuCommand.Execute(null);
        });

        private ICommand? _collapseMenuCommand;
        public ICommand CollapseMenuCommand => _collapseMenuCommand ??= new RelayCommand(() =>
        {
            IsExpand = false;
        });
    }
}
