// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Scoring;

namespace osu.Game.Database
{
    public static class Extensions
    {
        public static void Requery(this BeatmapSetInfo beatmapSetInfo, IDatabaseContextFactory databaseContextFactory)
        {
            var dbContext = databaseContextFactory.Get();

            foreach (var beatmap in beatmapSetInfo.Beatmaps)
            {
                // Workaround System.InvalidOperationException
                // The instance of entity type 'RulesetInfo' cannot be tracked because another instance with the same key value for {'ID'} is already being tracked.
                beatmap.Ruleset = dbContext.RulesetInfo.Find(beatmap.RulesetID);
            }

            beatmapSetInfo.Files.Requery(databaseContextFactory);
        }

        public static void Requery(this ScoreInfo scoreInfo, IDatabaseContextFactory databaseContextFactory)
        {
            scoreInfo.Files.Requery(databaseContextFactory);
            scoreInfo.Beatmap.BeatmapSet.Files.Requery(databaseContextFactory);
        }

        public static void Requery<T>(this List<T> files, IDatabaseContextFactory databaseContextFactory) where T : class, INamedFileInfo
        {
            var dbContext = databaseContextFactory.Get();

            foreach (var file in files)
            {
                // Workaround System.InvalidOperationException
                // The instance of entity type 'FileInfo' cannot be tracked because another instance with the same key value for {'ID'} is already being tracked.
                file.FileInfo = dbContext.FileInfo.Find(file.FileInfoID);
            }
        }
    }
}
