// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Commands;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Online.Leaderboards
{
    public class RetrievalFailurePlaceholder : Placeholder
    {
        public ICommand OnRetry;

        public RetrievalFailurePlaceholder()
        {
            AddArbitraryDrawable(new RetryButton
            {
                Command = OnRetry,
                Padding = new MarginPadding { Right = 10 }
            });

            AddText(@"Couldn't retrieve scores!");
        }

        public class RetryButton : OsuHoverContainer
        {
            private readonly SpriteIcon icon;

            public new ICommand Command;

            public RetryButton()
            {
                AutoSizeAxes = Axes.Both;

                Child = new OsuClickableContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Command = Command,
                    Child = icon = new SpriteIcon
                    {
                        Icon = FontAwesome.Solid.Sync,
                        Size = new Vector2(TEXT_SIZE),
                        Shadow = true,
                    },
                };
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                icon.ScaleTo(0.8f, 4000, Easing.OutQuint);
                return base.OnMouseDown(e);
            }

            protected override bool OnMouseUp(MouseUpEvent e)
            {
                icon.ScaleTo(1, 1000, Easing.OutElastic);
                return base.OnMouseUp(e);
            }
        }
    }
}
