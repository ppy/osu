// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Testing;
using osu.Game;

namespace osu.Desktop.VisualTests
{
    public class AutomatedVisualTestGame : OsuGameBase
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Have to construct this here, rather than in the constructor, because
            // we depend on some dependencies to be loaded within OsuGameBase.load().
            Add(new TestRunner(new TestBrowser()));
        }
    }
}