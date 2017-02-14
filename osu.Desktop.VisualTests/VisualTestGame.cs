// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework;
using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics.Cursor;
using osu.Game.Database;
using osu.Game;
using osu.Framework.Desktop.Platform;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Game.Screens.Backgrounds;

namespace osu.Desktop.VisualTests
{
    class VisualTestGame : OsuGameBase
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();

            (new BackgroundModeDefault() { Depth = 10 }).Preload(this, AddInternal);

            // Have to construct this here, rather than in the constructor, because
            // we depend on some dependencies to be loaded within OsuGameBase.load().
            Add(new TestBrowser());
        }
    }
}
