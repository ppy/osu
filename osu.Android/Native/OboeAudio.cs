// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Runtime.InteropServices;

namespace osu.Android.Native
{
    public class OboeAudio : IDisposable
    {
        private long nativePtr;

        public OboeAudio()
        {
            nativePtr = nOboeCreate();
        }

        public double GetTimestamp() => nGetTimestamp(nativePtr);

        public void Start() => nOboeStart(nativePtr);

        public void Stop() => nOboeStop(nativePtr);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (nativePtr != 0)
            {
                nOboeDestroy(nativePtr);
                nativePtr = 0;
            }
        }

        ~OboeAudio()
        {
            Dispose(false);
        }

        [DllImport("osu.Android.Native")]
        private static extern long nOboeCreate();

        [DllImport("osu.Android.Native")]
        private static extern void nOboeDestroy(long ptr);

        [DllImport("osu.Android.Native")]
        private static extern double nGetTimestamp(long ptr);

        [DllImport("osu.Android.Native")]
        private static extern void nOboeStart(long ptr);

        [DllImport("osu.Android.Native")]
        private static extern void nOboeStop(long ptr);
    }
}
