// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Logging;
using osu.Framework.Platform;
using SQLite.Net;

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
                Logger.Error(e, @"Failed to initialise the beatmap database! Trying again with a clean database...");
                storage.DeleteDatabase(@"beatmaps");
                Reset();
                Prepare();
            }
        }

        /// <summary>
        /// Prepare this database for use.
        /// </summary>
        protected abstract void Prepare();

        /// <summary>
        /// Reset this database to a default state. Undo all changes to database and storage backings.
        /// </summary>
        public abstract void Reset();
    }
}