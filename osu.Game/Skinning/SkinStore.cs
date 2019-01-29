// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using Microsoft.EntityFrameworkCore;
using osu.Framework.Platform;
using osu.Game.Database;

namespace osu.Game.Skinning
{
    public class SkinStore : MutableDatabaseBackedStore<SkinInfo>
    {
        public SkinStore(DatabaseContextFactory contextFactory, Storage storage = null)
            : base(contextFactory, storage)
        {
        }

        protected override IQueryable<SkinInfo> AddIncludesForConsumption(IQueryable<SkinInfo> query) =>
            base.AddIncludesForConsumption(query)
                .Include(s => s.Files).ThenInclude(f => f.FileInfo);
    }
}
