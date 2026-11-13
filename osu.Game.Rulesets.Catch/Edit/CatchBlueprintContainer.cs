// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Catch.Edit.Blueprints;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Catch.Edit
{
    public partial class CatchBlueprintContainer : ComposeBlueprintContainer
    {
        public new CatchHitObjectComposer Composer => (CatchHitObjectComposer)base.Composer;

        public CatchBlueprintContainer(CatchHitObjectComposer composer)
            : base(composer)
        {
        }

        protected override SelectionHandler<HitObject> CreateSelectionHandler() => new CatchSelectionHandler();

        public override HitObjectSelectionBlueprint? CreateHitObjectBlueprintFor(HitObject hitObject)
        {
            switch (hitObject)
            {
                case Fruit fruit:
                    return new FruitSelectionBlueprint(fruit);

                case JuiceStream juiceStream:
                    return new JuiceStreamSelectionBlueprint(juiceStream);

                case BananaShower bananaShower:
                    return new BananaShowerSelectionBlueprint(bananaShower);
            }

            return base.CreateHitObjectBlueprintFor(hitObject);
        }

        protected sealed override DragBox CreateDragBox() => new ScrollingDragBox(Composer.Playfield);

        protected override bool TryMoveBlueprints(DragEvent e, IList<(SelectionBlueprint<HitObject> blueprint, Vector2[] originalSnapPositions)> blueprints)
        {
            Vector2 distanceTravelled = e.ScreenSpaceMousePosition - e.ScreenSpaceMouseDownPosition;

            // The final movement position, relative to movementBlueprintOriginalPosition.
            Vector2 movePosition = blueprints.First().originalSnapPositions.First() + distanceTravelled;

            // Retrieve a snapped position.
            var gridSnapResult = Composer.FindSnappedPositionAndTime(movePosition);
            gridSnapResult.ScreenSpacePosition.X = movePosition.X;
            var distanceSnapResult = Composer.TryDistanceSnap(gridSnapResult.ScreenSpacePosition);

            var result = distanceSnapResult != null && Vector2.Distance(gridSnapResult.ScreenSpacePosition, distanceSnapResult.ScreenSpacePosition) < CatchHitObjectComposer.DISTANCE_SNAP_RADIUS
                ? distanceSnapResult
                : gridSnapResult;

            var referenceBlueprint = blueprints.First().blueprint;
            bool moved = SelectionHandler.HandleMovement(new MoveSelectionEvent<HitObject>(referenceBlueprint, result.ScreenSpacePosition - referenceBlueprint.ScreenSpaceSelectionPoint));
            if (moved)
                ApplySnapResultTime(result, referenceBlueprint.Item.StartTime);
            return moved;
        }
    }
}
