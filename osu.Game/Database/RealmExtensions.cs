// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

            c.CreateMap<DatabasedKeyBinding, DatabasedKeyBinding>();
            c.CreateMap<RealmKeyBinding, RealmKeyBinding>();
            c.CreateMap<DatabasedSetting, DatabasedSetting>();
            c.CreateMap<FileInfo, FileInfo>();
            c.CreateMap<ScoreFileInfo, ScoreFileInfo>();
            c.CreateMap<SkinInfo, SkinInfo>();
            c.CreateMap<RulesetInfo, RulesetInfo>();
        }).CreateMapper();

        public static List<T> Detach<T>(this List<T> items) where T : RealmObject
        {
            var list = new List<T>();

            foreach (var obj in items)
                list.Add(obj.Detach());

            return list;
        }

        public static T Detach<T>(this T obj) where T : RealmObject
        {
            if (!obj.IsManaged)
                return obj;

            var detached = mapper.Map<T>(obj);

            //typeof(RealmObject).GetField("_realm", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(detached, null);

            return detached;
        }

        public static Live<T> Wrap<T>(this T obj, IRealmFactory contextFactory)
            where T : RealmObject, IHasGuidPrimaryKey => new Live<T>(obj, contextFactory);

        public static Live<T> WrapAsUnmanaged<T>(this T obj)
            where T : RealmObject, IHasGuidPrimaryKey => new Live<T>(obj, null);
    }
}
