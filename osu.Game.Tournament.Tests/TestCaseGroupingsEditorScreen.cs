// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
