// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Screens.Edit.Setup
{
    public class SetupScreen : EditorScreen
    {
        public SetupScreen()
        {
            Child = new ScreenWhiteBox.UnderConstructionMessage("Setup mode");
        }
    }
}
