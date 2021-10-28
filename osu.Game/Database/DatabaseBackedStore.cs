// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using Microsoft.EntityFrameworkCore;
using osu.Framework.Platform;

namespace osu.Game.Database
{
    public abstract class DatabaseBackedStore
    {
        protected readonly Storage Storage;

        protected readonly IDatabaseContextFactory ContextFactory;

        /// <summary>
        /// Refresh an instance potentially from a different thread with a local context-tracked instance.
        /// </summary>
        /// <param name="obj">The object to use as a reference when negotiating a local instance.</param>
        /// <param name="lookupSource">An optional lookup source which will be used to query and populate a freshly retrieved replacement. If not provided, the refreshed object will still be returned but will not have any includes.</param>
        /// <typeparam name="T">A valid EF-stored type.</typeparam>
        protected virtual void Refresh<T>(ref T obj, IQueryable<T> lookupSource = null) where T : class, IHasPrimaryKey
        {
            using (var usage = ContextFactory.GetForWrite())
            {
                var context = usage.Context;

                if (context.Entry(obj).State != EntityState.Detached) return;

                int id = obj.ID;
                var foundObject = lookupSource?.SingleOrDefault(t => t.ID == id) ?? context.Find<T>(id);
                if (foundObject != null)
                    obj = foundObject;
                else
                    context.Add(obj);
            }
        }

        protected DatabaseBackedStore(IDatabaseContextFactory contextFactory, Storage storage = null)
        {
            ContextFactory = contextFactory;
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
