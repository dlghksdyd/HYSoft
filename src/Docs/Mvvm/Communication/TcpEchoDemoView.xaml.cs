using System.Collections.Specialized;
using System.Windows.Controls;

namespace Docs.Mvvm.Communication
{
    /// <summary>
    /// Communication › TCP › Live Echo Demo 페이지.
    /// </summary>
    /// <remarks>
    /// 🔴 정리는 <c>Unloaded</c> 가 아니라 <see cref="Dispose"/> 로 한다 — 페이지를 바꾸는 쪽
    /// (<see cref="BottomSharedContext"/>)이 교체 즉시 부른다. Unloaded 에 맡기면 실측에서 페이지를 떠난 뒤
    /// 2초가 지나도 리스너가 남아 있었다(2026-11-10). 정리 시점이 이벤트 배달에 달려 있으면 안 된다.
    /// </remarks>
    public partial class TcpEchoDemoView : UserControl, IDisposable
    {
        private readonly TcpEchoDemoViewModel _viewModel = new TcpEchoDemoViewModel();
        private bool _disposed;

        public TcpEchoDemoView(IBottomSharedContext context)
        {
            InitializeComponent();
            DataContext = _viewModel;

            _viewModel.Logs.CollectionChanged += OnLogsChanged;
        }

        /// <summary>CLI(<c>echo.*</c>)가 조작하는 뷰모델.</summary>
        public TcpEchoDemoViewModel ViewModel => _viewModel;

        private void OnLogsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add) LogScrollViewer.ScrollToEnd();
        }

        /// <summary>서버·클라이언트를 정리한다. 여러 번 불러도 한 번만 정리한다.</summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _viewModel.Logs.CollectionChanged -= OnLogsChanged;
            _viewModel.Dispose();
        }
    }
}
