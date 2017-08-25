// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Sprites;
using System;

namespace osu.Game.Graphics.Containers
{
    public class OsuTextFlowContainer : TextFlowContainer
    {
        public OsuTextFlowContainer(Action<SpriteText> defaultCreationParameters = null) : base(defaultCreationParameters)
        {
        }

        protected override SpriteText CreateSpriteText() => new OsuSpriteText();

        public void AddIcon(FontAwesome icon, Action<SpriteText> creationParameters = null) => AddText(((char)icon).ToString(), creationParameters);
    }
}
