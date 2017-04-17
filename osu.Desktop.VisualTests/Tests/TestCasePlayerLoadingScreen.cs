// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Testing;
using osu.Game.Database;
using osu.Game.Screens.Select;
using System.Linq;
using osu.Game.Screens.Play;
using OpenTK;

namespace osu.Desktop.VisualTests.Tests
{
    public class TestCasePlayerLoadingScreen : TestCase
    {
        public override string Description => @"Loading screen in player";

        public override void Reset()
        {
            base.Reset();

            Add(new LoadingScreen
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }
    }
}

