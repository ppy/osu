// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public class OsuSelectionHandler : SelectionHandler
    {
        public override bool HandleMovement(MoveSelectionEvent moveEvent)
        {
            Vector2 minPosition = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 maxPosition = new Vector2(float.MinValue, float.MinValue);

            // Go through all hitobjects to make sure they would remain in the bounds of the editor after movement, before any movement is attempted
            foreach (var h in SelectedHitObjects.OfType<OsuHitObject>())
            {
                if (h is Spinner)
                {
                    // Spinners don't support position adjustments
                    continue;
                }

                // Stacking is not considered
                minPosition = Vector2.ComponentMin(minPosition, Vector2.ComponentMin(h.EndPosition + moveEvent.InstantDelta, h.Position + moveEvent.InstantDelta));
                maxPosition = Vector2.ComponentMax(maxPosition, Vector2.ComponentMax(h.EndPosition + moveEvent.InstantDelta, h.Position + moveEvent.InstantDelta));
            }

            if (minPosition.X < 0 || minPosition.Y < 0 || maxPosition.X > DrawWidth || maxPosition.Y > DrawHeight)
                return false;

            foreach (var h in SelectedHitObjects.OfType<OsuHitObject>())
            {
                if (h is Spinner)
                {
                    // Spinners don't support position adjustments
                    continue;
                }

                h.Position += moveEvent.InstantDelta;
            }

            return true;
        }
    }
}
