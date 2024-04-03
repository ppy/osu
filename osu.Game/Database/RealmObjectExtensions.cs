// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using AutoMapper;
using AutoMapper.Internal;
using osu.Framework.Logging;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Input.Bindings;
using osu.Game.Models;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using Realms;

namespace osu.Game.Database
{
    public static class RealmObjectExtensions
    {
        private static readonly IMapper write_mapper = new MapperConfiguration(c =>
        {
            c.ShouldMapField = _ => false;
            c.ShouldMapProperty = pi => pi.SetMethod?.IsPublic == true;

            c.CreateMap<BeatmapMetadata, BeatmapMetadata>()
             .ForMember(s => s.Author, cc => cc.Ignore())
             .AfterMap((s, d) =>
             {
                 copyChangesToRealm(s.Author, d.Author);
             });
            c.CreateMap<BeatmapDifficulty, BeatmapDifficulty>();
            c.CreateMap<AudioNormalization, AudioNormalization>();
            c.CreateMap<RealmUser, RealmUser>();
            c.CreateMap<RealmFile, RealmFile>();
            c.CreateMap<RealmNamedFileUsage, RealmNamedFileUsage>();
            c.CreateMap<BeatmapInfo, BeatmapInfo>()
             .ForMember(s => s.Ruleset, cc => cc.Ignore())
             .ForMember(s => s.Metadata, cc => cc.Ignore())
             .ForMember(s => s.UserSettings, cc => cc.Ignore())
             .ForMember(s => s.Difficulty, cc => cc.Ignore())
             .ForMember(s => s.BeatmapSet, cc => cc.Ignore())
             .ForMember(s => s.AudioNormalization, cc => cc.Ignore())
             .AfterMap((s, d) =>
             {
                 d.Ruleset = d.Realm!.Find<RulesetInfo>(s.Ruleset.ShortName)!;
                 copyChangesToRealm(s.Difficulty, d.Difficulty);
                 copyChangesToRealm(s.Metadata, d.Metadata);
             });
            c.CreateMap<BeatmapSetInfo, BeatmapSetInfo>()
             .ConstructUsing(_ => new BeatmapSetInfo(null))
             .ForMember(s => s.Beatmaps, cc => cc.Ignore())
             .AfterMap((s, d) =>
             {
                 foreach (var beatmap in s.Beatmaps)
                 {
                     // Importantly, search all of realm for the beatmap (not just the set's beatmaps).
                     // It may have gotten detached, and if that's the case let's use this opportunity to fix
                     // things up.
                     var existingBeatmap = d.Realm!.Find<BeatmapInfo>(beatmap.ID);

                     if (existingBeatmap != null)
                     {
                         // As above, reattach if it happens to not be in the set's beatmaps.
                         if (!d.Beatmaps.Contains(existingBeatmap))
                         {
                             Debug.Fail("Beatmaps should never become detached under normal circumstances. If this ever triggers, it should be investigated further.");
                             Logger.Log("WARNING: One of the difficulties in a beatmap was detached from its set. Please save a copy of logs and report this to devs.", LoggingTarget.Database, LogLevel.Important);
                             d.Beatmaps.Add(existingBeatmap);
                         }

                         copyChangesToRealm(beatmap, existingBeatmap);
                     }
                     else
                     {
                         var newBeatmap = new BeatmapInfo
                         {
                             ID = beatmap.ID,
                             BeatmapSet = d,
                             Ruleset = d.Realm.Find<RulesetInfo>(beatmap.Ruleset.ShortName)!
                         };

                         d.Beatmaps.Add(newBeatmap);
                         copyChangesToRealm(beatmap, newBeatmap);
                     }
                 }
             });

            c.Internal().ForAllMaps((_, expression) =>
            {
                expression.ForAllMembers(m =>
                {
                    if (m.DestinationMember.Has<IgnoredAttribute>() || m.DestinationMember.Has<BacklinkAttribute>() || m.DestinationMember.Has<IgnoreDataMemberAttribute>())
                        m.Ignore();
                });
            });
        }).CreateMapper();

        private static readonly IMapper mapper = new MapperConfiguration(c =>
        {
            applyCommonConfiguration(c);

            c.CreateMap<BeatmapSetInfo, BeatmapSetInfo>()
             .ConstructUsing(_ => new BeatmapSetInfo(null))
             .MaxDepth(2)
             .AfterMap((_, d) =>
             {
                 foreach (var beatmap in d.Beatmaps)
                     beatmap.BeatmapSet = d;
             });

            // This can be further optimised to reduce cyclic retrievals, similar to the optimised set mapper below.
            // Only hasn't been done yet as we detach at the point of BeatmapInfo less often.
            c.CreateMap<BeatmapInfo, BeatmapInfo>()
             .MaxDepth(2)
             .AfterMap((_, d) =>
             {
                 for (int i = 0; i < d.BeatmapSet?.Beatmaps.Count; i++)
                 {
                     if (d.BeatmapSet.Beatmaps[i].Equals(d))
                     {
                         d.BeatmapSet.Beatmaps[i] = d;
                         break;
                     }
                 }
             });
        }).CreateMapper();

        /// <summary>
        /// A slightly optimised mapper that avoids double-fetches in cyclic reference.
        /// </summary>
        private static readonly IMapper beatmap_set_mapper = new MapperConfiguration(c =>
        {
            applyCommonConfiguration(c);

            c.CreateMap<BeatmapSetInfo, BeatmapSetInfo>()
             .ConstructUsing(_ => new BeatmapSetInfo(null))
             .MaxDepth(2)
             .ForMember(b => b.Files, cc => cc.Ignore())
             .AfterMap((_, d) =>
             {
                 foreach (var beatmap in d.Beatmaps)
                     beatmap.BeatmapSet = d;
             });

            c.CreateMap<BeatmapInfo, BeatmapInfo>()
             .MaxDepth(1)
             // This is not required as it will be populated in the `AfterMap` call from the `BeatmapInfo`'s parent.
             .ForMember(b => b.BeatmapSet, cc => cc.Ignore());
        }).CreateMapper();

        private static void applyCommonConfiguration(IMapperConfigurationExpression c)
        {
            c.ShouldMapField = _ => false;

            // This is specifically to avoid mapping explicit interface implementations.
            // If we want to limit this further, we can avoid mapping properties with no setter that are not IList<>.
            // Takes a bit of effort to determine whether this is the case though, see https://stackoverflow.com/questions/951536/how-do-i-tell-whether-a-type-implements-ilist
            c.ShouldMapProperty = pi => pi.GetMethod?.IsPublic == true;

            c.Internal().ForAllMaps((_, expression) =>
            {
                expression.ForAllMembers(m =>
                {
                    if (m.DestinationMember.Has<IgnoredAttribute>() || m.DestinationMember.Has<BacklinkAttribute>() || m.DestinationMember.Has<IgnoreDataMemberAttribute>())
                        m.Ignore();
                });
            });

            c.CreateMap<RealmKeyBinding, RealmKeyBinding>();
            c.CreateMap<BeatmapMetadata, BeatmapMetadata>();
            c.CreateMap<BeatmapUserSettings, BeatmapUserSettings>();
            c.CreateMap<BeatmapDifficulty, BeatmapDifficulty>();
            c.CreateMap<RulesetInfo, RulesetInfo>();
            c.CreateMap<AudioNormalization, AudioNormalization>();
            c.CreateMap<ScoreInfo, ScoreInfo>();
            c.CreateMap<RealmUser, RealmUser>();
            c.CreateMap<RealmFile, RealmFile>();
            c.CreateMap<RealmNamedFileUsage, RealmNamedFileUsage>();
        }

        /// <summary>
        /// Create a detached copy of the each item in the collection.
        /// </summary>
        /// <remarks>
        /// Items which are already detached (ie. not managed by realm) will not be modified.
        /// </remarks>
        /// <param name="items">A list of managed <see cref="RealmObject"/>s to detach.</param>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <returns>A list containing non-managed copies of provided items.</returns>
        public static List<T> Detach<T>(this IEnumerable<T> items) where T : RealmObjectBase
        {
            var list = new List<T>();

            foreach (var obj in items)
                list.Add(obj.Detach());

            return list;
        }

        /// <summary>
        /// Create a detached copy of the item.
        /// </summary>
        /// <remarks>
        /// If the item if already detached (ie. not managed by realm) it will not be detached again and the original instance will be returned. This allows this method to be potentially called at multiple levels while only incurring the clone overhead once.
        /// </remarks>
        /// <param name="item">The managed <see cref="RealmObject"/> to detach.</param>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <returns>A non-managed copy of provided item. Will return the provided item if already detached.</returns>
        public static T Detach<T>(this T item) where T : RealmObjectBase
        {
            if (!item.IsManaged)
                return item;

            if (item is BeatmapSetInfo)
                return beatmap_set_mapper.Map<T>(item);

            return mapper.Map<T>(item);
        }

        /// <summary>
        /// Copy changes in a detached beatmap back to realm.
        /// This is a temporary method to handle existing flows only. It should not be used going forward if we can avoid it.
        /// </summary>
        /// <param name="source">The detached beatmap to copy from.</param>
        /// <param name="destination">The live beatmap to copy to.</param>
        public static void CopyChangesToRealm(this BeatmapSetInfo source, BeatmapSetInfo destination)
            => copyChangesToRealm(source, destination);

        private static void copyChangesToRealm<T>(T source, T destination) where T : RealmObjectBase
            => write_mapper.Map(source, destination);

        public static List<Live<T>> ToLiveUnmanaged<T>(this IEnumerable<T> realmList)
            where T : RealmObject, IHasGuidPrimaryKey
        {
            return realmList.Select(l => new RealmLiveUnmanaged<T>(l)).Cast<Live<T>>().ToList();
        }

        public static Live<T> ToLiveUnmanaged<T>(this T realmObject)
            where T : RealmObject, IHasGuidPrimaryKey
        {
            return new RealmLiveUnmanaged<T>(realmObject);
        }

        public static Live<T> ToLive<T>(this T realmObject, RealmAccess realm)
            where T : RealmObject, IHasGuidPrimaryKey
        {
            return new RealmLive<T>(realmObject, realm);
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
        /// </returns>
        /// <seealso cref="M:Realms.CollectionExtensions.SubscribeForNotifications``1(System.Collections.Generic.IList{``0},Realms.NotificationCallbackDelegate{``0})" />
        /// <seealso cref="M:Realms.CollectionExtensions.SubscribeForNotifications``1(System.Linq.IQueryable{``0},Realms.NotificationCallbackDelegate{``0})" />
        public static IDisposable QueryAsyncWithNotifications<T>(this IRealmCollection<T> collection, NotificationCallbackDelegate<T> callback)
            where T : RealmObjectBase
        {
            if (!RealmAccess.CurrentThreadSubscriptionsAllowed)
                throw new InvalidOperationException($"Make sure to call {nameof(RealmAccess)}.{nameof(RealmAccess.RegisterForNotifications)}");

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
