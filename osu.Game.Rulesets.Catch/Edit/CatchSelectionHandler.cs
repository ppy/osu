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
            deltaX = limitMovement(deltaX, EditorBeatmap.SelectedHitObjects);

            if (deltaX == 0)
            {
                // Even if there is no positional change, there may be a time change.
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
        /// Limit positional movement of the objects by the constraint that moved objects should stay in bounds.
        /// </summary>
        /// <param name="deltaX">The positional movement.</param>
        /// <param name="movingObjects">The objects to be moved.</param>
        /// <returns>The positional movement with the restriction applied.</returns>
        private float limitMovement(float deltaX, IEnumerable<HitObject> movingObjects)
        {
            float minX = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;

            foreach (float x in movingObjects.SelectMany(getOriginalPositions))
            {
                minX = Math.Min(minX, x);
                maxX = Math.Max(maxX, x);
            }

            // To make an object with position `x` stay in bounds after `deltaX` movement, `0 <= x + deltaX <= WIDTH` should be satisfied.
            // Subtracting `x`, we get `-x <= deltaX <= WIDTH - x`.
            // We only need to apply the inequality to extreme values of `x`.
            float lowerBound = -minX;
            float upperBound = CatchPlayfield.WIDTH - maxX;
            // The inequality may be unsatisfiable if the objects were already out of bounds.
            // In that case, don't move objects at all.
            if (lowerBound > upperBound)
                return 0;

            return Math.Clamp(deltaX, lowerBound, upperBound);
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
                        // Even if `OriginalX` is outside the playfield, tiny droplets can be moved inside the playfield after the random offset application.
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
