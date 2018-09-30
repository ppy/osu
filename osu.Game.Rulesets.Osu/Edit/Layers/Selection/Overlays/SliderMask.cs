// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Game.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Osu.Edit.Layers.Selection.Overlays
{
    public class SliderMask : HitObjectMask
    {
        private readonly SliderBody body;
        private readonly DrawableSlider slider;

        public SliderMask(DrawableSlider slider)
            : base(slider)
        {
            this.slider = slider;

            Position = slider.Position;

            var sliderObject = (Slider)slider.HitObject;

            InternalChildren = new Drawable[]
            {
                body = new SliderBody(sliderObject)
                {
                    AccentColour = Color4.Transparent,
                    PathWidth = sliderObject.Scale * 64
                },
                new SliderCircleMask(slider.HeadCircle, slider),
                new SliderCircleMask(slider.TailCircle, slider),
            };

            sliderObject.PositionChanged += _ => Position = slider.Position;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            body.BorderColour = colours.Yellow;
        }

        protected override void Update()
        {
            base.Update();

            Size = slider.Size;
            OriginPosition = slider.OriginPosition;

            // Need to cause one update
            body.UpdateProgress(0);
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => body.ReceivePositionalInputAt(screenSpacePos);

        public override Vector2 SelectionPoint => ToScreenSpace(OriginPosition);
        public override Quad SelectionQuad => body.PathDrawQuad;
    }
}
