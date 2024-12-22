// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;

namespace osu.Game.Screens.Edit.Audio
{
    public partial class AudioScreen : EditorScreenWithTimeline
    {
        public AudioScreen()
            : base(EditorScreenMode.Audio)
        { }

        protected override Drawable CreateMainContent()
        {
            return new ScreenWhiteBox.UnderConstructionMessage("Audio mode");
        }
    }
}
