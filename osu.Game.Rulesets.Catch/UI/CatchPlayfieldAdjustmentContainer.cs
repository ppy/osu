// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CatchPlayfieldAdjustmentContainer : PlayfieldAdjustmentContainer
    {
        private const float playfield_size_adjust = 0.8f;

        protected override Container<Drawable> Content => content;
        private readonly Container content;

        public CatchPlayfieldAdjustmentContainer()
        {
            // because we are using centre anchor/origin, we will need to limit visibility in the future
            // to ensure tall windows do not get a readability advantage.
            // it may be possible to bake the catch-specific offsets (-100..340 mentioned below) into new values
            // which are compatible with TopCentre alignment.
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

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
        private class ScalingContainer : Container
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
