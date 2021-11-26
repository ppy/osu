// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
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

        public bool IsManaged { get; }

        private readonly SynchronizationContext? fetchedContext;
        private readonly int fetchedThreadId;

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

            if (data.IsManaged)
            {
                IsManaged = true;

                fetchedContext = SynchronizationContext.Current;
                fetchedThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            ID = data.ID;
        }

        /// <summary>
        /// Perform a read operation on this live object.
        /// </summary>
        /// <param name="perform">The action to perform.</param>
        public void PerformRead(Action<T> perform)
        {
            if (originalDataValid)
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

            if (originalDataValid)
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
                if (originalDataValid)
                    return data;

                T retrieved;

                using (var realm = Realm.GetInstance(data.Realm.Config))
                    retrieved = realm.Find<T>(ID);

                if (!retrieved.IsValid)
                    throw new InvalidOperationException("Attempted to access value without an open context");

                return retrieved;
            }
        }

        // TODO: Revisit adding these conditionals back as an optimisation: || (isCorrectThread && data.IsValid);
        // They have temporarily been removed due to an oversight involving .AsQueryable, see https://github.com/realm/realm-dotnet/discussions/2734.
        // This means we are fetching a new context every `PerformRead` or `PerformWrite`, even when on the correct thread.
        private bool originalDataValid => !IsManaged;

        // this matches realm's internal thread validation (see https://github.com/realm/realm-dotnet/blob/903b4d0b304f887e37e2d905384fb572a6496e70/Realm/Realm/Native/SynchronizationContextScheduler.cs#L72)
        private bool isCorrectThread
            => (fetchedContext != null && SynchronizationContext.Current == fetchedContext) || fetchedThreadId == Thread.CurrentThread.ManagedThreadId;

        public bool Equals(ILive<T>? other) => ID == other?.ID;
    }
}
