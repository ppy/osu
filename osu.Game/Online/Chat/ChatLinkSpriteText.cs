// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using System.Linq;

namespace osu.Game.Online.Chat
{
    public class ChatLinkSpriteText : OsuLinkSpriteText
    {
        public int LinkId;

        private Color4 hoverColour;
        private Color4 urlColour;

        protected override bool OnHover(InputState state)
        {
            // Every word is one sprite in chat (for word wrap) so we need to find all other sprites that display the same link
            var otherSpritesWithSameLink = ((Container<Drawable>)Parent).Children.Where(child => (child as ChatLinkSpriteText)?.LinkId == LinkId && !Equals(child));

            var hoverResult = base.OnHover(state);

            if (!otherSpritesWithSameLink.Any(sprite => sprite.IsHovered))
                foreach (ChatLinkSpriteText sprite in otherSpritesWithSameLink)
                    sprite.TriggerOnHover(state);

            Content.FadeColour(hoverColour, 500, Easing.OutQuint);

            return hoverResult;
        }

        protected override void OnHoverLost(InputState state)
        {
            var spritesWithSameLink = ((Container<Drawable>)Parent).Children.Where(child => (child as ChatLinkSpriteText)?.LinkId == LinkId);

            if (spritesWithSameLink.Any(sprite => sprite.IsHovered))
            {
                // We have to do this so this sprite does not fade its colour back
                Content.FadeColour(hoverColour, 500, Easing.OutQuint);
                return;
            }

            foreach (ChatLinkSpriteText sprite in spritesWithSameLink)
                sprite.Content.FadeColour(urlColour, 500, Easing.OutQuint);

            base.OnHoverLost(state);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            hoverColour = colours.Yellow;
            urlColour = colours.Blue;
        }
    }
}
