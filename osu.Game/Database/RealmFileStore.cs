// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Win32.SafeHandles;
using osu.Framework;
using osu.Framework.Extensions;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Extensions;
using osu.Game.Models;
using Realms;

namespace osu.Game.Database
{
    /// <summary>
    /// Handles the storing of files to the file system (and database) backing.
    /// </summary>
    [ExcludeFromDynamicCompile]
    public class RealmFileStore
    {
        private readonly RealmAccess realm;

        public readonly IResourceStore<byte[]> Store;

        public readonly Storage Storage;

        public RealmFileStore(RealmAccess realm, Storage storage)
        {
            this.realm = realm;

            Storage = storage.GetStorageForDirectory(@"files");
            Store = new StorageBackedResourceStore(Storage);
        }

        /// <summary>
        /// Add a new file to the game-wide database, copying it to permanent storage if not already present.
        /// </summary>
        /// <param name="data">The file data stream.</param>
        /// <param name="realm">The realm instance to add to. Should already be in a transaction.</param>
        /// <param name="addToRealm">Whether the <see cref="RealmFile"/> should immediately be added to the underlying realm. If <c>false</c> is provided here, the instance must be manually added.</param>
        /// <param name="preferHardLinks">Whether this import should use hard links rather than file copy operations if available.</param>
        public RealmFile Add(Stream data, Realm realm, bool addToRealm = true, bool preferHardLinks = false)
        {
            string hash = data.ComputeSHA2Hash();

            var existing = realm.Find<RealmFile>(hash);

            var file = existing ?? new RealmFile { Hash = hash };

            if (!checkFileExistsAndMatchesHash(file))
                copyToStore(file, data, preferHardLinks);

            if (addToRealm && !file.IsManaged)
                realm.Add(file);

            return file;
        }

        private void copyToStore(RealmFile file, Stream data, bool preferHardLinks)
        {
            if (RuntimeInfo.OS == RuntimeInfo.Platform.Windows && data is FileStream fs && preferHardLinks)
            {
                // attempt to do a fast hard link rather than copy.
                if (CreateHardLink(Storage.GetFullPath(file.GetStoragePath()), fs.Name, IntPtr.Zero))
                    return;
            }

            data.Seek(0, SeekOrigin.Begin);

            using (var output = Storage.CreateFileSafely(file.GetStoragePath()))
                data.CopyTo(output);

            data.Seek(0, SeekOrigin.Begin);
        }

        private bool checkFileExistsAndMatchesHash(RealmFile file)
        {
            string path = file.GetStoragePath();

            // we may be re-adding a file to fix missing store entries.
            if (!Storage.Exists(path))
                return false;

            // even if the file already exists, check the existing checksum for safety.
            using (var stream = Storage.GetStream(path))
                return stream.ComputeSHA2Hash() == file.Hash;
        }

        public void Cleanup()
        {
            Logger.Log(@"Beginning realm file store cleanup");

            int totalFiles = 0;
            int removedFiles = 0;

            // can potentially be run asynchronously, although we will need to consider operation order for disk deletion vs realm removal.
            realm.Write(r =>
            {
                // TODO: consider using a realm native query to avoid iterating all files (https://github.com/realm/realm-dotnet/issues/2659#issuecomment-927823707)
                var files = r.All<RealmFile>().ToList();

                foreach (var file in files)
                {
                    totalFiles++;

                    if (file.BacklinksCount > 0)
                        continue;

                    try
                    {
                        removedFiles++;
                        Storage.Delete(file.GetStoragePath());
                        r.Remove(file);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, $@"Could not delete databased file {file.Hash}");
                    }
                }
            });

            Logger.Log($@"Finished realm file store cleanup ({removedFiles} of {totalFiles} deleted)");
        }

        #region Windows hard link support

        // For future use (to detect if a file is a hard link with other references existing on disk).
        public static int GetFileLinkCount(string filePath)
        {
            int result = 0;
            SafeFileHandle handle = CreateFile(filePath, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, FileAttributes.Archive, IntPtr.Zero);

            ByHandleFileInformation fileInfo;

            if (GetFileInformationByHandle(handle, out fileInfo))
                result = (int)fileInfo.NumberOfLinks;
            CloseHandle(handle);

            return result;
        }

        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern bool CreateHardLink(
            string lpFileName,
            string lpExistingFileName,
            IntPtr lpSecurityAttributes
        );

        [StructLayout(LayoutKind.Sequential)]
        private struct ByHandleFileInformation
        {
            public readonly uint FileAttributes;
            public readonly FILETIME CreationTime;
            public readonly FILETIME LastAccessTime;
            public readonly FILETIME LastWriteTime;
            public readonly uint VolumeSerialNumber;
            public readonly uint FileSizeHigh;
            public readonly uint FileSizeLow;
            public readonly uint NumberOfLinks;
            public readonly uint FileIndexHigh;
            public readonly uint FileIndexLow;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern SafeFileHandle CreateFile(
            string lpFileName,
            [MarshalAs(UnmanagedType.U4)] FileAccess dwDesiredAccess,
            [MarshalAs(UnmanagedType.U4)] FileShare dwShareMode,
            IntPtr lpSecurityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode dwCreationDisposition,
            [MarshalAs(UnmanagedType.U4)] FileAttributes dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetFileInformationByHandle(SafeFileHandle handle, out ByHandleFileInformation lpFileInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(SafeHandle hObject);

        #endregion
    }
}
