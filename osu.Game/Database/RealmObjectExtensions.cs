// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using osu.Game.Input.Bindings;
using Realms;

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

        public static List<RealmLive<T>> ToLive<T>(this IEnumerable<T> realmList)
            where T : RealmObject, IHasGuidPrimaryKey
        {
            return realmList.Select(l => new RealmLive<T>(l)).ToList();
        }

        public static RealmLive<T> ToLive<T>(this T realmObject)
            where T : RealmObject, IHasGuidPrimaryKey
        {
            return new RealmLive<T>(realmObject);
        }
    }
}
