// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Realms;

#nullable enable

namespace osu.Game.Database
{
    /// <summary>
    /// Provides a method of working with unmanaged realm objects.
    /// Usually used for testing purposes where the instance is never required to be managed.
    /// </summary>
    /// <typeparam name="T">The underlying object type.</typeparam>
    public class RealmLiveUnmanaged<T> : ILive<T> where T : RealmObjectBase, IHasGuidPrimaryKey
    {
        /// <summary>
        /// Construct a new instance of live realm data.
        /// </summary>
        /// <param name="data">The realm data.</param>
        public RealmLiveUnmanaged(T data)
        {
            Value = data;
        }

        public bool Equals(ILive<T>? other) => ID == other?.ID;

        public override string ToString() => Value.ToString();

        public Guid ID => Value.ID;

        public void PerformRead(Action<T> perform) => perform(Value);

        public TReturn PerformRead<TReturn>(Func<T, TReturn> perform) => perform(Value);

        public void PerformWrite(Action<T> perform) => throw new InvalidOperationException(@"Can't perform writes on a non-managed underlying value");

        public bool IsManaged => false;

        /// <summary>
        /// The original live data used to create this instance.
        /// </summary>
        public T Value { get; }
    }
}
