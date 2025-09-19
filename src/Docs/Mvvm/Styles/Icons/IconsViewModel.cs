using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
using HYSoft.Presentation.Interactivity;
using HYSoft.Presentation.Styles.Icons;

namespace Docs.Mvvm.Styles.Icons
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
            foreach (EIconKeys key in System.Enum.GetValues(typeof(EIconKeys)))
            {
                _keys.Add(key);
            }
        }
    }
}
