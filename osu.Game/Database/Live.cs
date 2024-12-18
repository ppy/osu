// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;

namespace osu.Game.Database
{
    /// <summary>
    /// A wrapper to provide access to database backed classes in a thread-safe manner.
    /// </summary>
    /// <typeparam name="T">The databased type.</typeparam>
    public abstract class Live<T> : IEquatable<Live<T>>
        where T : class, IHasGuidPrimaryKey
    {
        public Guid ID { get; }

        /// <summary>
        /// Perform a read operation on this live object.
        /// </summary>
        /// <param name="perform">The action to perform.</param>
        public abstract void PerformRead([InstantHandle] Action<T> perform);

        /// <summary>
        /// Perform a read operation on this live object.
        /// </summary>
        /// <param name="perform">The action to perform.</param>
        public abstract TReturn PerformRead<TReturn>([InstantHandle] Func<T, TReturn> perform);

        /// <summary>
        /// Perform a write operation on this live object.
        /// </summary>
        /// <param name="perform">The action to perform.</param>
        public abstract void PerformWrite([InstantHandle] Action<T> perform);

        /// <summary>
        /// Whether this instance is tracking data which is managed by the database backing.
        /// </summary>
        public abstract bool IsManaged { get; }

        /// <summary>
        /// Resolve the value of this instance on the update thread.
        /// </summary>
        /// <remarks>
        /// After resolving, the data should not be passed between threads.
        /// </remarks>
        public abstract T Value { get; }

        protected Live(Guid id)
        {
            ID = id;
        }

        public bool Equals(Live<T>? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;

            return ID == other.ID;
        }

        public override int GetHashCode() => HashCode.Combine(ID);

        public override string? ToString() => PerformRead(i => i.ToString());
    }
}
