// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Replays.Legacy;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Replays.Types;

namespace osu.Game.Rulesets.Catch.Replays
{
    public class CatchReplayFrame : ReplayFrame, IConvertibleReplayFrame
    {
        public List<CatchAction> Actions = new List<CatchAction>();

        public float Position;
        public bool Dashing;

        public CatchReplayFrame()
        {
        }

        public CatchReplayFrame(double time, float? position = null, bool dashing = false, CatchReplayFrame lastFrame = null)
            : base(time)
        {
            Position = position ?? -1;
            Dashing = dashing;

            if (Dashing)
                Actions.Add(CatchAction.Dash);

            if (lastFrame != null)
            {
                if (Position > lastFrame.Position)
                    Actions.Add(CatchAction.MoveRight);
                else if (Position < lastFrame.Position)
                    Actions.Add(CatchAction.MoveLeft);
            }
        }

        public void ConvertFrom(LegacyReplayFrame currentFrame, IBeatmap beatmap, LegacyReplayFrame lastFrame = null)
        {
            Position = currentFrame.Position.X / CatchPlayfield.BASE_WIDTH;
            Dashing = currentFrame.ButtonState == ReplayButtonState.Left1;

            if (Dashing)
                Actions.Add(CatchAction.Dash);

            if (lastFrame != null)
            {
                if (currentFrame.Position.X > lastFrame.Position.X)
                    Actions.Add(CatchAction.MoveRight);
                else if (currentFrame.Position.X < lastFrame.Position.X)
                    Actions.Add(CatchAction.MoveLeft);
            }
        }
    }
}
