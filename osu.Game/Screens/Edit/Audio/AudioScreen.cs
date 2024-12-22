// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Screens.Edit.Audio
{
    public partial class AudioScreen : EditorScreen
    {
        public AudioScreen()
            : base(EditorScreenMode.Audio)
        {
            Child = new ScreenWhiteBox.UnderConstructionMessage("Audio mode");
        }
    }
}
