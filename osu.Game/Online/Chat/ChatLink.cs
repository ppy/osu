// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Chat;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Online.Chat
{
    public class ChatLink : OsuLinkSpriteText, IHasTooltip
    {
        public int LinkId = -1;

        private Color4 hoverColour;
        private Color4 urlColour;

        private readonly ChatHoverContainer content;

        protected IEnumerable<ChatLink> SameLinkSprites { get; private set; }

        protected override Container<Drawable> Content => content ?? base.Content;

        public string TooltipText => LinkId != -1 ? Url : null;

        public ChatLink()
        {
            AddInternal(content = new ChatHoverContainer
            {
                AutoSizeAxes = Axes.Both,
            });

            OnLoadComplete = d =>
            {
                // All sprites in the same chatline that represent the same URL
                SameLinkSprites = (d.Parent as Container<Drawable>).Children.Where(child => (child as ChatLink)?.LinkId == LinkId && !d.Equals(child)).Cast<ChatLink>();
            };
        }

        protected override bool OnHover(InputState state)
        {
            var hoverResult = base.OnHover(state);

            if (!SameLinkSprites.Any(sprite => sprite.IsHovered))
                foreach (ChatLink sprite in SameLinkSprites)
                    sprite.TriggerOnHover(state);

            Content.FadeColour(hoverColour, 500, Easing.OutQuint);

            return hoverResult;
        }

        protected override void OnHoverLost(InputState state)
        {
            if (SameLinkSprites.Any(sprite => sprite.IsHovered))
            {
                // We have to do this so this sprite does not fade its colour back
                Content.FadeColour(hoverColour, 500, Easing.OutQuint);
                return;
            }

            foreach (ChatLink sprite in SameLinkSprites)
                sprite.Content.FadeColour(urlColour, 500, Easing.OutQuint);

            base.OnHoverLost(state);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            hoverColour = colours.Yellow;
            urlColour = colours.Blue;
            if (LinkId != -1)
                Content.Colour = urlColour;
        }

        private class ChatHoverContainer : OsuHoverContainer, IHasHoverSounds
        {
            public bool ShouldPlayHoverSound => ((ChatLink)Parent).SameLinkSprites.All(sprite => !sprite.IsHovered);
        }
    }
}
