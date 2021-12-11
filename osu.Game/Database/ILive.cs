// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Database
{
    /// <summary>
    /// A wrapper to provide access to database backed classes in a thread-safe manner.
    /// </summary>
    /// <typeparam name="T">The databased type.</typeparam>
    public interface ILive<T> : IEquatable<ILive<T>>
        where T : class // TODO: Add IHasGuidPrimaryKey once we don't need EF support any more.
    {
        Guid ID { get; }

        /// <summary>
        /// Perform a read operation on this live object.
        /// </summary>
        /// <param name="perform">The action to perform.</param>
        void PerformRead(Action<T> perform);

        /// <summary>
        /// Perform a read operation on this live object.
        /// </summary>
        /// <param name="perform">The action to perform.</param>
        TReturn PerformRead<TReturn>(Func<T, TReturn> perform);

        /// <summary>
        /// Perform a write operation on this live object.
        /// </summary>
        /// <param name="perform">The action to perform.</param>
        void PerformWrite(Action<T> perform);

        /// <summary>
        /// Whether this instance is tracking data which is managed by the database backing.
        /// </summary>
        bool IsManaged { get; }

        /// <summary>
        /// Resolve the value of this instance on the update thread.
        /// </summary>
        /// <remarks>
        /// After resolving, the data should not be passed between threads.
        /// </remarks>
        T Value { get; }
    }
}
