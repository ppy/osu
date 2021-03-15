// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Database
{
    /// <summary>
    /// Extension methods which contain workarounds to make EFcore 5.x work with our existing (incorrect) thread safety.
    /// The intention is to avoid blocking package updates while we consider the future of the database backend, with a potential backend switch imminent.
    /// </summary>
    public static class DatabaseWorkaroundExtensions
    {
        /// <summary>
        /// Re-query the provided model to ensure it is in a sane state. This method requires explicit implementation per model type.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="contextFactory"></param>
        public static void Requery(this IHasPrimaryKey model, IDatabaseContextFactory contextFactory)
        {
            switch (model)
            {
                case SkinInfo skinInfo:
                    requeryFiles(skinInfo.Files, contextFactory);
                    break;

                case ScoreInfo scoreInfo:
                    requeryFiles(scoreInfo.Beatmap.BeatmapSet.Files, contextFactory);
                    requeryFiles(scoreInfo.Files, contextFactory);
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

                default:
                    throw new ArgumentException($"{nameof(Requery)} does not have support for the provided model type", nameof(model));
            }

            void requeryFiles<T>(List<T> files, IDatabaseContextFactory databaseContextFactory) where T : class, INamedFileInfo
            {
                var dbContext = databaseContextFactory.Get();

                foreach (var file in files)
                {
                    Requery(file, dbContext);
                }
            }
        }

        public static void Requery(this INamedFileInfo file, OsuDbContext dbContext)
        {
            // Workaround System.InvalidOperationException
            // The instance of entity type 'FileInfo' cannot be tracked because another instance with the same key value for {'ID'} is already being tracked.
            file.FileInfo = dbContext.FileInfo.Find(file.FileInfoID);
        }
    }
}
