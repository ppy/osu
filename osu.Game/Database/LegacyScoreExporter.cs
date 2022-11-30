// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.IO;
using System.Linq;
using osu.Framework.Platform;
using osu.Game.Extensions;
using osu.Game.Scoring;
using osu.Game.Utils;

namespace osu.Game.Database
{
    public class LegacyScoreExporter : LegacyExporter<ScoreInfo>
    {
        protected override string FileExtension => ".osr";

        public LegacyScoreExporter(Storage storage)
            : base(storage)
        {
        }

        public override void ExportModelTo(ScoreInfo model, Stream outputStream)
        {
            var file = model.Files.SingleOrDefault();
            if (file == null)
                return;

            using (var inputStream = UserFileStorage.GetStream(file.File.GetStoragePath()))
                inputStream.CopyTo(outputStream);
        }

        public override void Export(ScoreInfo item)
        {
            var itemFilename = item.GetDisplayString().GetValidFilename();

            var existingExports = ExportStorage.GetFiles("", $"{itemFilename}*{FileExtension}").ToArray();

            // trim the file extension
            for (int i = 0; i < existingExports.Length; i++)
                existingExports[i] = existingExports[i].TrimEnd(FileExtension.ToCharArray());

            string filename = $"{NamingUtils.GetNextBestName(existingExports, itemFilename)}{FileExtension}";
            using (var stream = ExportStorage.CreateFileSafely(filename))
                ExportModelTo(item, stream);

            ExportStorage.PresentFileExternally(filename);
        }
    }
}
