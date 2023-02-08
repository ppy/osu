// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Screens.Edit
{
    public enum EditorScreenMode
    {
        [LocalisableDescription(typeof(EditorStrings), nameof(EditorStrings.SetupScreen))]
        SongSetup,

        [LocalisableDescription(typeof(EditorStrings), nameof(EditorStrings.ComposeScreen))]
        Compose,

        [LocalisableDescription(typeof(EditorStrings), nameof(EditorStrings.DesignScreen))]
        Design,

        [LocalisableDescription(typeof(EditorStrings), nameof(EditorStrings.TimingScreen))]
        Timing,

        [LocalisableDescription(typeof(EditorStrings), nameof(EditorStrings.VerifyScreen))]
        Verify,
    }
}
