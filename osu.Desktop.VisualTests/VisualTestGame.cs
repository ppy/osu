// Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics.Cursor;
using osu.Game.Database;
using osu.Game;

namespace osu.Framework.VisualTests
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
