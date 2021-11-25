using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Framework.Platform;
using osu.Game.Scoring;

namespace osu.Game.Database
{
    public class StableScoreImporter : StableImporter<ScoreInfo>
    {
        protected override string ImportFromStablePath => Path.Combine("Data", "r");

        protected override IEnumerable<string> GetStableImportPaths(Storage storage)
            => storage.GetFiles(ImportFromStablePath).Where(p => Importer.HandledExtensions.Any(ext => Path.GetExtension(p)?.Equals(ext, StringComparison.OrdinalIgnoreCase) ?? false))
                      .Select(path => storage.GetFullPath(path));

        public StableScoreImporter(IModelImporter<ScoreInfo> importer)
            : base(importer)
        {
        }
    }
}
