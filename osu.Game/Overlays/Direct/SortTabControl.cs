// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Direct
{
    public class SortTabControl : OsuTabControl<SortCriteria>
    {
        protected override TabItem<SortCriteria> CreateTabItem(SortCriteria value) => new SortTabItem(value);

        public SortTabControl()
        {
            Height = 30;
        }

        private class SortTabItem : TabItem<SortCriteria>
        {
            private const float transition_duration = 100;

            private readonly Box box;

            public override bool Active
            {
                get { return base.Active; }
                set
                {
                    if (Active == value) return;

                    if (value)
                        slideActive();
                    else
                        slideInactive();
                    base.Active = value;
                }
            }

            public SortTabItem(SortCriteria value) : base(value)
            {
                AutoSizeAxes = Axes.X;
                RelativeSizeAxes = Axes.Y;

                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Margin = new MarginPadding { Top = 8, Bottom = 8 },
                        Origin = Anchor.BottomLeft,
                        Anchor = Anchor.BottomLeft,
                        Text = (value as Enum).GetDescription() ?? value.ToString(),
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
        }
    }

    public enum SortCriteria
    {
        Title,
        Artist,
        Creator,
        Difficulty,
        Ranked,
        Rating,
    }
}
