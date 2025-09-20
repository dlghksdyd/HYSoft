using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HYSoft.Presentation.Interactivity;

namespace Docs.Mvvm.LeftMenu
{
    public class MenuItemContainer : NotifyPropertyChangedBase
    {
        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }
        
        private ObservableCollection<MenuItem> _items = new ObservableCollection<MenuItem>();
        public ObservableCollection<MenuItem> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }
    }
}
