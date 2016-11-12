// Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework;
using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics.Cursor;
using osu.Game.Database;
using osu.Game;
using osu.Framework.Desktop.Platform;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using osu.Game.GameModes.Play;
using SQLiteNetExtensions.Extensions;
using osu.Desktop.Platform;
using osu.Framework.Allocation;

namespace osu.Desktop.VisualTests
{
    class VisualTestGame : OsuGameBase
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Add(new TestBrowser());
        }
    }
}
