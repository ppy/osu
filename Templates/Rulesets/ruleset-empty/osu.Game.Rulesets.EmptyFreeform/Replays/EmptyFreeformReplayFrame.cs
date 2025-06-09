// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Replays;
using osuTK;

namespace osu.Game.Rulesets.EmptyFreeform.Replays
{
    public class EmptyFreeformReplayFrame : ReplayFrame
    {
        public List<EmptyFreeformAction> Actions = new List<EmptyFreeformAction>();
        public Vector2 Position;

        public EmptyFreeformReplayFrame(EmptyFreeformAction? button = null)
        {
            if (button.HasValue)
                Actions.Add(button.Value);
        }

        public override bool IsEquivalentTo(ReplayFrame other)
            => other is EmptyFreeformReplayFrame freeformFrame && Time == freeformFrame.Time && Position == freeformFrame.Position && Actions.SequenceEqual(freeformFrame.Actions);
    }
}
