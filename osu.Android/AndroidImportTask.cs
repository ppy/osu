// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Threading.Tasks;
using Android.Content;
using Android.Net;
using Android.Provider;
using osu.Game.Database;

namespace osu.Android
{
    public class AndroidImportTask : ImportTask
    {
        private readonly ContentResolver contentResolver;

        private readonly Uri uri;

        private AndroidImportTask(Stream stream, string filename, ContentResolver contentResolver, Uri uri)
            : base(stream, filename)
        {
            this.contentResolver = contentResolver;
            this.uri = uri;
        }

        public override void DeleteFile()
        {
            contentResolver.Delete(uri, null, null);
        }

        public static async Task<AndroidImportTask?> Create(ContentResolver contentResolver, Uri uri)
        {
            // there are more performant overloads of this method, but this one is the most backwards-compatible
            // (dates back to API 1).

            var cursor = contentResolver.Query(uri, null, null, null, null);

            if (cursor == null)
                return null;

            if (!cursor.MoveToFirst())
                return null;

            int filenameColumn = cursor.GetColumnIndex(IOpenableColumns.DisplayName);
            string filename = cursor.GetString(filenameColumn) ?? uri.Path ?? string.Empty;

            // SharpCompress requires archive streams to be seekable, which the stream opened by
            // OpenInputStream() seems to not necessarily be.
            // copy to an arbitrary-access memory stream to be able to proceed with the import.
            var copy = new MemoryStream();

            using (var stream = contentResolver.OpenInputStream(uri))
            {
                if (stream == null)
                    return null;

                await stream.CopyToAsync(copy).ConfigureAwait(false);
            }

            return new AndroidImportTask(copy, filename, contentResolver, uri);
        }
    }
}
