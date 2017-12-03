// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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

        public void AddLink(string text, string url, Action<SpriteText> creationParameters = null)
        {
            AddText(text, link =>
            {
                LoadComponentAsync(link, d => ((T)d).Url = url);
                creationParameters?.Invoke(link);
            });
        }
    }
}
