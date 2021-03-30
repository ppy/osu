// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using Microsoft.EntityFrameworkCore;
using osu.Framework.Platform;
using osu.Game.Database;

namespace osu.Game.Scoring
{
    public class ScoreStore : MutableDatabaseBackedStoreWithFileIncludes<ScoreInfo, ScoreFileInfo>
    {
        public ScoreStore(IDatabaseContextFactory factory, Storage storage)
            : base(factory, storage)
        {
        }

        protected override IQueryable<ScoreInfo> AddIncludesForConsumption(IQueryable<ScoreInfo> query)
            => base.AddIncludesForConsumption(query)
                   .Include(s => s.Beatmap)
                   .Include(s => s.Beatmap).ThenInclude(b => b.Metadata)
                   .Include(s => s.Beatmap).ThenInclude(b => b.BeatmapSet).ThenInclude(s => s.Metadata)
                   .Include(s => s.Ruleset);
    }
}
