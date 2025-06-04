// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    public partial class ShearedNub : Container, IHasCurrentValue<bool>, IHasAccentColour
    {
        public Action? OnDoubleClicked { get; init; }

        public const int HEIGHT = 30;
        public const float EXPANDED_SIZE = 50;
        public const float CORNER_RADIUS = 5;

        private readonly Box fill;
        private readonly Container main;
        private readonly Container shadow;

        public ShearedNub()
        {
            Size = new Vector2(EXPANDED_SIZE, HEIGHT);
            InternalChildren = new Drawable[]
            {
                shadow = new Container
                {
                    Shear = OsuGame.SHEAR,
                    Masking = true,
                    CornerRadius = CORNER_RADIUS,
                    RelativeSizeAxes = Axes.Both,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Radius = 20f,
                    },
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        AlwaysPresent = true,
                    }
                },
                main = new Container
                {
                    Shear = OsuGame.SHEAR,
                    BorderColour = Colour4.White,
                    BorderThickness = 8f,
                    Masking = true,
                    CornerRadius = CORNER_RADIUS,
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Child = fill = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        AlwaysPresent = true,
                    }
                },
            };
        }

        [BackgroundDependencyLoader(true)]
        private void load(OverlayColourProvider? colourProvider, OsuColour colours)
        {
            AccentColour = colourProvider?.Highlight1 ?? colours.Pink;
            GlowingAccentColour = colourProvider?.Highlight1.Lighten(0.4f) ?? colours.PinkLighter;
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
            FinishTransforms(true);
        }

        private bool glowing;

        public bool Glowing
        {
            get => glowing;
            set
            {
                if (glowing == value)
                    return;

                glowing = value;
                updateDisplay();
            }
        }

        private Color4 shadowColour = Color4.Black.Opacity(0f);

        public Color4 ShadowColour
        {
            get => shadowColour;
            set
            {
                if (shadowColour == value)
                    return;

                shadowColour = value;
                shadow.FadeEdgeEffectTo(value, 800, Easing.OutQuint);
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
                updateDisplay();
            }
        }

        private Color4 glowingAccentColour;

        public Color4 GlowingAccentColour
        {
            get => glowingAccentColour;
            set
            {
                glowingAccentColour = value;
                updateDisplay();
            }
        }

        private Color4 glowColour;

        public Color4 GlowColour
        {
            get => glowColour;
            set
            {
                glowColour = value;
                updateDisplay();
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
                main.TransformTo(nameof(BorderThickness), 8f, duration, Easing.OutQuint);
            }
        }

        private void updateDisplay()
        {
            if (Glowing)
            {
                main.FadeColour(GlowingAccentColour.Lighten(0.1f), 40, Easing.OutQuint)
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

        protected override bool OnClick(ClickEvent e) => true;

        protected override bool OnDoubleClick(DoubleClickEvent e)
        {
            OnDoubleClicked?.Invoke();
            return true;
        }
    }
}
