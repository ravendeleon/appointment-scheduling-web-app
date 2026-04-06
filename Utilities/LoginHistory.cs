using System;
using System.IO;

namespace SchedulingApp.Utilities
{
    public static class LoginHistory
    {
        // log file to track login attempts
        private static readonly string FileName = "Login_History.txt";

        // adds a new line to the log file each time someone logs in
        public static void Append(string username)
        {
            string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {username}";
            File.AppendAllText(FileName, line + Environment.NewLine);
        }
    }
}