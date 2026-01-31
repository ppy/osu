using System;
using System.Runtime.InteropServices;

namespace osu.Android.Native
{
    public class VulkanRenderer : IDisposable
    {
        private long nativePtr;

        public VulkanRenderer()
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

        ~VulkanRenderer()
        {
            Dispose(false);
        }

        [DllImport("osu.Android.Native")]
        private static extern long nCreate();

        [DllImport("osu.Android.Native")]
        private static extern void nDestroy(long ptr);
    }
}
