using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace LunchViewerApp.Core
{
    public static class Logger
    {
        private static string LogFileName = "AppLog";

        public static async Task WriteAsync(string message)
        {
            var log_file = await ApplicationData.Current.LocalFolder.CreateFileAsync(LogFileName, CreationCollisionOption.OpenIfExists);
            
            var line = string.Format("[{0}] {1}", DateTime.Now.ToString("G"), message + Environment.NewLine);
            await FileIO.AppendTextAsync(log_file, line);
        }

        public static async Task<IList<string>> ReadAsync()
        {
            var log_file = await ApplicationData.Current.LocalFolder.CreateFileAsync(LogFileName, CreationCollisionOption.OpenIfExists);
            return await FileIO.ReadLinesAsync(log_file);
        }

        public static async Task ClearAsync()
        {
            var log_file = await ApplicationData.Current.LocalFolder.CreateFileAsync(LogFileName, CreationCollisionOption.OpenIfExists);
            await log_file.DeleteAsync();
        }
    }
}
