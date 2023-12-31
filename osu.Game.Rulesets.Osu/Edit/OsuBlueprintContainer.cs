// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Input.Events;
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
        private OsuGridToolboxGroup gridToolbox = null!;

        public OsuBlueprintContainer(HitObjectComposer composer)
            : base(composer)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuGridToolboxGroup gridToolbox)
        {
            this.gridToolbox = gridToolbox;
            gridToolbox.GridFromPointsClicked += OnGridFromPointsClicked;
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

        private bool isPlacingGridFromPoints;
        private Vector2? gridFromPointsStart;

        private void OnGridFromPointsClicked()
        {
            isPlacingGridFromPoints = true;
            gridFromPointsStart = null;

            // Deselect all objects because we cant snap to objects which are selected.
            DeselectAll();
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            return isPlacingGridFromPoints || base.OnMouseDown(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (!isPlacingGridFromPoints)
                return base.OnClick(e);

            var pos = ToLocalSpace(Composer.FindSnappedPositionAndTime(e.ScreenSpaceMousePosition, ~SnapType.GlobalGrids).ScreenSpacePosition);

            if (!gridFromPointsStart.HasValue)
            {
                gridFromPointsStart = pos;
            }
            else
            {
                gridToolbox.SetGridFromPoints(gridFromPointsStart.Value, pos);
                isPlacingGridFromPoints = false;
                gridFromPointsStart = null;
            }

            return true;
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            if (!isPlacingGridFromPoints)
                return base.OnMouseMove(e);

            var pos = ToLocalSpace(Composer.FindSnappedPositionAndTime(e.ScreenSpaceMousePosition, ~SnapType.GlobalGrids).ScreenSpacePosition);

            if (!gridFromPointsStart.HasValue)
            {
                gridToolbox.StartPosition.Value = pos;
            }
            else
            {
                gridToolbox.SetGridFromPoints(gridFromPointsStart.Value, pos);
            }

            return true;
        }
    }
}
