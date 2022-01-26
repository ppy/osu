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
    public class RealmLiveUnmanaged<T> : Live<T> where T : RealmObjectBase, IHasGuidPrimaryKey
    {
        /// <summary>
        /// The original live data used to create this instance.
        /// </summary>
        public override T Value { get; }

        /// <summary>
        /// Construct a new instance of live realm data.
        /// </summary>
        /// <param name="data">The realm data.</param>
        public RealmLiveUnmanaged(T data)
            : base(data.ID)
        {
            if (data.IsManaged)
                throw new InvalidOperationException($"Cannot use {nameof(RealmLiveUnmanaged<T>)} with managed instances");

            Value = data;
        }

        public override void PerformRead(Action<T> perform) => perform(Value);

        public override TReturn PerformRead<TReturn>(Func<T, TReturn> perform) => perform(Value);

        public override void PerformWrite(Action<T> perform) => throw new InvalidOperationException(@"Can't perform writes on a non-managed underlying value");

        public override bool IsManaged => false;
    }
}
