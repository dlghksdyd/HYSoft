using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HYSoft.Presentation.Styles.Lib
{
    internal static class DesignLog
    {
        private static readonly string LogPath =
            Path.Combine(Path.GetTempPath(), "HyIcon.Designer.log");

        private static bool InDesign =>
            DesignerProperties.GetIsInDesignMode(new DependencyObject());

        public static void Write(string msg)
        {
            try
            {
                if (!InDesign) return; // 디자인타임에서만 기록
                Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
                File.AppendAllText(LogPath,
                    $"[{DateTime.Now:HH:mm:ss.fff}] {msg}{Environment.NewLine}");
            }
            catch { /* 파일 쓰기 실패는 무시 */ }
        }

        public static void Clear()
        {
            try { if (InDesign && File.Exists(LogPath)) File.Delete(LogPath); } catch { }
        }

        public static string PathToLog => LogPath;
    }
}
