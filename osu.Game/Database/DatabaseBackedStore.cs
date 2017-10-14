// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using osu.Framework.Logging;
using osu.Framework.Platform;

namespace osu.Game.Database
{
    public abstract class DatabaseBackedStore
    {
        protected readonly Storage Storage;
        protected readonly OsuDbContext Connection;

        protected DatabaseBackedStore(OsuDbContext connection, Storage storage = null)
        {
            Storage = storage;
            Connection = connection;
            Connection.Database.SetCommandTimeout(new TimeSpan(TimeSpan.TicksPerSecond * 10));

            try
            {
                Prepare();
            }
            catch (Exception e)
            {
                Logger.Error(e, $@"Failed to initialise the {GetType()}! Trying again with a clean database...");
                Prepare(true);
            }
        }

        /// <summary>
        /// Perform any common startup tasks. Runs after <see cref="Prepare(bool)"/> and <see cref="PerformMigration(int, int)"/>.
        /// </summary>
        protected virtual void StartupTasks()
        {

        }

        /// <summary>
        /// Prepare this database for use. Tables should be created here.
        /// </summary>
        protected abstract void Prepare(bool reset = false);

        /// <summary>
        /// Reset this database to a default state. Undo all changes to database and storage backings.
        /// </summary>
        public void Reset() => Prepare(true);

        public List<T> Query<T>(Func<T, bool> filter = null) where T : class
        {
            checkType(typeof(T));

            var dbSet = Connection.GetType().GetProperties().Single(property => property.PropertyType == typeof(DbSet<T>)).GetValue(Connection) as DbSet<T>;
            var query = dbSet.ToList();

            if (filter != null)
                query = query.Where(filter).ToList();

            return query;
        }

        /// <summary>
        /// Query and populate results.
        /// </summary>
        /// <param name="filter">An filter to refine results.</param>
        /// <returns></returns>
        public List<T> QueryAndPopulate<T>(Func<T, bool> filter)
            where T : class, IPopulate
        {
            checkType(typeof(T));

            var query = Query(filter);
            foreach (var item in query)
                Populate(item);
            return query;
        }

        /// <summary>
        /// Populate a database-backed item.
        /// </summary>
        /// <param name="item"></param>
        public void Populate(IPopulate item)
        {
            checkType(item.GetType());

            item.Populate(Connection);
        }

        private void checkType(Type type)
        {
            if (!ValidTypes.Contains(type))
                throw new InvalidOperationException($"The requested operation specified a type of {type}, which is invalid for this {nameof(DatabaseBackedStore)}.");
        }

        protected abstract Type[] ValidTypes { get; }
    }
}
