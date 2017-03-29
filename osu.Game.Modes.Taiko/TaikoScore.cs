// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Scoring;
using System.IO;

namespace osu.Game.Modes.Taiko
{
    public class TaikoScore : Score
    {
        public override Replay CreateLegacyReplayFrom(StreamReader reader) => new LegacyTaikoReplay(reader);
    }
}
