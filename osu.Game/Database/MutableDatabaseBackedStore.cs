using System;
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
            ItemRemoved?.Invoke(item);

            using (var usage = ContextFactory.GetForWrite())
                usage.Context.Update(item);

            ItemAdded?.Invoke(item);
        }

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

        public bool Undelete(T item)
        {
            using (ContextFactory.GetForWrite())
            {
                Refresh(ref item);

                if (!item.DeletePending) return false;

                item.DeletePending = false;
            }

            ItemAdded?.Invoke(item);
            return true;
        }
    }
}
