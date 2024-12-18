// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Graphics;
using osu.Game.Input.Bindings;

namespace osu.Game.Screens.Select
{
    public partial class FooterButtonOptions : FooterButton
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            SelectedColour = colours.Blue;
            DeselectedColour = SelectedColour.Opacity(0.5f);
            Text = @"options";
            Hotkey = GlobalAction.ToggleBeatmapOptions;
        }
    }
}
