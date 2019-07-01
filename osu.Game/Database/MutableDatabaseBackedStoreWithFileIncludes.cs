// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using Microsoft.EntityFrameworkCore;
using osu.Framework.Platform;

namespace osu.Game.Database
{
    public abstract class MutableDatabaseBackedStoreWithFileIncludes<T, U> : MutableDatabaseBackedStore<T>
        where T : class, IHasPrimaryKey, ISoftDelete, IHasFiles<U>
        where U : INamedFileInfo
    {
        protected MutableDatabaseBackedStoreWithFileIncludes(IDatabaseContextFactory contextFactory, Storage storage = null)
            : base(contextFactory, storage)
        {
        }

        protected override IQueryable<T> AddIncludesForConsumption(IQueryable<T> query) =>
            base.AddIncludesForConsumption(query)
                .Include(s => s.Files).ThenInclude(f => f.FileInfo);

        protected override IQueryable<T> AddIncludesForDeletion(IQueryable<T> query) =>
            base.AddIncludesForDeletion(query)
                .Include(s => s.Files); // don't include FileInfo. these are handled by the FileStore itself.
    }
}
