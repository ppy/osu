// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using osu.Framework.Platform;

namespace osu.Game.Database
{
    /// <summary>
    /// A typed store which supports basic addition, deletion and updating for soft-deletable models.
    /// </summary>
    /// <typeparam name="T">The databased model.</typeparam>
    public abstract class MutableDatabaseBackedStore<T> : DatabaseBackedStore
        where T : class, IHasPrimaryKey, ISoftDelete
    {
        public event Action<T> ItemAdded;
        public event Action<T> ItemRemoved;

        protected MutableDatabaseBackedStore(IDatabaseContextFactory contextFactory, Storage storage = null)
            : base(contextFactory, storage)
        {
        }

        /// <summary>
        /// Access items pre-populated with includes for consumption.
        /// </summary>
        public IQueryable<T> ConsumableItems => AddIncludesForConsumption(ContextFactory.Get().Set<T>());

        /// <summary>
        /// Add a <see cref="T"/> to the database.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Add(T item)
        {
            using (var usage = ContextFactory.GetForWrite())
            {
                var context = usage.Context;
                context.Attach(item);
            }

            ItemAdded?.Invoke(item);
        }

        /// <summary>
        /// Update a <see cref="T"/> in the database.
        /// </summary>
        /// <param name="item">The item to update.</param>
        public void Update(T item)
        {
            using (var usage = ContextFactory.GetForWrite())
                usage.Context.Update(item);

            ItemRemoved?.Invoke(item);
            ItemAdded?.Invoke(item);
        }

        /// <summary>
        /// Delete a <see cref="T"/> from the database.
        /// </summary>
        /// <param name="item">The item to delete.</param>
        public bool Delete(T item)
        {
            using (ContextFactory.GetForWrite())
            {
                Refresh(ref item);

                if (item.DeletePending) return false;

                item.DeletePending = true;
            }

            ItemRemoved?.Invoke(item);
            return true;
        }

        /// <summary>
        /// Restore a <see cref="T"/> from a deleted state.
        /// </summary>
        /// <param name="item">The item to undelete.</param>
        public bool Undelete(T item)
        {
            using (ContextFactory.GetForWrite())
            {
                Refresh(ref item, ConsumableItems);

                if (!item.DeletePending) return false;

                item.DeletePending = false;
            }

            ItemAdded?.Invoke(item);
            return true;
        }

        /// <summary>
        /// Allow implementations to add database-side includes or constraints when querying for consumption of items.
        /// </summary>
        /// <param name="query">The input query.</param>
        /// <returns>A potentially modified output query.</returns>
        protected virtual IQueryable<T> AddIncludesForConsumption(IQueryable<T> query) => query;

        /// <summary>
        /// Allow implementations to add database-side includes or constraints when deleting items.
        /// Included properties could then be subsequently deleted by overriding <see cref="Purge"/>.
        /// </summary>
        /// <param name="query">The input query.</param>
        /// <returns>A potentially modified output query.</returns>
        protected virtual IQueryable<T> AddIncludesForDeletion(IQueryable<T> query) => query;

        /// <summary>
        /// Called when removing an item completely from the database.
        /// </summary>
        /// <param name="items">The items to be purged.</param>
        /// <param name="context">The write context which can be used to perform subsequent deletions.</param>
        protected virtual void Purge(List<T> items, OsuDbContext context) => context.RemoveRange(items);

        public override void Cleanup()
        {
            base.Cleanup();
            PurgeDeletable();
        }

        /// <summary>
        /// Purge items in a pending delete state.
        /// </summary>
        /// <param name="query">An optional query limiting the scope of the purge.</param>
        public void PurgeDeletable(Expression<Func<T, bool>> query = null)
        {
            using (var usage = ContextFactory.GetForWrite())
            {
                var context = usage.Context;

                var lookup = context.Set<T>().Where(s => s.DeletePending);

                if (query != null) lookup = lookup.Where(query);

                lookup = AddIncludesForDeletion(lookup);

                var purgeable = lookup.ToList();

                if (!purgeable.Any()) return;

                Purge(purgeable, context);
            }
        }
    }
}
