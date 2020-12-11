// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Chat
{
    public class CloseAllTabsButton : OsuClickableContainer
    {
        private readonly OsuSpriteText text;

        public CloseAllTabsButton()
        {
            AutoSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                text = new OsuSpriteText() {
                    Text = "Close All"
                }
            };
        }
        protected override bool OnHover(HoverEvent e)
        {
            text.FadeColour(Color4.Red, 200, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            text.FadeColour(Color4.White, 200, Easing.OutQuint);
            base.OnHoverLost(e);
        }
    }
}
