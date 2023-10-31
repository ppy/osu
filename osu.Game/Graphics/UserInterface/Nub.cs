// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Overlays;

namespace osu.Game.Graphics.UserInterface
{
    public partial class Nub : Container, IHasCurrentValue<bool>, IHasAccentColour
    {
        public const float HEIGHT = 15;

        public const float DEFAULT_EXPANDED_SIZE = 50;

        private const float border_width = 3;

        private readonly Box fill;
        private readonly Container main;

        public Nub(float expandedSize = DEFAULT_EXPANDED_SIZE)
        {
            Size = new Vector2(expandedSize, HEIGHT);

            InternalChildren = new[]
            {
                main = new CircularContainer
                {
                    BorderColour = Color4.White,
                    BorderThickness = border_width,
                    Masking = true,
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Children = new Drawable[]
                    {
                        fill = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
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

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(onCurrentValueChanged, true);
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

        private readonly Bindable<bool> current = new Bindable<bool>();

        public Bindable<bool> Current
        {
            get => current;
            set
            {
                ArgumentNullException.ThrowIfNull(value);

                current.UnbindBindings();
                current.BindTo(value);
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

        private void onCurrentValueChanged(ValueChangedEvent<bool> filled)
        {
            const double duration = 200;

            fill.FadeTo(filled.NewValue ? 1 : 0, duration, Easing.OutQuint);

            if (filled.NewValue)
            {
                main.ResizeWidthTo(1, duration, Easing.OutElasticHalf);
                main.TransformTo(nameof(BorderThickness), 8.5f, duration, Easing.OutElasticHalf);
            }
            else
            {
                main.ResizeWidthTo(0.75f, duration, Easing.OutQuint);
                main.TransformTo(nameof(BorderThickness), border_width, duration, Easing.OutQuint);
            }
        }
    }
}
