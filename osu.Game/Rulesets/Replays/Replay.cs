// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Users;

namespace osu.Game.Rulesets.Replays
{
    public class Replay
    {
        public User User;
        public List<ReplayFrame> Frames = new List<ReplayFrame>();
    }
}
