//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Users;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// A beatmap set contains multiple beatmap (difficulties).
    /// </summary>
    public class BeatmapSet
    {
        public List<Beatmap> Beatmaps { get; protected set; }

        public string Artist;
        public string Title;

        public User Creator;
    }
}
