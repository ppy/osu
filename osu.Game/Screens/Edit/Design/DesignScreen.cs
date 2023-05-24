// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Screens.Edit.Design
{
    public partial class DesignScreen : EditorScreen
    {
        public DesignScreen()
            : base(EditorScreenMode.Design)
        {
            Child = new ScreenWhiteBox.UnderConstructionMessage("Design mode");
        }
    }
}
