// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Replays.Legacy;
using osu.Game.Rulesets.Replays.Types;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Catch.Replays
{
    public class CatchReplayFrame : ReplayFrame, IConvertibleReplayFrame
    {
        public float X;
        public bool Dashing;

        public CatchReplayFrame()
        {
        }

        public CatchReplayFrame(double time, float? x = null, bool dashing = false)
            : base(time)
        {
            X = x ?? -1;
            Dashing = dashing;
        }

        public void ConvertFrom(LegacyReplayFrame legacyFrame, Score score, Beatmap beatmap)
        {
            // Todo: This needs to be re-scaled
            X = legacyFrame.Position.X;
            Dashing = legacyFrame.ButtonState == ReplayButtonState.Left1;
        }
    }
}
