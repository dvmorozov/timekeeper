using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace TimeKeeper.Core
{
    class PersistentData
    {
        public static async Task<Stream> GetReadFileStream(string fileName)
        {
            var roamingFolder = ApplicationData.Current.RoamingFolder;
            var file = await roamingFolder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
            var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
            return stream.AsStreamForRead();
        }

        public static async Task<Stream> GetWriteFileStream(string fileName)
        {
            var roamingFolder = ApplicationData.Current.RoamingFolder;
            var file = await roamingFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
            return stream.AsStreamForWrite();
        }
    }
}
