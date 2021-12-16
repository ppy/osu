// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using osu.Framework.Development;
using osu.Game.Input.Bindings;
using Realms;

#nullable enable

namespace osu.Game.Database
{
    public static class RealmObjectExtensions
    {
        private static readonly IMapper mapper = new MapperConfiguration(c =>
        {
            c.ShouldMapField = fi => false;
            c.ShouldMapProperty = pi => pi.SetMethod != null && pi.SetMethod.IsPublic;

            c.CreateMap<RealmKeyBinding, RealmKeyBinding>();
        }).CreateMapper();

        /// <summary>
        /// Create a detached copy of the each item in the collection.
        /// </summary>
        /// <param name="items">A list of managed <see cref="RealmObject"/>s to detach.</param>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <returns>A list containing non-managed copies of provided items.</returns>
        public static List<T> Detach<T>(this IEnumerable<T> items) where T : RealmObject
        {
            var list = new List<T>();

            foreach (var obj in items)
                list.Add(obj.Detach());

            return list;
        }

        /// <summary>
        /// Create a detached copy of the item.
        /// </summary>
        /// <param name="item">The managed <see cref="RealmObject"/> to detach.</param>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <returns>A non-managed copy of provided item. Will return the provided item if already detached.</returns>
        public static T Detach<T>(this T item) where T : RealmObject
        {
            if (!item.IsManaged)
                return item;

            return mapper.Map<T>(item);
        }

        public static List<ILive<T>> ToLiveUnmanaged<T>(this IEnumerable<T> realmList)
            where T : RealmObject, IHasGuidPrimaryKey
        {
            return realmList.Select(l => new RealmLiveUnmanaged<T>(l)).Cast<ILive<T>>().ToList();
        }

        public static ILive<T> ToLiveUnmanaged<T>(this T realmObject)
            where T : RealmObject, IHasGuidPrimaryKey
        {
            return new RealmLiveUnmanaged<T>(realmObject);
        }

        public static List<ILive<T>> ToLive<T>(this IEnumerable<T> realmList, RealmContextFactory realmContextFactory)
            where T : RealmObject, IHasGuidPrimaryKey
        {
            return realmList.Select(l => new RealmLive<T>(l, realmContextFactory)).Cast<ILive<T>>().ToList();
        }

        public static ILive<T> ToLive<T>(this T realmObject, RealmContextFactory realmContextFactory)
            where T : RealmObject, IHasGuidPrimaryKey
        {
            return new RealmLive<T>(realmObject, realmContextFactory);
        }

        /// <summary>
        /// Register a callback to be invoked each time this <see cref="T:Realms.IRealmCollection`1" /> changes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This adds osu! specific thread and managed state safety checks on top of <see cref="IRealmCollection{T}.SubscribeForNotifications"/>.
        /// </para>
        /// <para>
        /// The first callback will be invoked with the initial <see cref="T:Realms.IRealmCollection`1" /> after the asynchronous query completes,
        /// and then called again after each write transaction which changes either any of the objects in the collection, or
        /// which objects are in the collection. The <c>changes</c> parameter will
        /// be <c>null</c> the first time the callback is invoked with the initial results. For each call after that,
        /// it will contain information about which rows in the results were added, removed or modified.
        /// </para>
        /// <para>
        /// If a write transaction did not modify any objects in this <see cref="T:Realms.IRealmCollection`1" />, the callback is not invoked at all.
        /// If an error occurs the callback will be invoked with <c>null</c> for the <c>sender</c> parameter and a non-<c>null</c> <c>error</c>.
        /// Currently the only errors that can occur are when opening the <see cref="T:Realms.Realm" /> on the background worker thread.
        /// </para>
        /// <para>
        /// At the time when the block is called, the <see cref="T:Realms.IRealmCollection`1" /> object will be fully evaluated
        /// and up-to-date, and as long as you do not perform a write transaction on the same thread
        /// or explicitly call <see cref="M:Realms.Realm.Refresh" />, accessing it will never perform blocking work.
        /// </para>
        /// <para>
        /// Notifications are delivered via the standard event loop, and so can't be delivered while the event loop is blocked by other activity.
        /// When notifications can't be delivered instantly, multiple notifications may be coalesced into a single notification.
        /// This can include the notification with the initial collection.
        /// </para>
        /// </remarks>
        /// <param name="collection">The <see cref="IRealmCollection{T}"/> to observe for changes.</param>
        /// <param name="callback">The callback to be invoked with the updated <see cref="T:Realms.IRealmCollection`1" />.</param>
        /// <returns>
        /// A subscription token. It must be kept alive for as long as you want to receive change notifications.
        /// To stop receiving notifications, call <see cref="M:System.IDisposable.Dispose" />.
        ///
        /// May be null in the case the provided collection is not managed.
        /// </returns>
        /// <seealso cref="M:Realms.CollectionExtensions.SubscribeForNotifications``1(System.Collections.Generic.IList{``0},Realms.NotificationCallbackDelegate{``0})" />
        /// <seealso cref="M:Realms.CollectionExtensions.SubscribeForNotifications``1(System.Linq.IQueryable{``0},Realms.NotificationCallbackDelegate{``0})" />
        public static IDisposable? QueryAsyncWithNotifications<T>(this IRealmCollection<T> collection, NotificationCallbackDelegate<T> callback)
            where T : RealmObjectBase
        {
            // Subscriptions can only work on the main thread.
            if (!ThreadSafety.IsUpdateThread)
                throw new InvalidOperationException("Cannot subscribe for realm notifications from a non-update thread.");

            return collection.SubscribeForNotifications(callback);
        }

        /// <summary>
        /// A convenience method that casts <see cref="IQueryable{T}"/> to <see cref="IRealmCollection{T}"/> and subscribes for change notifications.
        /// </summary>
        /// <remarks>
        /// This adds osu! specific thread and managed state safety checks on top of <see cref="IRealmCollection{T}.SubscribeForNotifications"/>.
        /// </remarks>
        /// <param name="list">The <see cref="IQueryable{T}"/> to observe for changes.</param>
        /// <typeparam name="T">Type of the elements in the list.</typeparam>
        /// <seealso cref="IRealmCollection{T}.SubscribeForNotifications"/>
        /// <param name="callback">The callback to be invoked with the updated <see cref="IRealmCollection{T}"/>.</param>
        /// <returns>
        /// A subscription token. It must be kept alive for as long as you want to receive change notifications.
        /// To stop receiving notifications, call <see cref="IDisposable.Dispose"/>.
        ///
        /// May be null in the case the provided collection is not managed.
        /// </returns>
        public static IDisposable? QueryAsyncWithNotifications<T>(this IQueryable<T> list, NotificationCallbackDelegate<T> callback)
            where T : RealmObjectBase
        {
            // Subscribing to non-managed instances doesn't work.
            // In this usage, the instance may be non-managed in tests.
            if (!(list is IRealmCollection<T> realmCollection))
                return null;

            return QueryAsyncWithNotifications(realmCollection, callback);
        }

        /// <summary>
        /// A convenience method that casts <see cref="IList{T}"/> to <see cref="IRealmCollection{T}"/> and subscribes for change notifications.
        /// </summary>
        /// <remarks>
        /// This adds osu! specific thread and managed state safety checks on top of <see cref="IRealmCollection{T}.SubscribeForNotifications"/>.
        /// </remarks>
        /// <param name="list">The <see cref="IList{T}"/> to observe for changes.</param>
        /// <typeparam name="T">Type of the elements in the list.</typeparam>
        /// <seealso cref="IRealmCollection{T}.SubscribeForNotifications"/>
        /// <param name="callback">The callback to be invoked with the updated <see cref="IRealmCollection{T}"/>.</param>
        /// <returns>
        /// A subscription token. It must be kept alive for as long as you want to receive change notifications.
        /// To stop receiving notifications, call <see cref="IDisposable.Dispose"/>.
        ///
        /// May be null in the case the provided collection is not managed.
        /// </returns>
        public static IDisposable? QueryAsyncWithNotifications<T>(this IList<T> list, NotificationCallbackDelegate<T> callback)
            where T : RealmObjectBase
        {
            // Subscribing to non-managed instances doesn't work.
            // In this usage, the instance may be non-managed in tests.
            if (!(list is IRealmCollection<T> realmCollection))
                return null;

            return QueryAsyncWithNotifications(realmCollection, callback);
        }
    }
}
