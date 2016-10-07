//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Beatmaps.Objects;
using osu.Game.Beatmaps.Timing;
using osu.Game.GameModes.Play;
using osu.Game.Users;
using SQLite;

namespace osu.Game.Beatmaps
{
    public class Beatmap
    {
        [PrimaryKey]
        public int BeatmapID { get; set; }
        [NotNull, Indexed]
        public int BeatmapSetID { get; set; }
        [Indexed]
        public int BeatmapMetadataID { get; set; }
        public int BaseDifficultyID { get; set; }
        [Ignore]
        public List<HitObject> HitObjects { get; set; }
        [Ignore]
        public List<ControlPoint> ControlPoints { get; set; }
        [Ignore]
        public BeatmapMetadata Metadata { get; set; }
        [Ignore]
        public BaseDifficulty BaseDifficulty { get; set; }
        public PlayMode Mode { get; set; }
        public string Version { get; set; }
    }
}