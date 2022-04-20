// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Development;
using osu.Framework.Statistics;
using Realms;

#nullable enable

namespace osu.Game.Database
{
    /// <summary>
    /// Provides a method of working with realm objects over longer application lifetimes.
    /// </summary>
    /// <typeparam name="T">The underlying object type.</typeparam>
    public class RealmLive<T> : Live<T> where T : RealmObject, IHasGuidPrimaryKey
    {
        public override bool IsManaged => data.IsManaged;

        /// <summary>
        /// The original live data used to create this instance.
        /// </summary>
        private T data;

        private bool dataIsFromUpdateThread;

        private readonly RealmAccess realm;

        /// <summary>
        /// Construct a new instance of live realm data.
        /// </summary>
        /// <param name="data">The realm data.</param>
        /// <param name="realm">The realm factory the data was sourced from. May be null for an unmanaged object.</param>
        public RealmLive(T data, RealmAccess realm)
            : base(data.ID)
        {
            this.data = data;
            this.realm = realm;

            dataIsFromUpdateThread = ThreadSafety.IsUpdateThread;
        }

        /// <summary>
        /// Perform a read operation on this live object.
        /// </summary>
        /// <param name="perform">The action to perform.</param>
        public override void PerformRead(Action<T> perform)
        {
            if (!IsManaged)
            {
                perform(data);
                return;
            }

            realm.Run(r =>
            {
                if (ThreadSafety.IsUpdateThread)
                {
                    ensureDataIsFromUpdateThread();
                    perform(data);
                    return;
                }

                perform(retrieveFromID(r));
                RealmLiveStatistics.USAGE_ASYNC.Value++;
            });
        }

        /// <summary>
        /// Perform a read operation on this live object.
        /// </summary>
        /// <param name="perform">The action to perform.</param>
        public override TReturn PerformRead<TReturn>(Func<T, TReturn> perform)
        {
            if (!IsManaged)
                return perform(data);

            if (ThreadSafety.IsUpdateThread)
            {
                ensureDataIsFromUpdateThread();
                return perform(data);
            }

            return realm.Run(r =>
            {
                var returnData = perform(retrieveFromID(r));
                RealmLiveStatistics.USAGE_ASYNC.Value++;

                if (returnData is RealmObjectBase realmObject && realmObject.IsManaged)
                    throw new InvalidOperationException(@$"Managed realm objects should not exit the scope of {nameof(PerformRead)}.");

                return returnData;
            });
        }

        /// <summary>
        /// Perform a write operation on this live object.
        /// </summary>
        /// <param name="perform">The action to perform.</param>
        public override void PerformWrite(Action<T> perform)
        {
            if (!IsManaged)
                throw new InvalidOperationException(@"Can't perform writes on a non-managed underlying value");

            PerformRead(t =>
            {
                var transaction = t.Realm.BeginWrite();
                perform(t);
                transaction.Commit();
                RealmLiveStatistics.WRITES.Value++;
            });
        }

        public override T Value
        {
            get
            {
                if (!IsManaged)
                    return data;

                if (!ThreadSafety.IsUpdateThread)
                    throw new InvalidOperationException($"Can't use {nameof(Value)} on managed objects from non-update threads");

                ensureDataIsFromUpdateThread();
                return data;
            }
        }

        private void ensureDataIsFromUpdateThread()
        {
            Debug.Assert(ThreadSafety.IsUpdateThread);

            if (dataIsFromUpdateThread && !data.Realm.IsClosed)
            {
                RealmLiveStatistics.USAGE_UPDATE_IMMEDIATE.Value++;
                return;
            }

            dataIsFromUpdateThread = true;
            data = retrieveFromID(realm.Realm);
            RealmLiveStatistics.USAGE_UPDATE_REFETCH.Value++;
        }

        private T retrieveFromID(Realm realm)
        {
            var found = realm.Find<T>(ID);

            if (found == null)
            {
                // It may be that we access this from the update thread before a refresh has taken place.
                // To ensure that behaviour matches what we'd expect (the object *is* available), force
                // a refresh to bring in any off-thread changes immediately.
                realm.Refresh();
                found = realm.Find<T>(ID);
            }

            return found;
        }
    }

    internal static class RealmLiveStatistics
    {
        public static readonly GlobalStatistic<int> WRITES = GlobalStatistics.Get<int>(@"Realm", @"Live writes");
        public static readonly GlobalStatistic<int> USAGE_UPDATE_IMMEDIATE = GlobalStatistics.Get<int>(@"Realm", @"Live update read (fast)");
        public static readonly GlobalStatistic<int> USAGE_UPDATE_REFETCH = GlobalStatistics.Get<int>(@"Realm", @"Live update read (slow)");
        public static readonly GlobalStatistic<int> USAGE_ASYNC = GlobalStatistics.Get<int>(@"Realm", @"Live async read");
    }
}
