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

namespace osu.Game.Graphics.UserInterface
{
    public class Nub : CircularContainer, IHasCurrentValue<bool>, IHasAccentColour
    {
        public const float COLLAPSED_SIZE = 20;
        public const float EXPANDED_SIZE = 40;

        private const float border_width = 3;

        private const double animate_in_duration = 150;
        private const double animate_out_duration = 500;

        public Nub()
        {
            Box fill;

            Size = new Vector2(COLLAPSED_SIZE, 12);

            BorderColour = Color4.White;
            BorderThickness = border_width;

            Masking = true;

            Children = new[]
            {
                fill = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                    AlwaysPresent = true,
                },
            };

            Current.ValueChanged += filled =>
            {
                fill.FadeTo(filled.NewValue ? 1 : 0, 200, Easing.OutQuint);
                this.TransformTo(nameof(BorderThickness), filled.NewValue ? 8.5f : border_width, 200, Easing.OutQuint);
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AccentColour = colours.Pink;
            GlowingAccentColour = colours.PinkLighter;
            GlowColour = colours.PinkDarker;

            EdgeEffect = new EdgeEffectParameters
            {
                Colour = GlowColour.Opacity(0),
                Type = EdgeEffectType.Glow,
                Radius = 10,
                Roundness = 8,
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
                    this.FadeColour(GlowingAccentColour, animate_in_duration, Easing.OutQuint);
                    FadeEdgeEffectTo(1, animate_in_duration, Easing.OutQuint);
                }
                else
                {
                    FadeEdgeEffectTo(0, animate_out_duration);
                    this.FadeColour(AccentColour, animate_out_duration);
                }
            }
        }

        public bool Expanded
        {
            set
            {
                if (value)
                    this.ResizeTo(new Vector2(EXPANDED_SIZE, 12), animate_in_duration, Easing.OutQuint);
                else
                    this.ResizeTo(new Vector2(COLLAPSED_SIZE, 12), animate_out_duration, Easing.OutQuint);
            }
        }

        private readonly Bindable<bool> current = new Bindable<bool>();

        public Bindable<bool> Current
        {
            get => current;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

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
                    Colour = value;
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
                    Colour = value;
            }
        }

        private Color4 glowColour;

        public Color4 GlowColour
        {
            get => glowColour;
            set
            {
                glowColour = value;

                var effect = EdgeEffect;
                effect.Colour = Glowing ? value : value.Opacity(0);
                EdgeEffect = effect;
            }
        }
    }
}
