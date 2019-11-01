// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders
{
    public class SliderSelectionBlueprint : OsuSelectionBlueprint<Slider>
    {
        protected readonly SliderBodyPiece BodyPiece;
        protected readonly SliderCircleSelectionBlueprint HeadBlueprint;
        protected readonly SliderCircleSelectionBlueprint TailBlueprint;

        [Resolved(CanBeNull = true)]
        private HitObjectComposer composer { get; set; }

        public SliderSelectionBlueprint(DrawableSlider slider)
            : base(slider)
        {
            var sliderObject = (Slider)slider.HitObject;

            InternalChildren = new Drawable[]
            {
                BodyPiece = new SliderBodyPiece(),
                HeadBlueprint = CreateCircleSelectionBlueprint(slider, SliderPosition.Start),
                TailBlueprint = CreateCircleSelectionBlueprint(slider, SliderPosition.End),
                new PathControlPointVisualiser(sliderObject) { ControlPointsChanged = onNewControlPoints },
            };
        }

        protected override void Update()
        {
            base.Update();

            BodyPiece.UpdateFrom(HitObject);
        }

        private void onNewControlPoints(Vector2[] controlPoints)
        {
            var unsnappedPath = new SliderPath(controlPoints.Length > 2 ? PathType.Bezier : PathType.Linear, controlPoints);
            var snappedDistance = composer?.GetSnappedDistanceFromDistance(HitObject.StartTime, (float)unsnappedPath.Distance) ?? (float)unsnappedPath.Distance;

            HitObject.Path = new SliderPath(unsnappedPath.Type, controlPoints, snappedDistance);
        }

        public override Vector2 SelectionPoint => HeadBlueprint.SelectionPoint;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => BodyPiece.ReceivePositionalInputAt(screenSpacePos);

        protected virtual SliderCircleSelectionBlueprint CreateCircleSelectionBlueprint(DrawableSlider slider, SliderPosition position) => new SliderCircleSelectionBlueprint(slider, position);
    }
}
