// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Catch.UI
{
    public partial class CatchPlayfieldAdjustmentContainer : PlayfieldAdjustmentContainer
    {
        private const float playfield_size_adjust = 0.8f;

        protected override Container<Drawable> Content => content;
        private readonly Container content;

        public CatchPlayfieldAdjustmentContainer()
        {
            // playfields in stable are usually positioned vertically at exactly three quarters of the window's first quarter.
            // the code below essentially sets Y to 0.25 * 0.75 (i.e. 0.1875), but it's done in a way that matches stable code concept-wise.
            const float stable_playfield_ratio = 1f;
            const float stable_window_ratio = 1f / playfield_size_adjust;

            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
            RelativePositionAxes = Axes.Y;
            Y = (stable_window_ratio - stable_playfield_ratio) / 4 * 3;

            Size = new Vector2(playfield_size_adjust);

            InternalChild = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fit,
                FillAspectRatio = 4f / 3,
                Child = content = new ScalingContainer { RelativeSizeAxes = Axes.Both, }
            };
        }

        /// <summary>
        /// A <see cref="Container"/> which scales its content relative to a target width.
        /// </summary>
        private partial class ScalingContainer : Container
        {
            protected override void Update()
            {
                base.Update();

                // in stable, fruit fall vertically from -100 to 340.
                // to emulate this, we want to make our playfield 440 gameplay pixels high.
                // we then offset it -100 vertically in the position set below.
                const float stable_v_offset_ratio = 440 / CatchPlayfield.HEIGHT;

                // there is no explanation to this offset, but applying it brings lazer nearly 1:1 with stable.
                const float stable_offset = 6f;

                Scale = new Vector2(Parent!.ChildSize.X / CatchPlayfield.WIDTH);
                Position = new Vector2(0f, -100 * Scale.Y * stable_v_offset_ratio - stable_offset);
                Size = Vector2.Divide(new Vector2(1, stable_v_offset_ratio), Scale);
            }
        }
    }
}
