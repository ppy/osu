// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using osu.Framework.Platform;
using osu.Game.Extensions;
using SharpCompress.Archives.Zip;

namespace osu.Game.Database
{
    /// <summary>
    /// A class which handles exporting legacy user data of a single type from osu-stable.
    /// </summary>
    public abstract class LegacyExporter<TModel>
        where TModel : class, IHasNamedFiles
    {
        /// <summary>
        /// The file extension for exports (including the leading '.').
        /// </summary>
        protected abstract string FileExtension { get; }

        protected readonly Storage UserFileStorage;

        private readonly Storage exportStorage;

        protected LegacyExporter(Storage storage)
        {
            exportStorage = storage.GetStorageForDirectory(@"exports");
            UserFileStorage = storage.GetStorageForDirectory(@"files");
        }

        /// <summary>
        /// Exports an item to a legacy (.zip based) package.
        /// </summary>
        /// <param name="item">The item to export.</param>
        public void Export(TModel item)
        {
            string filename = $"{item.ToString().GetValidArchiveContentFilename()}{FileExtension}";

            using (var stream = exportStorage.GetStream(filename, FileAccess.Write, FileMode.Create))
                ExportModelTo(item, stream);

            exportStorage.PresentFileExternally(filename);
        }

        /// <summary>
        /// Exports an item to the given output stream.
        /// </summary>
        /// <param name="model">The item to export.</param>
        /// <param name="outputStream">The output stream to export to.</param>
        public virtual void ExportModelTo(TModel model, Stream outputStream)
        {
            using (var archive = ZipArchive.Create())
            {
                foreach (var file in model.Files)
                    archive.AddEntry(file.Filename, UserFileStorage.GetStream(file.File.GetStoragePath()));

                archive.SaveTo(outputStream);
            }
        }
    }
}
