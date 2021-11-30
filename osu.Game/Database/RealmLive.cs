// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Development;
using Realms;

#nullable enable

namespace osu.Game.Database
{
    /// <summary>
    /// Provides a method of working with realm objects over longer application lifetimes.
    /// </summary>
    /// <typeparam name="T">The underlying object type.</typeparam>
    public class RealmLive<T> : ILive<T> where T : RealmObject, IHasGuidPrimaryKey
    {
        public Guid ID { get; }

        public bool IsManaged => data.IsManaged;

        /// <summary>
        /// The original live data used to create this instance.
        /// </summary>
        private readonly T data;

        /// <summary>
        /// Construct a new instance of live realm data.
        /// </summary>
        /// <param name="data">The realm data.</param>
        public RealmLive(T data)
        {
            this.data = data;

            ID = data.ID;
        }

        /// <summary>
        /// Perform a read operation on this live object.
        /// </summary>
        /// <param name="perform">The action to perform.</param>
        public void PerformRead(Action<T> perform)
        {
            if (!IsManaged)
            {
                perform(data);
                return;
            }

            using (var realm = Realm.GetInstance(data.Realm.Config))
                perform(realm.Find<T>(ID));
        }

        /// <summary>
        /// Perform a read operation on this live object.
        /// </summary>
        /// <param name="perform">The action to perform.</param>
        public TReturn PerformRead<TReturn>(Func<T, TReturn> perform)
        {
            if (typeof(RealmObjectBase).IsAssignableFrom(typeof(TReturn)))
                throw new InvalidOperationException($"Realm live objects should not exit the scope of {nameof(PerformRead)}.");

            if (!IsManaged)
                return perform(data);

            using (var realm = Realm.GetInstance(data.Realm.Config))
                return perform(realm.Find<T>(ID));
        }

        /// <summary>
        /// Perform a write operation on this live object.
        /// </summary>
        /// <param name="perform">The action to perform.</param>
        public void PerformWrite(Action<T> perform)
        {
            if (!IsManaged)
                throw new InvalidOperationException("Can't perform writes on a non-managed underlying value");

            PerformRead(t =>
            {
                var transaction = t.Realm.BeginWrite();
                perform(t);
                transaction.Commit();
            });
        }

        public T Value
        {
            get
            {
                if (!IsManaged)
                    return data;

                if (!ThreadSafety.IsUpdateThread)
                    throw new InvalidOperationException($"Can't use {nameof(Value)} on managed objects from non-update threads");

                // When using Value, we rely on garbage collection for the realm instance used to retrieve the instance.
                // As we are sure that this is on the update thread, there should always be an open and constantly refreshing realm instance to ensure file size growth is a non-issue.
                var realm = Realm.GetInstance(data.Realm.Config);

                return realm.Find<T>(ID);
            }
        }

        public bool Equals(ILive<T>? other) => ID == other?.ID;
    }
}
