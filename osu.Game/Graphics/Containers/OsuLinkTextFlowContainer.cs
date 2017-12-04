// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Graphics.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Graphics.Containers
{
    public class OsuLinkTextFlowContainer : OsuLinkTextFlowContainer<OsuLinkSpriteText>
    {
        public OsuLinkTextFlowContainer(Action<SpriteText> defaultCreationParameters = null)
            : base(defaultCreationParameters)
        {
        }
    }

    public class OsuLinkTextFlowContainer<T> : OsuTextFlowContainer
        where T : OsuLinkSpriteText, new()
    {
        public override bool HandleInput => true;

        public OsuLinkTextFlowContainer(Action<SpriteText> defaultCreationParameters = null) : base(defaultCreationParameters)
        {
        }

        protected override SpriteText CreateSpriteText() => new T();

        /// <summary>
        /// The colour for normal text (links ignore this). Will only be used for new text elements.
        /// <para>Default is white.</para>
        /// </summary>
        public ColourInfo? TextColour;

        public void AddLink(string text, string url, Action<SpriteText> creationParameters = null)
        {
            AddText(text, link =>
            {
                creationParameters?.Invoke(link);
                LoadComponentAsync(link, d => ((T)d).Url = url);
            });
        }

        public IEnumerable<SpriteText> AddText(string text, Action<SpriteText> creationParameters = null)
        {
            return base.AddText(text, sprite =>
            {
                if (TextColour.HasValue)
                    ((OsuLinkSpriteText)sprite).TextColour = TextColour.Value;

                creationParameters?.Invoke(sprite);
            });
        }
    }
}
