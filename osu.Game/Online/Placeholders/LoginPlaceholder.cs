// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;

namespace osu.Game.Online.Placeholders
{
    public sealed class LoginPlaceholder : Placeholder
    {
        public LoginPlaceholder(string actionMessage)
        {
            AddArbitraryDrawable(new LoginButton(actionMessage));
        }

        private class LoginButton : OsuAnimatedButton
        {
            [Resolved(CanBeNull = true)]
            private LoginOverlay login { get; set; }

            public LoginButton(string actionMessage)
            {
                AutoSizeAxes = Axes.Both;

                var textFlowContainer = new OsuTextFlowContainer(cp => cp.Font = cp.Font.With(size: TEXT_SIZE))
                {
                    AutoSizeAxes = Axes.Both,
                    Margin = new MarginPadding(5)
                };

                Child = textFlowContainer;

                textFlowContainer.AddIcon(FontAwesome.Solid.UserLock, icon =>
                {
                    icon.Padding = new MarginPadding { Right = 10 };
                });

                textFlowContainer.AddText(actionMessage);

                Action = () => login?.Show();
            }
        }
    }
}
