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
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;

            // playfields in stable are positioned vertically at three fourths the difference between the playfield height and the window height in stable.
            // we can match that in lazer by using relative coordinates for Y and considering window height to be 1, and playfield height to be 0.8.
            RelativePositionAxes = Axes.Y;
            Y = (1 - playfield_size_adjust) / 4 * 3;

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
                const float stable_v_offset_ratio = 440 / 384f;

                Scale = new Vector2(Parent.ChildSize.X / CatchPlayfield.WIDTH);
                Position = new Vector2(0, -100 * stable_v_offset_ratio + Scale.X);
                Size = Vector2.Divide(new Vector2(1, stable_v_offset_ratio), Scale);
            }
        }
    }
}
