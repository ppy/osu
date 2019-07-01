// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Tournament.Screens.Editors;

namespace osu.Game.Tournament.Tests.Screens
{
    public class TestSceneRoundEditorScreen : LadderTestScene
    {
        public TestSceneRoundEditorScreen()
        {
            Add(new RoundEditorScreen
            {
                Width = 0.85f // create room for control panel
            });
        }
    }
}
