using System.Windows.Controls;
using Docs.Mvvm.Styles.Components;

namespace Docs.Mvvm.Database
{
    /// <summary>
    /// DB Support › MSSQL › DbContextBase 문서 페이지.
    /// </summary>
    public partial class DbContextBaseView : UserControl
    {
        public DbContextBaseView(IBottomSharedContext context)
        {
            InitializeComponent();
            DataContext = new ComponentViewModel(context);
        }
    }
}
