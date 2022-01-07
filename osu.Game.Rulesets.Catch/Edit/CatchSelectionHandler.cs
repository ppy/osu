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
using Direction = osu.Framework.Graphics.Direction;

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
            deltaX = limitMovement(deltaX, SelectedItems);

            if (deltaX == 0)
            {
                // Even if there is no positional change, there may be a time change.
                return true;
            }

            EditorBeatmap.PerformOnSelection(h =>
            {
                if (!(h is CatchHitObject catchObject)) return;

                catchObject.OriginalX += deltaX;

                // Move the nested hit objects to give an instant result before nested objects are recreated.
                foreach (var nested in catchObject.NestedHitObjects.OfType<CatchHitObject>())
                    nested.OriginalX += deltaX;
            });

            return true;
        }

        public override bool HandleFlip(Direction direction, bool flipOverOrigin)
        {
            if (SelectedItems.Count == 0)
                return false;

            // This could be implemented in the future if there's a requirement for it.
            if (direction == Direction.Vertical)
                return false;

            var selectionRange = CatchHitObjectUtils.GetPositionRange(SelectedItems);

            bool changed = false;

            EditorBeatmap.PerformOnSelection(h =>
            {
                if (h is CatchHitObject catchObject)
                    changed |= handleFlip(selectionRange, catchObject, flipOverOrigin);
            });

            return changed;
        }

        public override bool HandleReverse()
        {
            double selectionStartTime = SelectedItems.Min(h => h.StartTime);
            double selectionEndTime = SelectedItems.Max(h => h.GetEndTime());

            EditorBeatmap.PerformOnSelection(hitObject =>
            {
                hitObject.StartTime = selectionEndTime - (hitObject.GetEndTime() - selectionStartTime);

                if (hitObject is JuiceStream juiceStream)
                {
                    juiceStream.Path.Reverse(out Vector2 positionalOffset);
                    juiceStream.OriginalX += positionalOffset.X;
                    juiceStream.LegacyConvertedY += positionalOffset.Y;
                    EditorBeatmap.Update(juiceStream);
                }
            });
            return true;
        }

        protected override void OnSelectionChanged()
        {
            base.OnSelectionChanged();

            var selectionRange = CatchHitObjectUtils.GetPositionRange(SelectedItems);
            SelectionBox.CanFlipX = selectionRange.Length > 0 && SelectedItems.Any(h => h is CatchHitObject && !(h is BananaShower));
            SelectionBox.CanReverse = SelectedItems.Count > 1 || SelectedItems.Any(h => h is JuiceStream);
        }

        /// <summary>
        /// Limit positional movement of the objects by the constraint that moved objects should stay in bounds.
        /// </summary>
        /// <param name="deltaX">The positional movement.</param>
        /// <param name="movingObjects">The objects to be moved.</param>
        /// <returns>The positional movement with the restriction applied.</returns>
        private float limitMovement(float deltaX, IEnumerable<HitObject> movingObjects)
        {
            var range = CatchHitObjectUtils.GetPositionRange(movingObjects);
            // To make an object with position `x` stay in bounds after `deltaX` movement, `0 <= x + deltaX <= WIDTH` should be satisfied.
            // Subtracting `x`, we get `-x <= deltaX <= WIDTH - x`.
            // We only need to apply the inequality to extreme values of `x`.
            float lowerBound = -range.Min;
            float upperBound = CatchPlayfield.WIDTH - range.Max;
            // The inequality may be unsatisfiable if the objects were already out of bounds.
            // In that case, don't move objects at all.
            if (lowerBound > upperBound)
                return 0;

            return Math.Clamp(deltaX, lowerBound, upperBound);
        }

        private bool handleFlip(PositionRange selectionRange, CatchHitObject hitObject, bool flipOverOrigin)
        {
            switch (hitObject)
            {
                case BananaShower _:
                    return false;

                case JuiceStream juiceStream:
                    juiceStream.OriginalX = getFlippedPosition(juiceStream.OriginalX);

                    foreach (var point in juiceStream.Path.ControlPoints)
                        point.Position *= new Vector2(-1, 1);

                    EditorBeatmap.Update(juiceStream);
                    return true;

                default:
                    hitObject.OriginalX = getFlippedPosition(hitObject.OriginalX);
                    return true;
            }

            float getFlippedPosition(float original) => flipOverOrigin ? CatchPlayfield.WIDTH - original : selectionRange.GetFlippedPosition(original);
        }
    }
}
