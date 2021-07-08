// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Catch.Edit
{
    public class CatchSelectionHandler : EditorSelectionHandler
    {
        protected ScrollingHitObjectContainer HitObjectContainer => (ScrollingHitObjectContainer)playfield.HitObjectContainer;

        [Resolved]
        private Playfield playfield { get; set; }

        public override bool HandleMovement(MoveSelectionEvent<HitObject> moveEvent)
        {
            var blueprint = moveEvent.Blueprint;
            Vector2 originalPosition = HitObjectContainer.ToLocalSpace(blueprint.ScreenSpaceSelectionPoint);
            Vector2 targetPosition = HitObjectContainer.ToLocalSpace(blueprint.ScreenSpaceSelectionPoint + moveEvent.ScreenSpaceDelta);
            float deltaX = targetPosition.X - originalPosition.X;

            foreach (float x in EditorBeatmap.SelectedHitObjects.SelectMany(getOriginalPositions))
                deltaX = Math.Clamp(deltaX, -x, CatchPlayfield.WIDTH - x);

            if (deltaX == 0)
            {
                // Returns true: even there is no positional change, there may be a time change.
                return true;
            }

            EditorBeatmap.PerformOnSelection(h =>
            {
                if (!(h is CatchHitObject hitObject)) return;

                hitObject.OriginalX += deltaX;

                // Move the nested hit objects to give an instant result before nested objects are recreated.
                foreach (var nested in hitObject.NestedHitObjects.OfType<CatchHitObject>())
                    nested.OriginalX += deltaX;
            });

            return true;
        }

        /// <summary>
        /// Enumerate X positions that should be contained in-bounds after move offset is applied.
        /// </summary>
        private IEnumerable<float> getOriginalPositions(HitObject hitObject)
        {
            switch (hitObject)
            {
                case Fruit fruit:
                    yield return fruit.OriginalX;

                    break;

                case JuiceStream juiceStream:
                    foreach (var nested in juiceStream.NestedHitObjects.OfType<CatchHitObject>())
                    {
                        // Exclude tiny droplets: even if `OriginalX` is outside the playfield, it can be moved inside the playfield after the random offset application.
                        if (!(nested is TinyDroplet))
                            yield return nested.OriginalX;
                    }

                    break;

                case BananaShower _:
                    // A banana shower occupies the whole screen width.
                    // If the selection contains a banana shower, the selection cannot be moved horizontally.
                    yield return 0;
                    yield return CatchPlayfield.WIDTH;

                    break;
            }
        }
    }
}
