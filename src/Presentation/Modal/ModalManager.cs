using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using System.Linq; // added for CloseAll

namespace HYSoft.Presentation.Modal
{
    public static class ModalManager
    {
        public static ModalBaseView? View { get; set; }
        private static ModalBaseViewModel? ViewModel { get; set; }
        
        private static readonly Dictionary<object, TaskCompletionSource<ModalResult>> _pending = new();

        public static void Configure(Brush background)
        {
            if (background == null) throw new ArgumentNullException(nameof(background));

            View = new ModalBaseView();
            ViewModel = new ModalBaseViewModel(background);
            View.DataContext = ViewModel;
        }

        public static void Configure(string backgroundHex)
        {
            if (string.IsNullOrWhiteSpace(backgroundHex))
                throw new ArgumentNullException(nameof(backgroundHex));

            try
            {
                var color = (Color)ColorConverter.ConvertFromString(backgroundHex)!;
                var brush = new SolidColorBrush(color);
                Configure(brush);
            }
            catch (FormatException)
            {
                throw new ArgumentException($"배경 색상 문자열이 올바르지 않습니다. 입력값: '{backgroundHex}'", nameof(backgroundHex));
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"배경 색상 변환 중 오류 발생: {backgroundHex}", nameof(backgroundHex), ex);
            }
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
        public static ModalResult Open(object viewmodel)
        {
            if (ViewModel == null)
                throw new InvalidOperationException("PopupManager.Configure() must be called before opening popups.");

            var dispatcher = Application.Current?.Dispatcher ?? throw new InvalidOperationException("No current Application/Dispatcher found.");
            if (!dispatcher.CheckAccess())
                throw new InvalidOperationException("PopupManager.Open must be called on the UI thread.");

            View?.Focus();

            // 결과 대기용 TCS 준비
            var tcs = new TaskCompletionSource<ModalResult>(TaskCreationOptions.RunContinuationsAsynchronously);
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
        public static void Close(object viewmodel, ModalResult result = ModalResult.None)
        {
            if (ViewModel == null) return;

            if (_pending.Remove(viewmodel, out var tcs))
                tcs.TrySetResult(result);

            ViewModel.ClosePopup(viewmodel);
        }

        /// <summary>
        /// 모든 모달을 닫습니다. 아직 결과가 설정되지 않은 대기중 모달은 지정된 결과로 완료됩니다.
        /// </summary>
        public static void CloseAll(ModalResult result = ModalResult.None)
        {
            if (ViewModel == null) return;

            // 완료되지 않은 TCS 모두 처리
            foreach (var kv in _pending.ToList())
            {
                if (_pending.Remove(kv.Key, out var tcs))
                {
                    tcs.TrySetResult(result);
                }
            }

            // 열린 팝업 모두 제거
            var contents = ViewModel.PopupList?.Select(p => p.Content).Where(c => c != null).ToList();
            if (contents != null)
            {
                foreach (var vm in contents)
                {
                    ViewModel.ClosePopup(vm!);
                }
            }
        }
    }
}
