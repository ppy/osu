// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// A heart icon that toggles between an outline (inactive) and a filled pink (active)
    /// state with a pop animation, optionally accompanied by a particle burst when transitioning
    /// to the active state.
    /// </summary>
    public partial class HeartIcon : CompositeDrawable
    {
        private readonly SpriteIcon icon;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        public HeartIcon()
        {
            InternalChildren = new Drawable[]
            {
                icon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Icon = FontAwesome.Regular.Heart,
                    RelativeSizeAxes = Axes.Both,
                },
            };
        }

        private const double pop_out_duration = 100;
        private const double pop_in_duration = 500;

        private bool active;

        public void SetActive(bool active, bool withAnimation = false)
        {
            if (this.active == active)
                return;

            this.active = active;

            FinishTransforms(true);

            if (active)
            {
                transitionIcon(FontAwesome.Solid.Heart, colours.Pink1, emphasised: withAnimation);

                if (withAnimation)
                    playFavouriteAnimation();
            }
            else
            {
                transitionIcon(FontAwesome.Regular.Heart, colourProvider.Content2);
            }
        }

        private void transitionIcon(IconUsage newIcon, Color4 colour, bool emphasised = false)
        {
            icon.ScaleTo(emphasised ? 0.5f : 0.8f, pop_out_duration, Easing.OutQuad)
                .Then()
                .FadeColour(colour)
                .Schedule(() => icon.Icon = newIcon)
                .ScaleTo(1, pop_in_duration, Easing.OutElasticHalf);
        }

        private void playFavouriteAnimation()
        {
            var circle = new FastCircle
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(0.5f),
                Blending = BlendingParameters.Additive,
                Alpha = 0,
                Depth = float.MinValue,
            };

            AddInternal(circle);

            circle.Delay(pop_out_duration)
                  .FadeTo(0.35f)
                  .FadeOut(1400, Easing.OutCubic)
                  .ScaleTo(10f, 750, Easing.OutQuint)
                  .Expire();

            const int num_particles = 8;

            static float randomFloat(float min, float max) => min + Random.Shared.NextSingle() * (max - min);

            for (int i = 0; i < num_particles; i++)
            {
                double duration = randomFloat(600, 1000);
                float angle = (i + randomFloat(0, 0.75f)) / num_particles * MathF.PI * 2;
                var direction = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                float distance = randomFloat(DrawWidth / 2, DrawWidth);

                var particle = new FastCircle
                {
                    Position = direction * DrawWidth / 4,
                    Size = new Vector2(3),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Blending = BlendingParameters.Additive,
                    Alpha = 0,
                    Depth = 2,
                    Colour = colours.Pink,
                };

                AddInternal(particle);

                particle
                    .Delay(pop_out_duration)
                    .FadeTo(0.5f)
                    .MoveTo(direction * distance, 1300, Easing.OutQuint)
                    .FadeOut(duration, Easing.Out)
                    .ScaleTo(0.5f, duration)
                    .Expire();
            }
        }
    }
}
