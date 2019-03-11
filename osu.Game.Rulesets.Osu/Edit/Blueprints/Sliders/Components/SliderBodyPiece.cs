// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components
{
    public class SliderBodyPiece : SliderPiece
    {
        private readonly Slider slider;
        private readonly ManualSliderBody body;

        public SliderBodyPiece(Slider slider)
            : base(slider)
        {
            this.slider = slider;

            InternalChild = body = new ManualSliderBody
            {
                AccentColour = Color4.Transparent,
                PathRadius = slider.Scale * 64
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            body.BorderColour = colours.Yellow;

            PositionBindable.BindValueChanged(_ => updatePosition(), true);
            ScaleBindable.BindValueChanged(scale => body.PathRadius = scale.NewValue * 64, true);
        }

        private void updatePosition() => Position = slider.StackedPosition;

        protected override void Update()
        {
            base.Update();

            var vertices = new List<Vector2>();
            slider.Path.GetPathToProgress(vertices, 0, 1);

            body.SetVertices(vertices);

            Size = body.Size;
            OriginPosition = body.PathOffset;
        }
    }
}
