// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Taiko.Replays;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Taiko.UI
{
    public partial class TaikoReplayRecorder : ReplayRecorder<TaikoAction>
    {
        public TaikoReplayRecorder(Score score)
            : base(score)
        {
        }

        protected override ReplayFrame HandleFrame(Vector2 mousePosition, List<TaikoAction> actions, ReplayFrame previousFrame) =>
            new TaikoReplayFrame(Time.Current, actions.ToArray());
    }
}
