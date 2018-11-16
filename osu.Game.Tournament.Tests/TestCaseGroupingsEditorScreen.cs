// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Tournament.Screens.Groupings;

namespace osu.Game.Tournament.Tests
{
    public class TestCaseGroupingsEditorScreen : LadderTestCase
    {
        public TestCaseGroupingsEditorScreen()
        {
            Add(new GroupingsEditorScreen());
        }
    }
}
