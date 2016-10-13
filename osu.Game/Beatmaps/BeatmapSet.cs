//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Users;
using SQLite;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// A beatmap set contains multiple beatmap (difficulties).
    /// </summary>
    public class BeatmapSet
    {
        [PrimaryKey]
        public int BeatmapSetID { get; set; }
        [NotNull, Indexed]
        public int BeatmapMetadataID { get; set; }
        [Ignore]
        public List<Beatmap> Beatmaps { get; protected set; } = new List<Beatmap>();
        [Ignore]
        public BeatmapMetadata Metadata { get; set; }
        [Ignore]
        public User Creator { get; set; }
        public string Hash { get; set; }
        public string Path { get; set; }
    }
}
