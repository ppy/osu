using System;
using System.Runtime.InteropServices;

namespace osu.Android.Native
{
    public class OboeAudio : IDisposable
    {
        private long nativePtr;

        public OboeAudio()
        {
            nativePtr = nCreate();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (nativePtr != 0)
            {
                nDestroy(nativePtr);
                nativePtr = 0;
            }
        }

        ~OboeAudio()
        {
            Dispose(false);
        }

        [DllImport("osu.Android.Native")]
        private static extern long nCreate();

        [DllImport("osu.Android.Native")]
        private static extern void nDestroy(long ptr);
    }
}
