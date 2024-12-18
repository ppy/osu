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
            const float base_game_width = 1024f;
            const float base_game_height = 768f;

            // extra bottom space for the catcher to not get cut off at tall resolutions lower than 4:3 (e.g. 5:4). number chosen based on testing with maximum catcher scale (i.e. CS 0).
            const float extra_bottom_space = 200f;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            InternalChild = new Container
            {
                // This container limits vertical visibility of the playfield to ensure fairness between wide and tall resolutions (i.e. tall resolutions should not see more fruits).
                // Note that the container still extends across the screen horizontally, so that hit explosions at the sides of the playfield do not get cut off.
                Name = "Visible area",
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                Height = base_game_height + extra_bottom_space,
                Y = extra_bottom_space / 2,
                Masking = true,
                Child = new Container
                {
                    Name = "Playable area",
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    // playfields in stable are positioned vertically at three fourths the difference between the playfield height and the window height in stable.
                    Y = base_game_height * ((1 - playfield_size_adjust) / 4 * 3),
                    Size = new Vector2(base_game_width, base_game_height) * playfield_size_adjust,
                    Child = content = new ScalingContainer { RelativeSizeAxes = Axes.Both }
                },
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
