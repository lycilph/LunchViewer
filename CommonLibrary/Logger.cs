using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace CommonLibrary
{
    public static class Logger
    {
        private static string LogFileName = "AppLog";

        public static async Task AddAsync(string line)
        {
            var log_file = await ApplicationData.Current.LocalFolder.CreateFileAsync(LogFileName, CreationCollisionOption.OpenIfExists);
            await FileIO.AppendTextAsync(log_file, line);
        }

        public static async Task<IList<string>> Read()
        {
            var log_file = await ApplicationData.Current.LocalFolder.CreateFileAsync(LogFileName, CreationCollisionOption.OpenIfExists);
            return await FileIO.ReadLinesAsync(log_file);
        }

        public static async Task Clear()
        {
            var log_file = await ApplicationData.Current.LocalFolder.CreateFileAsync(LogFileName, CreationCollisionOption.OpenIfExists);
            await log_file.DeleteAsync();
        }
    }
}
