using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYSoft.Presentation.Interactivity
{
    /// <summary>
    /// RoutedEvent 발생 시 ICommand로 전달되는 실행 컨텍스트 정보입니다.
    /// </summary>
    public sealed class EventPayload
    {
        /// <summary>
        /// 이벤트를 발생시킨 원본 Sender를 가져옵니다.
        /// </summary>
        public object Sender { get; }

        /// <summary>
        /// 이벤트 인수를 가져옵니다.
        /// </summary>
        public EventArgs Args { get; }

        /// <summary>
        /// ICommand 실행 시 전달된 사용자 지정 파라미터를 가져옵니다.
        /// </summary>
        public object Parameter { get; }

        /// <summary>
        /// EventPayload를 초기화합니다.
        /// </summary>
        /// <param name="sender">이벤트 발생 원본</param>
        /// <param name="args">이벤트 인수</param>
        /// <param name="parameter">사용자 지정 파라미터</param>
        public EventPayload(object sender, EventArgs args, object parameter)
        {
            Sender = sender;
            Args = args;
            Parameter = parameter;
        }
    }
}
