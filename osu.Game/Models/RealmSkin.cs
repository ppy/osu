// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Testing;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.IO;
using osu.Game.Skinning;
using Realms;

#nullable enable

namespace osu.Game.Models
{
    [ExcludeFromDynamicCompile]
    [MapTo("Skin")]
    public class RealmSkin : RealmObject, IHasRealmFiles, IEquatable<RealmSkin>, IHasGuidPrimaryKey, ISoftDelete
    {
        public Guid ID { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Creator { get; set; } = string.Empty;

        public string Hash { get; set; } = string.Empty;

        public string InstantiationInfo { get; set; } = string.Empty;

        public virtual Skin CreateInstance(IStorageResourceProvider resources)
        {
            var type = string.IsNullOrEmpty(InstantiationInfo)
                // handle the case of skins imported before InstantiationInfo was added.
                ? typeof(LegacySkin)
                : Type.GetType(InstantiationInfo).AsNonNull();

            return (Skin)Activator.CreateInstance(type, this, resources);
        }

        public IList<RealmNamedFileUsage> Files { get; } = null!;

        public bool DeletePending { get; set; }

        public static RealmSkin Default { get; } = new RealmSkin
        {
            Name = "osu! (triangles)",
            Creator = "team osu!",
            InstantiationInfo = typeof(DefaultSkin).GetInvariantInstantiationInfo()
        };

        public bool Equals(RealmSkin? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;

            return ID == other.ID;
        }

        public override string ToString()
        {
            string author = string.IsNullOrEmpty(Creator) ? string.Empty : $"({Creator})";
            return $"{Name} {author}".Trim();
        }
    }
}
