
using System.Threading;
using System.Windows.Forms;

namespace MGAlienLib
{
    public class FormsUtility
    {
        private string _selectedFilePath;
        private bool _fileDialogResult;
        private ManualResetEvent _fileDialogEvent = new ManualResetEvent(false);
        private string initialDirectory = string.Empty;
        // ... 기존 코드 ...

        public void OpenFileExplorerSTA(string defaultPath)
        {
            _selectedFilePath = null;
            _fileDialogResult = false;
            _fileDialogEvent.Reset();
            initialDirectory ??= defaultPath;

            Thread staThread = new Thread(() =>
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.InitialDirectory = initialDirectory;
                    openFileDialog.Filter = "텍스트 파일 (*.txt)|*.txt|모든 파일 (*.*)|*.*";
                    openFileDialog.FilterIndex = 2;
                    openFileDialog.RestoreDirectory = true;

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        _selectedFilePath = openFileDialog.FileName;
                        _fileDialogResult = true;

                        initialDirectory = System.IO.Path.GetDirectoryName(_selectedFilePath);
                    }
                    else
                    {
                        _fileDialogResult = false;
                    }
                    _fileDialogEvent.Set(); // STA 스레드 작업 완료 알림
                }
            });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();

            // STA 스레드가 완료될 때까지 대기
            _fileDialogEvent.WaitOne();

            // 파일 다이얼로그 결과 처리 (메인 스레드)
            if (_fileDialogResult)
            {
                System.Console.WriteLine("선택된 파일 (STA): " + _selectedFilePath);
                // 선택된 파일 경로를 사용하여 원하는 작업 수행
            }
            else
            {
                System.Console.WriteLine("파일 선택 취소 (STA)");
            }
        }
    }
}
