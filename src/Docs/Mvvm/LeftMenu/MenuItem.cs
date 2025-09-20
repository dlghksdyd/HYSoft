using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        private Type _viewType = typeof(object);

        public Type ViewType
        {
            get => _viewType;
            set => SetProperty(ref _viewType, value);
        }
        
        private ObservableCollection<MenuItem> _subItems = new ObservableCollection<MenuItem>();
        public ObservableCollection<MenuItem> SubItems
        {
            get => _subItems;
            set => SetProperty(ref _subItems, value);
        }

        public void AddSubItem(string title, Type viewType)
        {
            var item = new MenuItem();
            item.Title = title;
            item.ViewType = viewType;
            SubItems.Add(item);
        }
    }
}
