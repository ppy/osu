// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Replays.Legacy;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Replays.Types;
using osuTK;

namespace osu.Game.Rulesets.Osu.Replays
{
    public class OsuReplayFrame : ReplayFrame, IConvertibleReplayFrame
    {
        public Vector2 Position;
        public List<OsuAction> Actions = new List<OsuAction>();

        public OsuReplayFrame()
        {
        }

        public OsuReplayFrame(double time, Vector2 position, params OsuAction[] actions)
            : base(time)
        {
            Position = position;
            Actions.AddRange(actions);
        }

        public void FromLegacy(LegacyReplayFrame currentFrame, IBeatmap beatmap, ReplayFrame lastFrame = null)
        {
            Position = currentFrame.Position;
            if (currentFrame.MouseLeft) Actions.Add(OsuAction.LeftButton);
            if (currentFrame.MouseRight) Actions.Add(OsuAction.RightButton);
        }

        public LegacyReplayFrame ToLegacy(IBeatmap beatmap)
        {
            ReplayButtonState state = ReplayButtonState.None;

            if (Actions.Contains(OsuAction.LeftButton))
                state |= ReplayButtonState.Left1;
            if (Actions.Contains(OsuAction.RightButton))
                state |= ReplayButtonState.Right1;

            return new LegacyReplayFrame(Time, Position.X, Position.Y, state);
        }
    }
}
