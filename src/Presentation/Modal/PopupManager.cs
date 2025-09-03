using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace HYSoft.Presentation.Modal
{
    public static class PopupManager
    {
        public static PopupBaseView? View { get; set; }
        private static PopupBaseViewModel? ViewModel { get; set; }
        
        private static readonly Dictionary<object, TaskCompletionSource<PopupResult>> _pending = new();

        public static void Configure(Brush background)
        {
            if (background == null) throw new ArgumentNullException(nameof(background));

            View = new PopupBaseView();
            ViewModel = new PopupBaseViewModel(background);
            View.DataContext = ViewModel;
        }

        public static void RegisterView<TView, TViewModel>()
        {
            if (View == null || ViewModel == null)
                throw new InvalidOperationException("PopupManager.Configure() must be called before registering views.");
            
            // DataTemplate: DataType = PopupInfoViewModel, VisualTree = PopupInfoView
            var template = new DataTemplate(typeof(TViewModel));

            var factory = new FrameworkElementFactory(typeof(TView));
            // XAML의 DataContext="{Binding}"와 동일
            factory.SetBinding(FrameworkElement.DataContextProperty, new Binding());

            template.VisualTree = factory;

            // DataTemplateKey(typeof(TViewModel))로 리소스에 등록
            var key = new DataTemplateKey(typeof(TViewModel));
            View!.Resources[key] = template;
        }

        /// <summary>
        /// 동기 모달 Open. UI 스레드에서 호출해야 합니다.
        /// </summary>
        public static PopupResult Open(object viewmodel)
        {
            if (ViewModel == null)
                throw new InvalidOperationException("PopupManager.Configure() must be called before opening popups.");

            var dispatcher = Application.Current?.Dispatcher ?? throw new InvalidOperationException("No current Application/Dispatcher found.");
            if (!dispatcher.CheckAccess())
                throw new InvalidOperationException("PopupManager.Open must be called on the UI thread.");

            // 결과 대기용 TCS 준비
            var tcs = new TaskCompletionSource<PopupResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            _pending[viewmodel] = tcs;
            
            // 팝업 열기
            ViewModel.OpenPopup(viewmodel);

            // DispatcherFrame으로 모달 대기
            var frame = new DispatcherFrame();
            tcs.Task.ContinueWith(_ => frame.Continue = false, TaskScheduler.FromCurrentSynchronizationContext());

            Dispatcher.PushFrame(frame); // 모달 대기 탈출은 Close에서 TCS 완료 시점

            return tcs.Task.Result;
        }

        /// <summary>
        /// 팝업 닫기(+ 모달 결과 전달). 팝업 내부 OK/Cancel 버튼 커맨드 등에서 호출.
        /// </summary>
        public static void Close(object viewmodel, PopupResult result = PopupResult.None)
        {
            if (ViewModel == null) return;

            if (_pending.Remove(viewmodel, out var tcs))
                tcs.TrySetResult(result);

            ViewModel.ClosePopup(viewmodel);
        }
    }
}
