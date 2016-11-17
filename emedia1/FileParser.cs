using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace emedia1
{
    public class FileParser
    {
        public async Task<byte[]> GetResource(string path)
        {
            path = "ms-appx:///Assets/owl.jpg";
            StorageFile file =
                  await StorageFile.GetFileFromApplicationUriAsync(new Uri(path));
            byte[] fileBytes = null;
            using (IRandomAccessStreamWithContentType stream = await file.OpenReadAsync())
            {
                fileBytes = new byte[stream.Size];
                using (DataReader reader = new DataReader(stream))
                {
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(fileBytes);
                }
            }
            return fileBytes;
        }

        public string ConvertToHexa(byte[] data)
        {
            var hex = new StringBuilder(data.Length * 2);
            foreach (var b in data)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString().ToLower().ToLower().Replace(" ", "");
        }
    }
}