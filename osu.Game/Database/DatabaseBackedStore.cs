// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Threading;
using osu.Framework.Logging;
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
        /// Retrieve a shared context for performing lookups (or write operations on the update thread, for now).
        /// </summary>
        protected OsuDbContext GetContext() => queryContext.Value;

        protected DatabaseBackedStore(Func<OsuDbContext> createContext, Storage storage = null)
        {
            CreateContext = createContext;

            // todo: while this seems to work quite well, we need to consider that contexts could enter a state where they are never cleaned up.
            queryContext = new ThreadLocal<OsuDbContext>(CreateContext);

            Storage = storage;

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
        /// Perform any common clean-up tasks. Should be run when idle, or whenever necessary.
        /// </summary>
        public virtual void Cleanup()
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
    }
}
