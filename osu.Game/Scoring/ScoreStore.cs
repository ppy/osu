// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using Microsoft.EntityFrameworkCore;
using osu.Framework.Platform;
using osu.Game.Database;

namespace osu.Game.Scoring
{
    public class ScoreStore : MutableDatabaseBackedStore<ScoreInfo>
    {
        public ScoreStore(IDatabaseContextFactory factory, Storage storage)
            : base(factory, storage)
        {
        }

        protected override IQueryable<ScoreInfo> AddIncludesForConsumption(IQueryable<ScoreInfo> query)
            => base.AddIncludesForConsumption(query)
                   .Include(s => s.Files).ThenInclude(f => f.FileInfo)
                   .Include(s => s.Beatmap)
                   .Include(s => s.Ruleset);
    }
}
