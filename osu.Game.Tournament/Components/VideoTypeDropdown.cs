// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Overlays.Settings;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Screens.Board.Components
{
    public partial class VideoTypeDropdown : SettingsDropdown<string>
    {
        public VideoTypeDropdown()
        {
            BackgroundVideoProps.DISPLAY_NAMES.ForEach(kvp => Control.AddDropdownItem(kvp.Value));
            Current.Value = BackgroundVideoProps.GetDisplayName(BackgroundVideo.Board);
        }
    }
}
