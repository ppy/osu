// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Graphics.Containers
{
    public class OsuTextFlowContainer: TextFlowContainer
    {
        protected override SpriteText CreateSpriteText() => new OsuSpriteText();

        public IEnumerable<SpriteText> AddTextAwesome(FontAwesome icon, Action<SpriteText> creationParameters = null) => AddText(((char)icon).ToString(), creationParameters);
    }
}
