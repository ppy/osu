// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Settings
{
    /// <summary>
    /// A <see cref="SettingsButton"/> with pink colours to mark dangerous/destructive actions.
    /// </summary>
    public partial class DangerousSettingsButton : SettingsButton
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            BackgroundColour = colours.DangerousButtonColour;
        }
    }
}
