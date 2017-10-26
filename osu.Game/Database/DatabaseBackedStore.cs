// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using Microsoft.EntityFrameworkCore;
using osu.Framework.Platform;

namespace osu.Game.Database
{
    public abstract class DatabaseBackedStore
    {
        protected readonly Storage Storage;

        /// <summary>
        /// <see cref="DatabaseContextFactory"/> for creating new <see cref="OsuDbContext"/> instances.
        /// </summary>
        protected readonly DatabaseContextFactory DbContextFactory;

        /// <summary>
        /// Refresh an instance potentially from a database context.
        /// </summary>
        /// <param name="context"><see cref="OsuDbContext"/> which is currently used</param>
        /// <param name="obj">The object to use as a reference when negotiating a local instance.</param>
        /// <typeparam name="T">A valid EF-stored type.</typeparam>
        protected virtual void Refresh<T>(OsuDbContext context, ref T obj) where T : class, IHasPrimaryKey
        {
            if (context.Entry(obj).State == EntityState.Detached)
                obj = context.Find<T>(obj.ID);
        }
        /// <summary>
        /// Retrieve new database context.
        /// </summary>
        protected OsuDbContext GetContext() => DbContextFactory.GetContext();

        protected DatabaseBackedStore(DatabaseContextFactory dbContextFactory, Storage storage = null)
        {
            DbContextFactory = dbContextFactory;

            Storage = storage;
        }

        /// <summary>
        /// Perform any common clean-up tasks. Should be run when idle, or whenever necessary.
        /// </summary>
        public virtual void Cleanup()
        {
        }
    }
}
