// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Rulesets.Edit.Layers.Selection;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Osu.Edit.Layers.Selection.Overlays
{
    public class SliderOverlay : HitObjectOverlay
    {
        private readonly SliderBody body;
        private readonly DrawableSlider slider;

        public SliderOverlay(DrawableSlider slider)
            : base(slider)
        {
            this.slider = slider;

            var obj = (Slider)slider.HitObject;

            InternalChildren = new Drawable[]
            {
                body = new SliderBody(obj)
                {
                    AccentColour = Color4.Transparent,
                    PathWidth = obj.Scale * 64
                },
                new SliderCircleOverlay(slider.HeadCircle, slider),
                new SliderCircleOverlay(slider.TailCircle, slider),
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            body.BorderColour = colours.Yellow;
        }

        protected override void Update()
        {
            base.Update();

            Position = slider.Position;
            Size = slider.Size;
            OriginPosition = slider.OriginPosition;

            // Need to cause one update
            body.UpdateProgress(0);
        }
    }
}
