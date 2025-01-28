// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using osu.Game.Database;
using osu.Game.IO;
using osu.Game.Models;
using Realms;

namespace osu.Game.Skinning
{
    [MapTo("Skin")]
    [JsonObject(MemberSerialization.OptIn)]
    public class SkinInfo : RealmObject, IHasRealmFiles, IEquatable<SkinInfo>, IHasGuidPrimaryKey, ISoftDelete
    {
        internal static readonly Guid TRIANGLES_SKIN = new Guid("2991CFD8-2140-469A-BCB9-2EC23FBCE4AD");
        internal static readonly Guid ARGON_SKIN = new Guid("CFFA69DE-B3E3-4DEE-8563-3C4F425C05D0");
        internal static readonly Guid ARGON_PRO_SKIN = new Guid("9FC9CF5D-0F16-4C71-8256-98868321AC43");
        internal static readonly Guid CLASSIC_SKIN = new Guid("81F02CD3-EEC6-4865-AC23-FAE26A386187");
        internal static readonly Guid RANDOM_SKIN = new Guid("D39DFEFB-477C-4372-B1EA-2BCEA5FB8908");

        [PrimaryKey]
        [JsonProperty]
        public Guid ID { get; set; }

        [JsonProperty]
        public string Name { get; set; } = null!;

        [JsonProperty]
        public string Creator { get; set; } = null!;

        [JsonProperty]
        public string InstantiationInfo { get; set; } = null!;

        public string Hash { get; set; } = string.Empty;

        public bool Protected { get; set; }

        public virtual Skin CreateInstance(IStorageResourceProvider resources)
        {
            var type = string.IsNullOrEmpty(InstantiationInfo)
                // handle the case of skins imported before InstantiationInfo was added.
                ? typeof(LegacySkin)
                : Type.GetType(InstantiationInfo);

            if (type == null)
            {
                // Since the class was renamed from "DefaultSkin" to "TrianglesSkin", the type retrieval would fail
                // for user modified skins. This aims to amicably handle that.
                // If we ever add more default skins in the future this will need some kind of proper migration rather than
                // a single fallback.
                return new TrianglesSkin(this, resources);
            }

            return (Skin)Activator.CreateInstance(type, this, resources)!;
        }

        public IList<RealmNamedFileUsage> Files { get; } = null!;

        public bool DeletePending { get; set; }

        public SkinInfo(string? name = null, string? creator = null, string? instantiationInfo = null)
        {
            Name = name ?? string.Empty;
            Creator = creator ?? string.Empty;
            InstantiationInfo = instantiationInfo ?? string.Empty;
            ID = Guid.NewGuid();
        }

        [UsedImplicitly] // Realm
        private SkinInfo()
        {
        }

        public bool Equals(SkinInfo? other)
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

        IEnumerable<INamedFileUsage> IHasNamedFiles.Files => Files;
    }
}
