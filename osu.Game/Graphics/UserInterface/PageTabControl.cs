// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Graphics.UserInterface
{
    public class PageTabControl<T> : OsuTabControl<T>
    {
        protected override TabItem<T> CreateTabItem(T value) => new PageTabItem(value);

        public PageTabControl()
        {
            Height = 30;
        }

        public class PageTabItem : TabItem<T>
        {
            private const float transition_duration = 100;

            private readonly Box box;

            protected readonly SpriteText Text;

            public PageTabItem(T value) : base(value)
            {
                AutoSizeAxes = Axes.X;
                RelativeSizeAxes = Axes.Y;

                Children = new Drawable[]
                {
                    Text = new OsuSpriteText
                    {
                        Margin = new MarginPadding { Top = 8, Bottom = 8 },
                        Origin = Anchor.BottomLeft,
                        Anchor = Anchor.BottomLeft,
                        Text = (value as Enum)?.GetDescription() ?? value.ToString(),
                        TextSize = 14,
                        Font = @"Exo2.0-Bold",
                    },
                    box = new Box
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 5,
                        Scale = new Vector2(1f, 0f),
                        Colour = Color4.White,
                        Origin = Anchor.BottomLeft,
                        Anchor = Anchor.BottomLeft,
                    },
                    new HoverClickSounds()
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                box.Colour = colours.Yellow;
            }

            protected override bool OnHover(InputState state)
            {
                if (!Active)
                    slideActive();
                return true;
            }

            protected override void OnHoverLost(InputState state)
            {
                if (!Active)
                    slideInactive();
            }

            private void slideActive()
            {
                box.ScaleTo(new Vector2(1f), transition_duration);
            }

            private void slideInactive()
            {
                box.ScaleTo(new Vector2(1f, 0f), transition_duration);
            }

            protected override void OnActivated() => slideActive();

            protected override void OnDeactivated() => slideInactive();
        }
    }
}
