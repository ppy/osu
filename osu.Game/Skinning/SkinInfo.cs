// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Configuration;
using osu.Game.Database;

namespace osu.Game.Skinning
{
    public class SkinInfo : IHasFiles<SkinFileInfo>, IEquatable<SkinInfo>, IHasPrimaryKey, ISoftDelete
    {
        internal const int DEFAULT_SKIN = 0;
        internal const int CLASSIC_SKIN = -1;
        internal const int RANDOM_SKIN = -2;

        public int ID { get; set; }

        public string Name { get; set; }

        public string Hash { get; set; }

        public string Creator { get; set; }

        public List<SkinFileInfo> Files { get; set; }

        public List<DatabasedSetting> Settings { get; set; }

        public bool DeletePending { get; set; }

        public static SkinInfo Default { get; } = new SkinInfo
        {
            ID = DEFAULT_SKIN,
            Name = "osu!lazer",
            Creator = "team osu!"
        };

        public bool Equals(SkinInfo other) => other != null && ID == other.ID;

        public override string ToString()
        {
            string author = Creator == null ? string.Empty : $"({Creator})";
            return $"{Name} {author}".Trim();
        }
    }
}
