//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.IO;
using osu.Framework.IO;

namespace osu.Framework.Configuration
{
    class SafeWriteStream : FileStream
    {
        static object SafeLock = new object(); //ensure we are only ever writing one stream to disk at a time, application wide.

        private bool aborted;

        string finalFilename;
        string temporaryFilename => base.Name;

        public SafeWriteStream(string filename) : base(filename + "." + DateTime.Now.Ticks, FileMode.Create)
        {
            finalFilename = filename;
        }

        ~SafeWriteStream()
        {
            if (!isDisposed) Dispose();
        }

        internal void Abort()
        {
            aborted = true;
        }

        public override void Close()
        {
            lock (SafeLock)
            {
                base.Close();

                if (!File.Exists(temporaryFilename)) return;

                if (aborted)
                {
                    FileSafety.FileDelete(temporaryFilename);
                    return;
                }

                try
                {
                    FileSafety.FileMove(temporaryFilename, finalFilename);
                }
                catch
                {
                }
            }
        }

        bool isDisposed;
        protected override void Dispose(bool disposing)
        {
            if (isDisposed) return;
            isDisposed = true;

            base.Dispose(disposing);
            Close();
        }
    }
}
