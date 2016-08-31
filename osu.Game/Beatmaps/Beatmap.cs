//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Beatmaps.Objects;
using osu.Game.Users;

namespace osu.Game.Beatmaps
{
    public class Beatmap
    {
        public List<HitObject> HitObjects;

        public string Difficulty;
        public User Creator;
    }
}