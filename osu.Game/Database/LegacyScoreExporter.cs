// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.IO;
using System.Linq;
using osu.Framework.Platform;
using osu.Game.Extensions;
using osu.Game.Scoring;

namespace osu.Game.Database
{
    public class LegacyScoreExporter : LegacyExporter<ScoreInfo>
    {
        protected override string FileExtension => ".osr";

        public LegacyScoreExporter(Storage storage)
            : base(storage)
        {
        }

        protected override string GetFilename(ScoreInfo score)
        {
            string scoreString = score.GetDisplayString();
            string filename = $"{scoreString} ({score.Date.LocalDateTime:yyyy-MM-dd_HH-mm})";

            return filename;
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
