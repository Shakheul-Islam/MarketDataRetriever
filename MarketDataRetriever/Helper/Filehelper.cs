using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketDataRetriever.Helper
{
    public static class Filehelper
    {
        static readonly object Locker = new object();
        private static bool blLogThreshold = false;

        public static string GetExcelFolderPath()
        {
            return System.Configuration.ConfigurationManager.AppSettings["excelfolderpath"];
        }
        public static string GetLogFolderPath()
        {
            return System.Configuration.ConfigurationManager.AppSettings["logfolderpath"];
        }
        private static string GetLogFilePath()
        {
            return System.Configuration.ConfigurationManager.AppSettings["logfilepath"];
        }

        public static void LogWrite(string text)
        {
            lock (Locker)
            {
                LogWrite(text, null);
            }
        }
        public static void LogWrite(string text, Exception exception)
        {
            lock (Locker)
            {
                try
                {
                    string logfolderpath = GetLogFolderPath();
                    CheckAndCreatePath(logfolderpath);
                    string filePath = GetLogFilePath();
                    CheckAndCreateLogFilePath(filePath);

                    using (StreamWriter writer = new StreamWriter(filePath, true))
                    {
                        if (!blLogThreshold)
                        {
                            FileInfo fileInfo = new FileInfo(filePath);
                            int maxFileSize = GetMaxLogFileSize() * 1024;
                            long length = fileInfo.Length;
                            if (fileInfo.Length > maxFileSize)
                            {
                                blLogThreshold = true;
                                writer.WriteLine("Log file threshold reached!");
                                writer.Dispose();
                                CreateNewLogFile(filePath, text);
                                return;
                            }

                            writer.Write(DateTime.Now + " : ");
                            writer.WriteLine(text);
                            if (exception != null)
                            {
                                writer.WriteLine("Error : " + exception.ToString());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Don't close this window. Please contact Dimik IT.\n" + ex.ToString());
                }
            }
        }
        public static void CreateNewLogFile(string path, string text)
        {
            try
            {
                string currentTime = DateTime.Now.ToString();
                currentTime = currentTime.Replace('/', '_');
                currentTime = currentTime.Replace(' ', '_');
                currentTime = currentTime.Replace(':', '_');
                string currentFileName = "_" + currentTime + ".txt";

                string currentFilePath = path.Replace(".txt", currentFileName);

                if (File.Exists(path))
                {
                    File.Move(path, currentFilePath);
                }

                if (!File.Exists(path))
                {
                    using (StreamWriter str = new StreamWriter(path, true))
                    {
                        str.Write(DateTime.Now + " : ");
                        str.WriteLine(text);
                        blLogThreshold = false;
                    }
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }
        private static int GetMaxLogFileSize()
        {
            return Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["maxLogSize"]);
        }
        public static string CheckAndCreatePath(string FolderPath)
        {
            DirectoryInfo dir = new DirectoryInfo(FolderPath);

            if (!dir.Exists)
            {
                Directory.CreateDirectory(FolderPath);
            }
            return FolderPath;
        }
        public static void CheckAndCreateLogFilePath(string path)
        {
            if (!File.Exists(path))
            {
                StreamWriter sw = File.CreateText(path);
                sw.Dispose();
            }
        }
        public static string DoSomething() {
            return "Tomal Sir is fadeup with Arunav";
        }
    }
}
