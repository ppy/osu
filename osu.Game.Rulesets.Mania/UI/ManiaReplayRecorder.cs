// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Replays;
using osu.Game.Rulesets.Mania.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Mania.UI
{
    public class ManiaReplayRecorder : ReplayRecorder<ManiaAction>
    {
        public ManiaReplayRecorder(Replay replay)
            : base(replay)
        {
        }

        protected override ReplayFrame HandleFrame(Vector2 mousePosition, List<ManiaAction> actions, ReplayFrame previousFrame)
            => new ManiaReplayFrame(Time.Current, actions.ToArray());
    }
}
