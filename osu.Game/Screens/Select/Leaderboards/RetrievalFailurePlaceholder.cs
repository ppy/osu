// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using OpenTK;

namespace osu.Game.Screens.Select.Leaderboards
{
    public class RetrievalFailurePlaceholder : Placeholder
    {
        public Action OnRetry;

        public RetrievalFailurePlaceholder()
        {
            Direction = FillDirection.Horizontal;
            AutoSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                new RetryButton
                {
                    Action = () => OnRetry?.Invoke(),
                    Margin = new MarginPadding { Right = 10 },
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.TopLeft,
                    Text = @"Couldn't retrieve scores!",
                    TextSize = 22,
                },
            };
        }

        public class RetryButton : OsuHoverContainer
        {
            private readonly SpriteIcon icon;

            public new Action Action;

            public RetryButton()
            {
                Height = 26;
                Width = 26;
                Child = new OsuClickableContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Action = () => Action?.Invoke(),
                    Child = icon = new SpriteIcon
                    {
                        Icon = FontAwesome.fa_refresh,
                        Size = new Vector2(26),
                        Shadow = true,
                    },
                };
            }

            protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
            {
                icon.ScaleTo(0.8f, 4000, Easing.OutQuint);
                return base.OnMouseDown(state, args);
            }

            protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
            {
                icon.ScaleTo(1, 1000, Easing.OutElastic);
                return base.OnMouseUp(state, args);
            }
        }
    }
}
