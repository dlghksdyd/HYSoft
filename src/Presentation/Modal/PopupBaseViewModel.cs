using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace HYSoft.Presentation.Modal
{
    public class PopupBaseViewModel(Brush background) : BindableBase
    {
        private ObservableCollection<PopupInfo> _popupList = [];
        public ObservableCollection<PopupInfo> PopupList
        {
            get => _popupList;
            set => SetProperty(ref _popupList, value);
        }

        public void OpenPopup(object popupViewModel)
        {
            var info = new PopupInfo()
            {
                Content = popupViewModel,
                Background = (SolidColorBrush)background
            };
            PopupList.Add(info);
        }

        public void ClosePopup(object popupViewModel)
        {
            var info = PopupList.ToList().Find(p => ReferenceEquals(p.Content, popupViewModel));
            if (info != null) PopupList.Remove(info);
        }
    }

    public class PopupInfo : BindableBase
    {
        private object? _content;
        public object? Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        private Brush _background = new SolidColorBrush(Color.FromArgb(0x4C, 0xD1, 0xE8, 0xEF));
        public Brush Background
        {
            get => _background;
            set => SetProperty(ref _background, value);
        }
    }
}
