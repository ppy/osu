// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Graphics.UserInterface
{
    public class Nub : CircularContainer, IHasCurrentValue<bool>
    {
        public const float COLLAPSED_SIZE = 20;
        public const float EXPANDED_SIZE = 40;

        private const float border_width = 3;
        private Color4 glowingColour, idleColour;

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

            Current.ValueChanged += newValue =>
            {
                if (newValue)
                    fill.FadeIn(200, EasingTypes.OutQuint);
                else
                    fill.FadeTo(0.01f, 200, EasingTypes.OutQuint); //todo: remove once we figure why containers aren't drawing at all times
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = idleColour = colours.Pink;
            glowingColour = colours.PinkLighter;

            EdgeEffect = new EdgeEffect
            {
                Colour = colours.PinkDarker,
                Type = EdgeEffectType.Glow,
                Radius = 10,
                Roundness = 8,
            };

            FadeEdgeEffectTo(0);
        }

        public bool Glowing
        {
            set
            {
                if (value)
                {
                    FadeColour(glowingColour, 500, EasingTypes.OutQuint);
                    FadeEdgeEffectTo(1, 500, EasingTypes.OutQuint);
                }
                else
                {
                    FadeEdgeEffectTo(0, 500);
                    FadeColour(idleColour, 500);
                }
            }
        }

        public bool Expanded
        {
            set
            {
                ResizeTo(new Vector2(value ? EXPANDED_SIZE : COLLAPSED_SIZE, 12), 500, EasingTypes.OutQuint);
            }
        }

        public Bindable<bool> Current { get; } = new Bindable<bool>();
    }
}
