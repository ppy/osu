// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Scoring;

namespace osu.Game.Database
{
    public static class DatabaseWorkaroundExtensions
    {
        public static void Requery(this IHasPrimaryKey model, IDatabaseContextFactory contextFactory)
        {
            switch (model)
            {
                case ScoreInfo scoreInfo:
                    scoreInfo.Beatmap.BeatmapSet.Requery(contextFactory);
                    scoreInfo.Files.RequeryFiles(contextFactory);
                    break;

                case BeatmapSetInfo beatmapSetInfo:
                    var context = contextFactory.Get();

                    foreach (var beatmap in beatmapSetInfo.Beatmaps)
                    {
                        // Workaround System.InvalidOperationException
                        // The instance of entity type 'RulesetInfo' cannot be tracked because another instance with the same key value for {'ID'} is already being tracked.
                        beatmap.Ruleset = context.RulesetInfo.Find(beatmap.RulesetID);
                    }

                    requeryFiles(beatmapSetInfo.Files, contextFactory);
                    break;
            }
        }

        public static void RequeryFiles<T>(this List<T> files, IDatabaseContextFactory databaseContextFactory) where T : class, INamedFileInfo
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
