// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Configuration
{
    public enum ScreenshotFormat
    {
        [LocalisableDescription(typeof(GraphicsSettingsStrings), nameof(GraphicsSettingsStrings.Jpg))]
        Jpg = 1,

        [LocalisableDescription(typeof(GraphicsSettingsStrings), nameof(GraphicsSettingsStrings.Png))]
        Png = 2
    }
}
