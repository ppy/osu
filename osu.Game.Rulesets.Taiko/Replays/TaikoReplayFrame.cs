// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Taiko.Replays
{
    public class TaikoReplayFrame : ReplayFrame
    {
        public override bool IsImportant => MouseLeft || MouseRight;

        public TaikoReplayFrame(double time, ReplayButtonState buttons)
            : base(time, null, null, buttons)
        {
        }
    }
}
