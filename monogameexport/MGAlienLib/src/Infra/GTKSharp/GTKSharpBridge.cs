using Gtk;
using Microsoft.Xna.Framework;

namespace MGAlienLib
{

    public class GtkUtility
    {
        public static string ShowOpenFileDialog(string title, string initialDirectory = null)
        {
            // Gtk# 초기화 (이미 초기화되어 있다면 중복 호출해도 안전함)
            Application.Init();

            string selectedFilename = null;

            // 파일 열기 대화 상자 생성
            using (FileChooserDialog fileChooser = new FileChooserDialog(
                title,
                null, // 부모 창 (MonoGame 창을 부모로 설정할 수도 있습니다)
                FileChooserAction.Open,
                "취소", ResponseType.Cancel,
                "열기", ResponseType.Ok))
            {
                // 초기 디렉토리 설정
                if (!string.IsNullOrEmpty(initialDirectory))
                {
                    fileChooser.SetCurrentFolder(initialDirectory);
                }

                // 대화 상자 실행 (모달 방식)
                ResponseType response = (ResponseType)fileChooser.Run();

                // 결과 확인
                if (response == ResponseType.Ok)
                {
                    selectedFilename = fileChooser.Filename;
                }

                // 대화 상자 파괴 (리소스 해제)
                //fileChooser.Destroy();
            }

            // Gtk# 종료 (선택 사항 - 다른 Gtk# 기능을 사용하지 않을 경우)
            // Application.Quit();

            return selectedFilename;
        }

        public static string ShowSaveFileDialog(string title, string initialDirectory = null)
        {
            // Gtk# 초기화 (이미 초기화되어 있다면 중복 호출해도 안전함)
            Application.Init();
            string selectedFilename = null;
            // 파일 저장 대화 상자 생성
            using (FileChooserDialog fileChooser = new FileChooserDialog(
                title,
                null, // 부모 창 (MonoGame 창을 부모로 설정할 수도 있습니다)
                FileChooserAction.Save,
                "취소", ResponseType.Cancel,
                "저장", ResponseType.Ok))
            {
                // 초기 디렉토리 설정
                if (!string.IsNullOrEmpty(initialDirectory))
                {
                    fileChooser.SetCurrentFolder(initialDirectory);
                }

                // 대화 상자 실행 (모달 방식)
                ResponseType response = (ResponseType)fileChooser.Run();
                // 결과 확인
                if (response == ResponseType.Ok)
                {
                    selectedFilename = fileChooser.Filename;
                }
                // 대화 상자 파괴 (리소스 해제)
                //fileChooser.Destroy();
            }
            // Gtk# 종료 (선택 사항 - 다른 Gtk# 기능을 사용하지 않을 경우)
            // Application.Quit();
            return selectedFilename;
        }

        public static Color? ShowColorPickerDialog(string title, Color oldColor, bool withAlpha = true)
        {
            // Gtk# 초기화 (이미 초기화되어 있다면 중복 호출해도 안전함)
            Application.Init();

            Gdk.RGBA selectedRgba = new Gdk.RGBA();
            bool colorSelected = false;

            using (ColorChooserDialog dialog = new ColorChooserDialog(title, null))
            {
                dialog.Modal = true;
                dialog.UseAlpha = withAlpha;

                // MonoGame Color를 Gdk.RGBA로 변환하여 초기 색상 설정
                var gdkColor = new Gdk.RGBA();
                gdkColor.Red = oldColor.R / 255.0;
                gdkColor.Green = oldColor.G / 255.0;
                gdkColor.Blue = oldColor.B / 255.0;
                gdkColor.Alpha = oldColor.A / 255.0;
                dialog.Rgba = gdkColor;

                if (dialog.Run() == (int)ResponseType.Ok)
                {
                    selectedRgba = dialog.Rgba;
                    colorSelected = true;
                }

                //dialog.Destroy();
            }

            if (colorSelected)
            {
                return new Color(
                    (float)selectedRgba.Red,
                    (float)selectedRgba.Green,
                    (float)selectedRgba.Blue,
                    (float)selectedRgba.Alpha);
            }
            else
            {
                return null;
            }

            // Application.Quit()을 호출하지 않습니다. 파일 열기 대화 상자와 동일한 방식으로 처리합니다.
        }
    }
}
