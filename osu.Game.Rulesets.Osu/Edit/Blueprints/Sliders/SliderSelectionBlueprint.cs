// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Threading;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders
{
    public class SliderSelectionBlueprint : OsuSelectionBlueprint
    {
        protected SliderCircleSelectionBlueprint HeadBlueprint { get; private set; }

        public SliderSelectionBlueprint(DrawableSlider slider)
            : base(slider)
        {
            Container sliderCircleContainer;
            var sliderObject = (Slider)slider.HitObject;

            InternalChildren = new Drawable[]
            {
                new SliderBodyPiece(sliderObject),
                sliderCircleContainer = new Container
                {
                    Children = new[]
                    {
                        HeadBlueprint = new SliderCircleSelectionBlueprint(slider.HeadCircle, sliderObject, SliderPosition.Start),
                        new SliderCircleSelectionBlueprint(slider.TailCircle, sliderObject, SliderPosition.End),
                    }
                },
                new PathControlPointVisualiser(sliderObject),
            };

            ScheduledDelegate regenerationDebounce = null;

            sliderObject.OnRegenerated += () =>
            {
                regenerationDebounce?.Cancel();
                regenerationDebounce = Schedule(() =>
                {
                    sliderCircleContainer.Clear();
                    sliderCircleContainer.AddRange(new[]
                    {
                        HeadBlueprint = new SliderCircleSelectionBlueprint(slider.HeadCircle, sliderObject, SliderPosition.Start),
                        new SliderCircleSelectionBlueprint(slider.TailCircle, sliderObject, SliderPosition.End),
                    });
                });
            };
        }

        public override Vector2 SelectionPoint => HeadBlueprint.SelectionPoint;
    }
}
