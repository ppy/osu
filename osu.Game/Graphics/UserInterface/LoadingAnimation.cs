// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// A loading spinner.
    /// </summary>
    public class LoadingAnimation : VisibilityContainer
    {
        private readonly SpriteIcon spinner;

        private const float spin_duration = 600;
        private const float transition_duration = 200;

        public LoadingAnimation()
        {
            Size = new Vector2(20);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Children = new Drawable[]
            {
                spinner = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Icon = FontAwesome.Solid.CircleNotch,
                    Shadow = true
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            spinner.Spin(spin_duration, RotationDirection.Clockwise);
        }

        protected override void PopIn() => this.FadeIn(transition_duration * 2, Easing.OutQuint);

        protected override void PopOut() => this.FadeOut(transition_duration, Easing.OutQuint);
    }
}
