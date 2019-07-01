// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Rulesets.Osu.Edit
{
    public class OsuSelectionHandler : SelectionHandler
    {
        public override void HandleDrag(SelectionBlueprint blueprint, DragEvent dragEvent)
        {
            foreach (var h in SelectedHitObjects.OfType<OsuHitObject>())
            {
                if (h is Spinner)
                {
                    // Spinners don't support position adjustments
                    continue;
                }

                h.Position += dragEvent.Delta;
            }

            base.HandleDrag(blueprint, dragEvent);
        }
    }
}
