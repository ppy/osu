// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Replays.Legacy;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Replays.Types;

namespace osu.Game.Rulesets.Catch.Replays
{
    public class CatchReplayFrame : ReplayFrame, IConvertibleReplayFrame
    {
        public float Position;
        public bool Dashing;

        public CatchReplayFrame()
        {
        }

        public CatchReplayFrame(double time, float? position = null, bool dashing = false)
            : base(time)
        {
            Position = position ?? -1;
            Dashing = dashing;
        }

        public void ConvertFrom(LegacyReplayFrame legacyFrame, IBeatmap beatmap)
        {
            Position = legacyFrame.Position.X / CatchPlayfield.BASE_WIDTH;
            Dashing = legacyFrame.ButtonState == ReplayButtonState.Left1;
        }
    }
}
