// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Input.EventArgs;
using osu.Framework.Input.States;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using OpenTK;

namespace osu.Game.Screens.Select.Leaderboards
{
    public class RetrievalFailurePlaceholder : Placeholder
    {
        public Action OnRetry;

        public RetrievalFailurePlaceholder()
        {
            AddArbitraryDrawable(new RetryButton
            {
                Action = () => OnRetry?.Invoke(),
                Padding = new MarginPadding { Right = 10 }
            });

            AddText(@"Couldn't retrieve scores!");
        }

        public class RetryButton : OsuHoverContainer
        {
            private readonly SpriteIcon icon;

            public new Action Action;

            public RetryButton()
            {
                AutoSizeAxes = Axes.Both;

                Child = new OsuClickableContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Action = () => Action?.Invoke(),
                    Child = icon = new SpriteIcon
                    {
                        Icon = FontAwesome.fa_refresh,
                        Size = new Vector2(TEXT_SIZE),
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
