//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework;
using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics.Cursor;
using osu.Game.Database;
using osu.Game;
using osu.Framework.Desktop.Platform;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using SQLiteNetExtensions.Extensions;
using osu.Framework.Allocation;

namespace osu.Desktop.VisualTests
{
    class VisualTestGame : OsuGameBase
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Have to construct this here, rather than in the constructor, because
            // we depend on some dependencies to be loaded within OsuGameBase.load().
            Add(new TestBrowser());
        }
    }
}
