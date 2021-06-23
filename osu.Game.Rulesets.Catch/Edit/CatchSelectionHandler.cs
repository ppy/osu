// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Game.Rulesets.Catch.Objects;
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

            EditorBeatmap.PerformOnSelection(h =>
            {
                if (!(h is CatchHitObject hitObject)) return;

                if (hitObject is BananaShower) return;

                // TODO: confine in bounds
                hitObject.OriginalXBindable.Value += deltaX;

                // Move the nested hit objects to give an instant result before nested objects are recreated.
                foreach (var nested in hitObject.NestedHitObjects.OfType<CatchHitObject>())
                    nested.OriginalXBindable.Value += deltaX;
            });

            return true;
        }
    }
}
