// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Input.Commands;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarDirectButton : ToolbarOverlayToggleButton
    {
        public ToolbarDirectButton()
        {
            SetIcon(OsuIcon.ChevronDownCircle);
        }

        [BackgroundDependencyLoader(true)]
        private void load(DirectOverlay direct, ToggleOnlineOverlayCommand<DirectOverlay> toggleOverlayCommand)
        {
            StateContainer = direct;
            Command = toggleOverlayCommand;
        }
    }
}
