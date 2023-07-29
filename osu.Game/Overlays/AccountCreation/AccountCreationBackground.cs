// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Overlays.AccountCreation
{
    public partial class AccountCreationBackground : Sprite
    {
        public AccountCreationBackground()
        {
            FillMode = FillMode.Fill;
            RelativeSizeAxes = Axes.Both;

            Anchor = Anchor.CentreRight;
            Origin = Anchor.CentreRight;
        }

        [BackgroundDependencyLoader]
        private void load(LargeTextureStore textures)
        {
            Texture = textures.Get("Backgrounds/registration");
        }

        protected override void LoadComplete()
        {
            const float x_movement = 80;

            const float initial_move_time = 5000;
            const float loop_move_time = 10000;

            base.LoadComplete();
            this.FadeInFromZero(initial_move_time / 4, Easing.OutQuint);
            this.MoveToX(x_movement / 2).MoveToX(0, initial_move_time, Easing.OutQuint);

            using (BeginDelayedSequence(initial_move_time))
            {
                this
                    .MoveToX(x_movement, loop_move_time, Easing.InOutSine)
                    .Then().MoveToX(0, loop_move_time, Easing.InOutSine)
                    .Loop();
            }
        }
    }
}
