// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Platform;

namespace osu.Game.Database
{
    public abstract class RealmBackedStore
    {
        protected readonly Storage Storage;

        protected readonly IRealmFactory ContextFactory;

        protected RealmBackedStore(IRealmFactory contextFactory, Storage storage = null)
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
