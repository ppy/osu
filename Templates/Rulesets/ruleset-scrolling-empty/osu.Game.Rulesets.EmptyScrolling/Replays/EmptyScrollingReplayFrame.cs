// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.EmptyScrolling.Replays
{
    public class EmptyScrollingReplayFrame : ReplayFrame
    {
        public List<EmptyScrollingAction> Actions = new List<EmptyScrollingAction>();

        public EmptyScrollingReplayFrame(EmptyScrollingAction? button = null)
        {
            if (button.HasValue)
                Actions.Add(button.Value);
        }
    }
}
