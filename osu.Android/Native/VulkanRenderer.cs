using System;
using System.Runtime.InteropServices;

namespace osu.Android.Native
{
    public class VulkanRenderer : IDisposable
    {
        private long nativePtr;

        public VulkanRenderer()
        {
            nativePtr = nVulkanCreate();
        }

        public bool Initialize(IntPtr window) => nVulkanInitialize(nativePtr, window);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (nativePtr != 0)
            {
                nVulkanDestroy(nativePtr);
                nativePtr = 0;
            }
        }

        ~VulkanRenderer()
        {
            Dispose(false);
        }

        [DllImport("osu.Android.Native")]
        private static extern long nVulkanCreate();

        [DllImport("osu.Android.Native")]
        private static extern void nVulkanDestroy(long ptr);

        [DllImport("osu.Android.Native")]
        private static extern bool nVulkanInitialize(long ptr, IntPtr window);
    }
}
