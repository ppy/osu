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
    public abstract class Database
    {
        protected SQLiteConnection Connection { get; }
        protected Storage Storage { get; }

        protected Database(Storage storage, SQLiteConnection connection)
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
        }

        /// <summary>
        /// Prepare this database for use.
        /// </summary>
        protected abstract void Prepare(bool reset = false);

        /// <summary>
        /// Reset this database to a default state. Undo all changes to database and storage backings.
        /// </summary>
        public void Reset() => Prepare(true);

        public TableQuery<T> Query<T>() where T : class
        {
            return Connection.Table<T>();
        }

        /// <summary>
        /// This is expensive. Use with caution.
        /// </summary>
        public List<T> GetAllWithChildren<T>(Expression<Func<T, bool>> filter = null, bool recursive = true)
            where T : class
        {
            return Connection.GetAllWithChildren(filter, recursive);
        }

        public T GetChildren<T>(T item, bool recursive = false)
        {
            if (item == null) return default(T);

            Connection.GetChildren(item, recursive);
            return item;
        }

        protected abstract Type[] ValidTypes { get; }

        public void Update<T>(T record, bool cascade = true) where T : class
        {
            if (ValidTypes.All(t => t != typeof(T)))
                throw new ArgumentException("Must be a type managed by BeatmapDatabase", nameof(T));
            if (cascade)
                Connection.UpdateWithChildren(record);
            else
                Connection.Update(record);
        }
    }
}