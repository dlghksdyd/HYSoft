using HYSoft.Presentation.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Docs.Mvvm.Styles;

namespace Docs.Mvvm
{
    public class BottomContentsViewModel : NotifyPropertyChangedBase
    {
        public BottomContentsViewModel()
        {
            Content = new IconsView();
        }

        private object _content;
        public object Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }
    }
}
