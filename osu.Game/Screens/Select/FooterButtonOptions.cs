// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Input.Bindings;

namespace osu.Game.Screens.Select
{
    public class FooterButtonOptions : FooterButton
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Text = @"Options";
            Hotkey = GlobalAction.ToggleBeatmapOptions;
        }
    }
}
