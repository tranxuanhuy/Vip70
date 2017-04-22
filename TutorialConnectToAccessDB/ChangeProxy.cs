using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace TutorialConnectToAccessDB
{
    class ChangeProxy
    {
        [DllImport("wininet.dll")]
        public static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);
        public const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
        public const int INTERNET_OPTION_REFRESH = 37;
        static bool settingsReturn, refreshReturn;
        public static void SetSockEntireComputer(string proxy)
        {
            ProcessStartInfo psi = new ProcessStartInfo("setSockEntireComputer.exe", proxy)
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            var process = System.Diagnostics.Process.Start(psi);
            process.WaitForExit();
        }
        public static void ResetProxySockEntireComputer()
        {
            ProcessStartInfo psi = new ProcessStartInfo("resetProxySockEntireComputer.exe")
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            var process = System.Diagnostics.Process.Start(psi);
            process.WaitForExit();
        }


        internal static void ResetTimezone()
        {
            ChangeTimezone("");
        }

        internal static void ChangeTimezone(string p)
        {
            string timezone;
            switch (p)
            {
                case "0800":
                    timezone = "alaskan Standard Time";
                    break;
                case "0400":
                    timezone = "Eastern Standard Time";
                    break;
                case "0500":
                    timezone = "central standard time";
                    break;
                case "0600":
                    timezone = "mountain standard time";
                    break;
                case "0700":
                    timezone = "pacific standard time";
                    break;
                default:
                    timezone = "se asia standard time";
                    break;
            }
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C tzutil /s \"" + timezone + "\"";
            process.StartInfo = startInfo;
            process.Start();
        }
    }
}
