using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Controls;
using System.Windows.Input;
using HYSoft.Presentation.Interactivity;
using HYSoft.Presentation.Styles.Controls;
using HYSoft.Presentation.Styles.Icons;

namespace Docs.Mvvm.Styles
{
    public class IconsViewModel : NotifyPropertyChangedBase
    {
        private ObservableCollection<EIconKeys> _keys = new ObservableCollection<EIconKeys>();
        public ObservableCollection<EIconKeys> Keys
        {
            get => _keys;
            set => SetProperty(ref _keys, value);
        }

        public IconsViewModel()
        {
            foreach (EIconKeys key in Enum.GetValues(typeof(EIconKeys)))
            {
                _keys.Add(key);
            }
        }

        private ICommand _searchTextChanged;
        public ICommand SearchTextChanged => _searchTextChanged ?? new RelayCommand<EventPayload>((p) =>
        {
            if (p?.Sender is not HyTextBox textBox) return;
            var text = textBox.Text.ToLower();

            _keys.Clear();
            foreach (EIconKeys key in Enum.GetValues(typeof(EIconKeys)))
            {
                if (key.ToString().ToLower().Contains(text))
                {
                    _keys.Add(key);
                }
            }
        });
    }
}
