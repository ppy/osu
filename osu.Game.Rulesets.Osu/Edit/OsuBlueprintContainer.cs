// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Spinners;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class OsuBlueprintContainer : ComposeBlueprintContainer
    {
        private Bindable<bool> limitedDistanceSnap { get; set; } = null!;

        public new OsuHitObjectComposer Composer => (OsuHitObjectComposer)base.Composer;

        public OsuBlueprintContainer(OsuHitObjectComposer composer)
            : base(composer)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            limitedDistanceSnap = config.GetBindable<bool>(OsuSetting.EditorLimitedDistanceSnap);
        }

        protected override SelectionHandler<HitObject> CreateSelectionHandler() => new OsuSelectionHandler();

        public override HitObjectSelectionBlueprint? CreateHitObjectBlueprintFor(HitObject hitObject)
        {
            switch (hitObject)
            {
                case HitCircle circle:
                    return new HitCircleSelectionBlueprint(circle);

                case Slider slider:
                    return new SliderSelectionBlueprint(slider);

                case Spinner spinner:
                    return new SpinnerSelectionBlueprint(spinner);
            }

            return base.CreateHitObjectBlueprintFor(hitObject);
        }

        protected override bool TryMoveBlueprints(DragEvent e, IList<(SelectionBlueprint<HitObject> blueprint, Vector2[] originalSnapPositions)> blueprints)
        {
            Vector2 distanceTravelled = e.ScreenSpaceMousePosition - e.ScreenSpaceMouseDownPosition;

            for (int i = 0; i < blueprints.Count; i++)
            {
                if (checkSnappingBlueprintToNearbyObjects(blueprints[i].blueprint, distanceTravelled, blueprints[i].originalSnapPositions))
                    return true;
            }

            // if no positional snapping could be performed, try unrestricted snapping from the earliest
            // item in the selection.

            // The final movement position, relative to movementBlueprintOriginalPosition.
            Vector2 movePosition = blueprints.First().originalSnapPositions.First() + distanceTravelled;
            var referenceBlueprint = blueprints.First().blueprint;

            // Retrieve a snapped position.
            var result = Composer.TrySnapToNearbyObjects(movePosition);
            result ??= Composer.TrySnapToDistanceGrid(movePosition, limitedDistanceSnap.Value ? referenceBlueprint.Item.StartTime : null);
            if (Composer.TrySnapToPositionGrid(result?.ScreenSpacePosition ?? movePosition, result?.Time) is SnapResult gridSnapResult)
                result = gridSnapResult;
            result ??= new SnapResult(movePosition, null);

            bool moved = SelectionHandler.HandleMovement(new MoveSelectionEvent<HitObject>(referenceBlueprint, result.ScreenSpacePosition - referenceBlueprint.ScreenSpaceSelectionPoint));
            if (moved)
                ApplySnapResultTime(result, referenceBlueprint.Item.StartTime);
            return moved;
        }

        /// <summary>
        /// Check for positional snap for given blueprint.
        /// </summary>
        /// <param name="blueprint">The blueprint to check for snapping.</param>
        /// <param name="distanceTravelled">Distance travelled since start of dragging action.</param>
        /// <param name="originalPositions">The snap positions of blueprint before start of dragging action.</param>
        /// <returns>Whether an object to snap to was found.</returns>
        private bool checkSnappingBlueprintToNearbyObjects(SelectionBlueprint<HitObject> blueprint, Vector2 distanceTravelled, Vector2[] originalPositions)
        {
            var currentPositions = blueprint.ScreenSpaceSnapPoints;

            for (int i = 0; i < originalPositions.Length; i++)
            {
                Vector2 originalPosition = originalPositions[i];
                var testPosition = originalPosition + distanceTravelled;

                var positionalResult = Composer.TrySnapToNearbyObjects(testPosition);

                if (positionalResult == null || positionalResult.ScreenSpacePosition == testPosition) continue;

                var delta = positionalResult.ScreenSpacePosition - currentPositions[i];

                // attempt to move the objects, and apply any time based snapping if we can.
                if (SelectionHandler.HandleMovement(new MoveSelectionEvent<HitObject>(blueprint, delta)))
                {
                    ApplySnapResultTime(positionalResult, blueprint.Item.StartTime);
                    return true;
                }
            }

            return false;
        }
    }
}
