// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.IO;
using osu.Game.Modes.Taiko.Replays;

namespace osu.Game.Modes.Taiko.Scoring
{
    public class TaikoScore : Score
    {
        public override Replay CreateLegacyReplayFrom(StreamReader reader) => new LegacyTaikoReplay(reader);
    }
}
