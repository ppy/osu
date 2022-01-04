// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

#nullable enable

namespace osu.Game.Database
{
    public class EntityFrameworkLive<T> : ILive<T> where T : class
    {
        public EntityFrameworkLive(T item)
        {
            IsManaged = true; // no way to really know.
            Value = item;
        }

        public Guid ID => throw new InvalidOperationException();

        public void PerformRead(Action<T> perform)
        {
            perform(Value);
        }

        public TReturn PerformRead<TReturn>(Func<T, TReturn> perform)
        {
            return perform(Value);
        }

        public void PerformWrite(Action<T> perform)
        {
            perform(Value);
        }

        public bool IsManaged { get; }

        public T Value { get; }

        public bool Equals(ILive<T>? other) => ID == other?.ID;
    }
}
