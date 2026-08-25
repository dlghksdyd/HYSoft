# TestApp

HYSoft 라이브러리 기능을 시연하는 샘플 WPF 애플리케이션.

- **Target**: WinExe
- **의존성**: Communication, Communication.FileTransfer, Presentation, Presentation.Styles

## 샘플 3가지

1. **File Transfer** - TCP ���일 전송 클라이언트/서버 데모
   - FileTransferClientView/ViewModel, FileTransferServerView/ViewModel
2. **Modal** - 모달 ���이얼로그 사용 데모
   - ModalSampleView/ViewModel, ModalInfoView/ViewModel
3. **Icon** - 87개 머티리얼 디자인 아이콘 전시

## 진입점

MainWindow + MainWindowViewModel (RelayCommand로 샘플 간 전환)
