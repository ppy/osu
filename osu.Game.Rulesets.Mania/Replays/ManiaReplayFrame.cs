// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Mania.Replays
{
    public class ManiaReplayFrame : ReplayFrame
    {
        public override bool IsImportant => MouseX > 0;

        public ManiaReplayFrame(double time, int activeColumns)
            : base(time, activeColumns, null, ReplayButtonState.None)
        {
        }
    }
}
