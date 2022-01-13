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

        private readonly RealmContextFactory realmFactory;

        /// <summary>
        /// Construct a new instance of live realm data.
        /// </summary>
        /// <param name="data">The realm data.</param>
        /// <param name="realmFactory">The realm factory the data was sourced from. May be null for an unmanaged object.</param>
        public RealmLive(T data, RealmContextFactory realmFactory)
        {
            this.data = data;
            this.realmFactory = realmFactory;

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

            using (var realm = realmFactory.CreateContext())
                perform(realm.Find<T>(ID));
        }

        /// <summary>
        /// Perform a read operation on this live object.
        /// </summary>
        /// <param name="perform">The action to perform.</param>
        public TReturn PerformRead<TReturn>(Func<T, TReturn> perform)
        {
            if (!IsManaged)
                return perform(data);

            using (var realm = realmFactory.CreateContext())
            {
                var returnData = perform(realm.Find<T>(ID));

                if (returnData is RealmObjectBase realmObject && realmObject.IsManaged)
                    throw new InvalidOperationException(@$"Managed realm objects should not exit the scope of {nameof(PerformRead)}.");

                return returnData;
            }
        }

        /// <summary>
        /// Perform a write operation on this live object.
        /// </summary>
        /// <param name="perform">The action to perform.</param>
        public void PerformWrite(Action<T> perform)
        {
            if (!IsManaged)
                throw new InvalidOperationException(@"Can't perform writes on a non-managed underlying value");

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

                return realmFactory.Context.Find<T>(ID);
            }
        }

        public bool Equals(ILive<T>? other) => ID == other?.ID;

        public override string ToString() => PerformRead(i => i.ToString());
    }
}
