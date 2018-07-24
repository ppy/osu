// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Screens.Edit.Screens.Setup.Components;

namespace osu.Game.Tests.Visual
{
    public class TestCaseLinkSpriteText : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(LinkSpriteText)
        };

        public TestCaseLinkSpriteText()
        {
            Child = new LinkSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Link = "https://osu.ppy.sh/home",
                Text = "osu!home",
                TextSize = 20,
            };
        }
    }
}
