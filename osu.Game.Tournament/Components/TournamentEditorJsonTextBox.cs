// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Overlays.Settings;

namespace osu.Game.Tournament.Components
{
    public partial class TournamentEditorJsonTextBox : SettingsTextBox
    {
        protected override Drawable CreateControl() => new OutlinedTextBox
        {
            RelativeSizeAxes = Axes.X,
            CommitOnFocusLost = true,
            LengthLimit = 262144
        };
    }
}
