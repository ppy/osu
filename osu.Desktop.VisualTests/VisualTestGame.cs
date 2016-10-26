// Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework;
using osu.Framework.GameModes.Testing;
using osu.Game;

namespace osu.Desktop.VisualTests
{
    class VisualTestGame : OsuGameBase
    {
        public override void Load(BaseGame game)
        {
            base.Load(game);

            Add(new TestBrowser());
        }
    }
}
