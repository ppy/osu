// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using osu.Framework.Platform;

namespace osu.Game.Database
{
    public abstract class DatabaseBackedStore
    {
        protected readonly Storage Storage;

        /// <summary>
        /// Create a new <see cref="OsuDbContext"/> instance (separate from the shared context via <see cref="GetContext"/> for performing isolated operations.
        /// </summary>
        protected readonly Func<OsuDbContext> CreateContext;

        private readonly ThreadLocal<OsuDbContext> queryContext;

        /// <summary>
        /// Refresh an instance potentially from a different thread with a local context-tracked instance.
        /// </summary>
        /// <param name="obj">The object to use as a reference when negotiating a local instance.</param>
        /// <param name="lookupSource">An optional lookup source which will be used to query and populate a freshly retrieved replacement. If not provided, the refreshed object will still be returned but will not have any includes.</param>
        /// <typeparam name="T">A valid EF-stored type.</typeparam>
        protected virtual void Refresh<T>(ref T obj, IEnumerable<T> lookupSource = null) where T : class, IHasPrimaryKey
        {
            var context = GetContext();

            if (context.Entry(obj).State != EntityState.Detached) return;

            var id = obj.ID;
            obj = lookupSource?.SingleOrDefault(t => t.ID == id) ?? context.Find<T>(id);
            context.Entry(obj).Reload();
        }

        /// <summary>
        /// Retrieve a shared context for performing lookups (or write operations on the update thread, for now).
        /// </summary>
        protected OsuDbContext GetContext() => queryContext.Value;

        protected DatabaseBackedStore(Func<OsuDbContext> createContext, Storage storage = null)
        {
            CreateContext = createContext;

            // todo: while this seems to work quite well, we need to consider that contexts could enter a state where they are never cleaned up.
            queryContext = new ThreadLocal<OsuDbContext>(CreateContext);

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
