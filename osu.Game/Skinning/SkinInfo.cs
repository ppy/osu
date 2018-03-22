// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using osu.Game.Database;

namespace osu.Game.Skinning
{
    public class SkinInfo : IHasFiles<SkinFileInfo>, IEquatable<SkinInfo>, IHasPrimaryKey, ISoftDelete
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public string Name { get; set; }

        public string Creator { get; set; }

        public List<SkinFileInfo> Files { get; set; }

        public bool DeletePending { get; set; }

        public static SkinInfo Default { get; } = new SkinInfo { Name = "osu!lazer", Creator = "team osu!" };

        public bool Equals(SkinInfo other) => other != null && ID == other.ID;

        public override string ToString() => $"\"{Name}\" by {Creator}";
    }
}
