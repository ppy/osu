// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.IO;

namespace osu.Game.Skinning
{
    [Table(@"SkinInfo")]
    public class EFSkinInfo : IHasFiles<SkinFileInfo>, IEquatable<EFSkinInfo>, IHasPrimaryKey, ISoftDelete
    {
        internal const int DEFAULT_SKIN = 0;
        internal const int CLASSIC_SKIN = -1;
        internal const int RANDOM_SKIN = -2;

        public int ID { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Creator { get; set; } = string.Empty;

        public string Hash { get; set; }

        public string InstantiationInfo { get; set; }

        public virtual Skin CreateInstance(IStorageResourceProvider resources)
        {
            var type = string.IsNullOrEmpty(InstantiationInfo)
                // handle the case of skins imported before InstantiationInfo was added.
                ? typeof(LegacySkin)
                : Type.GetType(InstantiationInfo).AsNonNull();

            return (Skin)Activator.CreateInstance(type, this, resources);
        }

        public List<SkinFileInfo> Files { get; set; } = new List<SkinFileInfo>();

        public bool DeletePending { get; set; }

        public static EFSkinInfo Default { get; } = new EFSkinInfo
        {
            ID = DEFAULT_SKIN,
            Name = "osu! (triangles)",
            Creator = "team osu!",
            InstantiationInfo = typeof(DefaultSkin).GetInvariantInstantiationInfo()
        };

        public bool Equals(EFSkinInfo other) => other != null && ID == other.ID;

        public override string ToString()
        {
            string author = Creator == null ? string.Empty : $"({Creator})";
            return $"{Name} {author}".Trim();
        }

        public bool IsManaged => ID > 0;
    }
}
