// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using AutoMapper;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Input.Bindings;
using osu.Game.IO;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Skinning;
using Realms;

namespace osu.Game.Database
{
    public static class RealmExtensions
    {
        private static readonly IMapper mapper = new MapperConfiguration(c =>
        {
            c.ShouldMapField = fi => false;
            c.ShouldMapProperty = pi => pi.SetMethod != null && pi.SetMethod.IsPublic;

            c.CreateMap<BeatmapDifficulty, BeatmapDifficulty>();
            c.CreateMap<BeatmapInfo, BeatmapInfo>();
            c.CreateMap<BeatmapMetadata, BeatmapMetadata>();
            c.CreateMap<BeatmapSetFileInfo, BeatmapSetFileInfo>();

            c.CreateMap<BeatmapSetInfo, BeatmapSetInfo>()
             .ForMember(s => s.Beatmaps, d => d.MapFrom(s => s.Beatmaps))
             .ForMember(s => s.Files, d => d.MapFrom(s => s.Files))
             .MaxDepth(2);

            c.CreateMap<RealmKeyBinding, RealmKeyBinding>();
            c.CreateMap<DatabasedSetting, DatabasedSetting>();
            c.CreateMap<FileInfo, FileInfo>();
            c.CreateMap<ScoreFileInfo, ScoreFileInfo>();
            c.CreateMap<SkinInfo, SkinInfo>();
            c.CreateMap<RulesetInfo, RulesetInfo>();
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
        /// Create a detached copy of the each item in the list.
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

        /// <summary>
        /// Wrap a managed instance of a realm object in a <see cref="Live{T}"/>.
        /// </summary>
        /// <param name="item">The item to wrap.</param>
        /// <param name="contextFactory">A factory to retrieve realm contexts from.</param>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <returns>A wrapped instance of the provided item.</returns>
        public static Live<T> Wrap<T>(this T item, IRealmFactory contextFactory)
            where T : RealmObject, IHasGuidPrimaryKey => new Live<T>(item, contextFactory);

        /// <summary>
        /// Wrap an unmanaged instance of a realm object in a <see cref="Live{T}"/>.
        /// </summary>
        /// <param name="item">The item to wrap.</param>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <returns>A wrapped instance of the provided item.</returns>
        /// <exception cref="ArgumentException">Throws if the provided item is managed.</exception>
        public static Live<T> WrapAsUnmanaged<T>(this T item)
            where T : RealmObject, IHasGuidPrimaryKey
        {
            if (item.IsManaged)
                throw new ArgumentException("Provided item must not be managed", nameof(item));

            return new Live<T>(item, null);
        }
    }
}
