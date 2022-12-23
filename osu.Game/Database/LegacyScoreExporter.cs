// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
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

        private readonly Storage exportStorage;

        public LegacyScoreExporter(Storage storage)
            : base(storage)
        {
            exportStorage = storage.GetStorageForDirectory(@"exports");
        }

        private string GetScoreExportString(ScoreInfo score) => $"{score.GetDisplayString()} ({score.Date.LocalDateTime:yyyy-MM-dd})";

        public override void Export(ScoreInfo score)
        {
            string scoreExportTitle = GetScoreExportString(score).GetValidFilename();

            IEnumerable<string> existingExports = exportStorage.GetFiles("", $"{scoreExportTitle}*{FileExtension}");

            string scoreExportFilename = NamingUtils.GetNextBestFilename(existingExports, $"{scoreExportTitle}{FileExtension}");
            using (var stream = exportStorage.CreateFileSafely(scoreExportFilename))
                ExportModelTo(score, stream);

            exportStorage.PresentFileExternally(scoreExportFilename);
        }

        public override void ExportModelTo(ScoreInfo model, Stream outputStream)
        {
            var file = model.Files.SingleOrDefault();
            if (file == null)
                return;

            using (var inputStream = UserFileStorage.GetStream(file.File.GetStoragePath()))
                inputStream.CopyTo(outputStream);
        }
    }
}
