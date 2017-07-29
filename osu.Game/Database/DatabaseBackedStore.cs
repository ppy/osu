// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using osu.Framework.Logging;
using osu.Framework.Platform;
using SQLite.Net;
using SQLiteNetExtensions.Extensions;

namespace osu.Game.Database
{
    public abstract class DatabaseBackedStore
    {
        protected readonly Storage Storage;
        protected readonly SQLiteConnection Connection;

        protected virtual int StoreVersion => 1;

        protected DatabaseBackedStore(SQLiteConnection connection, Storage storage = null)
        {
            Storage = storage;
            Connection = connection;

            try
            {
                Prepare();
            }
            catch (Exception e)
            {
                Logger.Error(e, $@"Failed to initialise the {GetType()}! Trying again with a clean database...");
                Prepare(true);
            }

            checkMigrations();
        }

        private void checkMigrations()
        {
            var storeName = GetType().Name;

            var reportedVersion = Connection.Table<StoreVersion>().FirstOrDefault(s => s.StoreName == storeName) ?? new StoreVersion
            {
                StoreName = storeName,
                Version = 0
            };

            if (reportedVersion.Version != StoreVersion)
                PerformMigration(reportedVersion.Version, reportedVersion.Version = StoreVersion);

            Connection.InsertOrReplace(reportedVersion);
        }

        protected virtual void PerformMigration(int currentVersion, int newVersion)
        {
        }

        /// <summary>
        /// Prepare this database for use.
        /// </summary>
        protected abstract void Prepare(bool reset = false);

        /// <summary>
        /// Reset this database to a default state. Undo all changes to database and storage backings.
        /// </summary>
        public void Reset() => Prepare(true);


        public TableQuery<T> Query<T>(Expression<Func<T, bool>> filter = null) where T : class
        {
            checkType(typeof(T));

            var query = Connection.Table<T>();

            if (filter != null)
                query = query.Where(filter);

            return query;
        }

        /// <summary>
        /// Query and populate results.
        /// </summary>
        /// <param name="filter">An optional filter to refine results.</param>
        /// <returns></returns>
        public List<T> QueryAndPopulate<T>(Expression<Func<T, bool>> filter = null)
            where T : class
        {
            checkType(typeof(T));

            return Connection.GetAllWithChildren(filter, true);
        }

        /// <summary>
        /// Populate a database-backed item.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="recursive">Whether population should recurse beyond a single level.</param>
        public void Populate<T>(T item, bool recursive = true)
        {
            checkType(item.GetType());

            Connection.GetChildren(item, recursive);
        }

        private void checkType(Type type)
        {
            if (!ValidTypes.Contains(type))
                throw new InvalidOperationException($"The requested operation specified a type of {type}, which is invalid for this {nameof(DatabaseBackedStore)}.");
        }

        protected abstract Type[] ValidTypes { get; }
    }
}
