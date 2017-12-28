// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Sprites;
using System;
using System.Collections.Generic;

namespace osu.Game.Graphics.Containers
{
    public class OsuLinkFlowContainer : OsuLinkFlowContainer<OsuSpriteLink>
    {
        public OsuLinkFlowContainer(Action<SpriteText> defaultCreationParameters = null)
            : base(defaultCreationParameters)
        {
        }
    }

    public class OsuLinkFlowContainer<T> : OsuTextFlowContainer
        where T : OsuSpriteLink, new()
    {
        public override bool HandleInput => true;

        public OsuLinkFlowContainer(Action<SpriteText> defaultCreationParameters = null)
            : base(defaultCreationParameters)
        {
        }

        protected override SpriteText CreateSpriteText() => new T();

        /// <summary>
        /// The colour for text (links override this). Will only be used for new text elements.
        /// </summary>
        public ColourInfo TextColour = Color4.White;

        public IEnumerable<SpriteText> AddLink(string text, string url, Action<SpriteText> creationParameters = null)
        {
            // TODO: Remove this and get word wrapping working
            text = text.Replace(' ', '_');

            return AddText(text, link =>
            {
                ((OsuSpriteLink)link).Url = url;
                creationParameters?.Invoke(link);
            });
        }

        public new IEnumerable<SpriteText> AddText(string text, Action<SpriteText> creationParameters = null)
        {
            return base.AddText(text, sprite =>
            {
                ((OsuSpriteLink)sprite).TextColour = TextColour;
                creationParameters?.Invoke(sprite);
            });
        }
    }
}
