
using System;
using System.Diagnostics;

namespace MGAlienLib
{
    /// <summary>
    /// 로그를 남기는 클래스입니다.
    /// </summary>
    public class Logger
    {
        /// <summary>
        /// 로그를 남깁니다.
        /// </summary>
        /// <param name="str"></param>
        public static void Log(string str)
        {
            // Log 를 호출한 filename 과 line number 만 가져오기
            str += GetStackInfo();
            Pipe?.Invoke(str);
        }

        public static string GetStackInfo()
        {
            var stackTrace = new StackTrace(true); // true로 하면 파일 이름과 줄 번호 포함
            StackFrame frame = null;
            string str = "";

            // 0: Log() 메서드, 1: Log()를 호출한 곳
            for (int i = 1; i < stackTrace.FrameCount; i++)
            {
                var f = stackTrace.GetFrame(i);
                var method = f.GetMethod();
                if (method.DeclaringType != typeof(Logger))
                {
                    frame = f;
                    break;
                }
            }

            if (frame != null)
            {
                string fileName = System.IO.Path.GetFileName(frame.GetFileName());
                int lineNumber = frame.GetFileLineNumber();
                str += $" [{fileName}:{lineNumber}]";
            }

            return str;
        }

        /// <summary>
        /// 로그를 남깁니다.
        /// </summary>
        /// <param name="msg"></param>
        public static void Log(object msg)
        {
            string str = msg.ToString();
            Log(str);
        }

        /// <summary>
        /// 로그를 남길 때 pipe 에 추가된 함수를 실행합니다.
        /// </summary>
        public static Action<string> Pipe = (_) => { };

        /// <summary>
        /// log.txt 파일로 로그를 남기는, 미리 정의된 pipe 함수입니다.
        /// Logger.Pipe += Logger.PipeToLogFile; 와 같이 사용하세요.
        /// </summary>
        /// <param name="str"></param>
        public static void PipeToLogFile(string str)
        {
            var stream = System.IO.File.AppendText("log.txt");
            stream.Write($"{str}\n");
            stream.Close();
        }
    }
}
