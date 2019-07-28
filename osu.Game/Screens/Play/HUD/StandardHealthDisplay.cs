// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Game.Graphics;
using osu.Game.Rulesets.Judgements;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Screens.Play.HUD
{
    public class StandardHealthDisplay : HealthDisplay, IHasAccentColour
    {
        /// <summary>
        /// The base opacity of the glow.
        /// </summary>
        private const float base_glow_opacity = 0.6f;

        /// <summary>
        /// The number of sequential hits required within <see cref="glow_fade_delay"/> to reach the maximum glow opacity.
        /// </summary>
        private const int glow_max_hits = 8;

        /// <summary>
        /// The amount of time to delay before fading the glow opacity back to <see cref="base_glow_opacity"/>.
        /// <para>
        /// This is calculated to require a stream snapped to 1/4 at 150bpm to reach the maximum glow opacity with <see cref="glow_max_hits"/> hits.
        /// </para>
        /// </summary>
        private const float glow_fade_delay = 100;

        /// <summary>
        /// The amount of time to fade the glow to <see cref="base_glow_opacity"/> after <see cref="glow_fade_delay"/>.
        /// </summary>
        private const double glow_fade_time = 500;

        private readonly Container fill;

        public Color4 AccentColour
        {
            get => fill.Colour;
            set => fill.Colour = value;
        }

        private Color4 glowColour;

        public Color4 GlowColour
        {
            get => glowColour;
            set
            {
                if (glowColour == value)
                    return;

                glowColour = value;

                fill.EdgeEffect = new EdgeEffectParameters
                {
                    Colour = glowColour.Opacity(base_glow_opacity),
                    Radius = 8,
                    Roundness = 4,
                    Type = EdgeEffectType.Glow,
                };
            }
        }

        public StandardHealthDisplay()
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                },
                fill = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0, 1),
                    Masking = true,
                    Children = new[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        }
                    }
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AccentColour = colours.BlueLighter;
            GlowColour = colours.BlueDarker;
        }

        public void Flash(JudgementResult result)
        {
            if (result.Type == HitResult.Miss)
                return;

            fill.FadeEdgeEffectTo(Math.Min(1, fill.EdgeEffect.Colour.Linear.A + (1f - base_glow_opacity) / glow_max_hits), 50, Easing.OutQuint)
                .Delay(glow_fade_delay)
                .FadeEdgeEffectTo(base_glow_opacity, glow_fade_time, Easing.OutQuint);
        }

        protected override void SetHealth(float value) => fill.ResizeTo(new Vector2(value, 1), 200, Easing.OutQuint);
    }
}
