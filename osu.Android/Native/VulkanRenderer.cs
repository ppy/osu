// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Runtime.InteropServices;
using Android.Views;

namespace osu.Android.Native
{
    public class VulkanRenderer : IDisposable
    {
        private long nativePtr;

        public VulkanRenderer()
        {
            nativePtr = nVulkanCreate();
        }

        public void Initialize(Surface surface) => nVulkanInit(nativePtr, surface);

        public void Render() => nVulkanRender(nativePtr);

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
        private static extern void nVulkanInit(long ptr, Surface surface);

        [DllImport("osu.Android.Native")]
        private static extern void nVulkanRender(long ptr);
    }
}
