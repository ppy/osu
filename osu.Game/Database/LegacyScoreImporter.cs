// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Framework.Platform;
using osu.Game.Scoring;

namespace osu.Game.Database
{
    public class LegacyScoreImporter : LegacyModelImporter<ScoreInfo>
    {
        protected override string ImportFromStablePath => Path.Combine("Data", "r");

        protected override IEnumerable<string> GetStableImportPaths(Storage storage)
        {
            if (!storage.ExistsDirectory(ImportFromStablePath))
                return Enumerable.Empty<string>();

            return storage.GetFiles(ImportFromStablePath)
                          .Where(p => Importer.HandledExtensions.Any(ext => Path.GetExtension(p).Equals(ext, StringComparison.OrdinalIgnoreCase)))
                          .Select(path => storage.GetFullPath(path));
        }

        public LegacyScoreImporter(IModelImporter<ScoreInfo> importer)
            : base(importer)
        {
        }
    }
}
