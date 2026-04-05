using HYSoft.Presentation.Interactivity;

namespace Docs.Mvvm.Styles.Components
{
    public class ComponentViewModel : NotifyPropertyChangedBase
    {
        public IBottomSharedContext? SharedContext { get; }

        public ComponentViewModel()
        {
        }

        public ComponentViewModel(IBottomSharedContext context)
        {
            SharedContext = context;
        }
    }
}
