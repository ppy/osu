// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using Microsoft.EntityFrameworkCore;
using osu.Framework.Platform;
using osu.Game.Database;

namespace osu.Game.Skinning
{
    public class SkinStore : MutableDatabaseBackedStoreWithFileIncludes<SkinInfo, SkinFileInfo>
    {
        public SkinStore(DatabaseContextFactory contextFactory, Storage storage = null)
            : base(contextFactory, storage)
        {
        }

        protected override IQueryable<SkinInfo> AddIncludesForDeletion(IQueryable<SkinInfo> query) =>
            base.AddIncludesForDeletion(query)
                .Include(s => s.Settings); // don't include FileInfo. these are handled by the FileStore itself.
    }
}
