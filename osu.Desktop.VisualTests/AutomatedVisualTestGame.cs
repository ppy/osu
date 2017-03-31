// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Testing;
using osu.Game;

namespace osu.Desktop.VisualTests
{
    public class AutomatedVisualTestGame : OsuGameBase
    {
        public AutomatedVisualTestGame()
        {
            Add(new TestRunner(new TestBrowser()));
        }
    }
}