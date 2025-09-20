using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using HYSoft.Presentation.Interactivity;

namespace Docs.Mvvm
{
    public interface IBottomSharedContext
    {
        ICommand? UpdateContent { get; set; }
    }

    public class BottomSharedContext : IBottomSharedContext
    {
        public ICommand? UpdateContent { get; set; }
    }
}
