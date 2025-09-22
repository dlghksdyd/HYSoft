using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using HYSoft.Presentation.Interactivity;

namespace Docs.Mvvm.Styles.Icons
{
    public class GettingStartedViewModel : NotifyPropertyChangedBase
    {
        public IBottomSharedContext SharedContext { get; }

        /// <summary>
        /// 디자인 타임 용
        /// </summary>
        public GettingStartedViewModel()
        {
            SharedContext = new BottomSharedContext();

        }

        public GettingStartedViewModel(IBottomSharedContext context)
        {
            SharedContext = context;
        }

        public ICommand SelectIconAssetMenuCommand => new RelayCommand<EventPayload>((p) =>
        {
            var item = SharedContext.FindMenuItem(typeof(IconAssetView));
            if (item is null) return;
            SharedContext.SelectMenuItemCommand.Execute(new EventPayload(null, null, item));
        });
    }
}
