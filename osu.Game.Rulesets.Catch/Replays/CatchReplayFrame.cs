// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Catch.Replays
{
    public class CatchReplayFrame : ReplayFrame
    {
        public override bool IsImportant => MouseX > 0;

        public CatchReplayFrame(double time, float? x = null)
            : base(time, x ?? -1, null, ReplayButtonState.None)
        {
        }
    }
}
