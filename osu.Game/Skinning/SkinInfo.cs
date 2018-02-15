// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using osu.Game.Database;
using osu.Game.IO;

namespace osu.Game.Skinning
{
    public class SkinInfo : IHasFiles<SkinFileInfo>, IHasPrimaryKey, ISoftDelete
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public string Name { get; set; }

        public string Creator { get; set; }

        public List<SkinFileInfo> Files { get; set; }

        public bool DeletePending { get; set; }
    }
}
