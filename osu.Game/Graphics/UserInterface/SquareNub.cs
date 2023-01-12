// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Overlays;

namespace osu.Game.Graphics.UserInterface
{
    public partial class SquareNub : Container, IHasAccentColour
    {
        // Setting default to 30 since current use cases will be using this height
        public const float HEIGHT = 30;

        public const float EXPANDED_SIZE = 50;

        private const float border_width = 3;

        private readonly Container main;

        public SquareNub()
        {
            Size = new Vector2(EXPANDED_SIZE, HEIGHT);

            InternalChildren = new[]
            {
                main = new Container
                {
                    Masking = true,
                    RelativeSizeAxes = Axes.Both,
                    CornerRadius = 5,
                    BorderColour = Color4.White,
                    BorderThickness = border_width,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            AlwaysPresent = true,
                        },
                    }
                },
            };
        }

        [BackgroundDependencyLoader(true)]
        private void load(OverlayColourProvider? colourProvider, OsuColour colours)
        {
            AccentColour = colourProvider?.Highlight1 ?? colours.Pink;
            GlowingAccentColour = colourProvider?.Highlight1.Lighten(0.2f) ?? colours.PinkLighter;
            GlowColour = colourProvider?.Highlight1 ?? colours.PinkLighter;

            main.EdgeEffect = new EdgeEffectParameters
            {
                Colour = GlowColour.Opacity(0),
                Type = EdgeEffectType.Glow,
                Radius = 8,
                Roundness = 4,
            };
        }

        private bool glowing;

        public bool Glowing
        {
            get => glowing;
            set
            {
                glowing = value;

                if (value)
                {
                    main.FadeColour(GlowingAccentColour.Lighten(0.5f), 40, Easing.OutQuint)
                        .Then()
                        .FadeColour(GlowingAccentColour, 800, Easing.OutQuint);

                    main.FadeEdgeEffectTo(Color4.White.Opacity(0.1f), 40, Easing.OutQuint)
                        .Then()
                        .FadeEdgeEffectTo(GlowColour.Opacity(0.1f), 800, Easing.OutQuint);
                }
                else
                {
                    main.FadeEdgeEffectTo(GlowColour.Opacity(0), 800, Easing.OutQuint);
                    main.FadeColour(AccentColour, 800, Easing.OutQuint);
                }
            }
        }

        private Color4 accentColour;

        public Color4 AccentColour
        {
            get => accentColour;
            set
            {
                accentColour = value;
                if (!Glowing)
                    main.Colour = value;
            }
        }

        private Color4 glowingAccentColour;

        public Color4 GlowingAccentColour
        {
            get => glowingAccentColour;
            set
            {
                glowingAccentColour = value;
                if (Glowing)
                    main.Colour = value;
            }
        }

        private Color4 glowColour;

        public Color4 GlowColour
        {
            get => glowColour;
            set
            {
                glowColour = value;

                var effect = main.EdgeEffect;
                effect.Colour = Glowing ? value : value.Opacity(0);
                main.EdgeEffect = effect;
            }
        }
    }
}
