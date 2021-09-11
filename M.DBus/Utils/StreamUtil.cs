using System.IO;

namespace M.DBus.Utils
{
    public static class StreamUtil
    {
        public static byte[] ToByteArray(this Stream stream)
        {
            if (stream is MemoryStream memStream)
                return memStream.ToArray();

            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}
