using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Docs.Mvvm.Styles.Icons;
using HYSoft.Presentation.Interactivity;

namespace Docs.Mvvm.Styles
{
    public class StylesViewModel : NotifyPropertyChangedBase
    {
        public StylesViewModel()
        {
            var view = new IconsView();
            view.DataContext = new IconsViewModel();
            Content = view;
        }

        private object _content;

        public object Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }
    }
}
