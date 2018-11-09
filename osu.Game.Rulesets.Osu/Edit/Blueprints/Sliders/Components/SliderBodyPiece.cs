// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components
{
    public class SliderBodyPiece : CompositeDrawable
    {
        private readonly IBindable<Vector2> positionBindable = new Bindable<Vector2>();
        private readonly IBindable<float> scaleBindable = new Bindable<float>();

        private readonly Slider slider;
        private readonly ManualSliderBody body;

        public SliderBodyPiece(Slider slider)
        {
            this.slider = slider;

            InternalChild = body = new ManualSliderBody
            {
                AccentColour = Color4.Transparent,
                PathWidth = slider.Scale * 64
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            body.BorderColour = colours.Yellow;

            positionBindable.BindValueChanged(_ => updatePosition());
            scaleBindable.BindValueChanged(v => body.PathWidth = v * 64);

            positionBindable.BindTo(slider.PositionBindable);
            scaleBindable.BindTo(slider.ScaleBindable);
        }

        private void updatePosition() => Position = slider.StackedPosition;

        protected override void Update()
        {
            base.Update();

            slider.Path.Calculate();

            var vertices = new List<Vector2>();
            slider.Path.GetPathToProgress(vertices, 0, 1);

            body.SetVertices(vertices);

            Size = body.Size;
            OriginPosition = body.PathOffset;
        }
    }
}
