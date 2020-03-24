// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Replays;
using osu.Game.Rulesets.Catch.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CatchReplayRecorder : ReplayRecorder<CatchAction>
    {
        public CatchReplayRecorder(Replay target)
            : base(target)
        {
        }

        protected override ReplayFrame HandleFrame(Vector2 position, List<CatchAction> actions, ReplayFrame previousFrame)
            => new CatchReplayFrame(Time.Current, position.X, actions.Contains(CatchAction.Dash), previousFrame as CatchReplayFrame);
    }
}
