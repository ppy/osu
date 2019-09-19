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
        public int ID { get; set; }

        public string Name { get; set; }

        public string Hash { get; set; }

        public string Creator { get; set; }

        public List<SkinFileInfo> Files { get; set; }

        public List<DatabasedSetting> Settings { get; set; }

        public bool DeletePending { get; set; }

        public string FullName => $"\"{Name}\" by {Creator}";

        public static SkinInfo Default { get; } = new SkinInfo
        {
            Name = "osu!lazer",
            Creator = "team osu!"
        };

        public bool Equals(SkinInfo other) => other != null && ID == other.ID;

        public override string ToString() => FullName;
    }
}
