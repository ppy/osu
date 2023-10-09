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
            public ScalingContainer()
            {
                Anchor = Anchor.BottomCentre;
                Origin = Anchor.BottomCentre;
            }

            protected override void Update()
            {
                base.Update();

                // in stable, fruit fall vertically from 100 pixels above the playfield top down to the catcher's Y position (i.e. -100 to 340),
                // see: https://github.com/peppy/osu-stable-reference/blob/1531237b63392e82c003c712faa028406073aa8f/osu!/GameplayElements/HitObjects/Fruits/HitCircleFruits.cs#L65
                // we already have the playfield positioned similar to stable (see CatchPlayfieldAdjustmentContainer constructor),
                // so we only need to increase this container's height 100 pixels above the playfield, and offset it to have the bottom at 340 rather than 384.
                const float stable_fruit_start_position = -100;
                const float stable_catcher_y_position = 340;
                const float playfield_v_size_adjustment = (stable_catcher_y_position - stable_fruit_start_position) / CatchPlayfield.HEIGHT;
                const float playfield_v_catcher_offset = stable_catcher_y_position - CatchPlayfield.HEIGHT;

                Scale = new Vector2(Parent!.ChildSize.X / CatchPlayfield.WIDTH);
                Position = new Vector2(0f, playfield_v_catcher_offset * Scale.Y);
                Size = Vector2.Divide(new Vector2(1, playfield_v_size_adjustment), Scale);
            }
        }
    }
}
