using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HYSoft.Presentation.Interactivity;

namespace Docs.Mvvm.LeftMenu
{
    public class LeftMenuViewModel : NotifyPropertyChangedBase
    {
        public LeftMenuViewModel()
        {
            Container.Title = "Content";

            Container.Items.Add(new MenuItem()
            {
                Title = "Icons",
            });

            Container.Items.Add(new MenuItem()
            {
                Title = "Controls",
            });
        }

        private MenuItemContainer _container = new MenuItemContainer();
        public MenuItemContainer Container
        {
            get => _container;
            set => SetProperty(ref _container, value);
        }
    }
}
