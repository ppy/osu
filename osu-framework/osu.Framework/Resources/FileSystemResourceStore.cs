//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Globalization;
using System.IO;

namespace osu.Framework.Resources
{
    public class FileSystemResourceStore : ChangeableResourceStore<byte[]>, IDisposable
    {
        private FileSystemWatcher watcher;
        private string directory;

        private bool isDisposed;

        public FileSystemResourceStore(string directory)
        {
            this.directory = directory;

            watcher = new FileSystemWatcher(directory) { EnableRaisingEvents = true };
            watcher.Renamed += watcherChanged;
            watcher.Changed += watcherChanged;
            watcher.Created += watcherChanged;
        }

        #region Disposal
        ~FileSystemResourceStore()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed)
                return;
            isDisposed = true;

            watcher.Renamed -= watcherChanged;
            watcher.Changed -= watcherChanged;
            watcher.Created -= watcherChanged;

            watcher.Dispose();
        }
        #endregion

        private void watcherChanged(object sender, FileSystemEventArgs e)
        {
            TriggerOnChanged(e.FullPath.Replace(directory, string.Empty));
        }

        public override byte[] Get(string name)
        {
            return File.ReadAllBytes(Path.Combine(directory, name));
        }
    }
}
