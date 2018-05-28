// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Screens.Multi;
using osu.Game.Screens.Multi.Screens.Lounge;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseMultiHeader : OsuTestCase
    {
        public TestCaseMultiHeader()
        {
            Lounge lounge;
            Children = new Drawable[]
            {
                lounge = new Lounge
                {
                    Padding = new MarginPadding { Top = Header.HEIGHT },
                },
                new Header(lounge),
            };
        }
    }
}
