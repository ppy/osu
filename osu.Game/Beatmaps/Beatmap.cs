//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Beatmaps.Objects;
using osu.Game.Beatmaps.Timing;
using osu.Game.Users;

namespace osu.Game.Beatmaps
{
    public class Beatmap
    {
        public int BeatmapID;
        
        public List<HitObject> HitObjects;
        public List<ControlPoint> ControlPoints;
        
        public string Version;
        public Metadata Metadata;
    }
}