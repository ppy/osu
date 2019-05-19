// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Commands;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarChatButton : ToolbarOverlayToggleButton
    {
        public ToolbarChatButton()
        {
            SetIcon(FontAwesome.Solid.Comments);
        }

        [BackgroundDependencyLoader(true)]
        private void load(ChatOverlay chat, ToggleOverlayCommand<ChatOverlay> toggleOverlayCommand)
        {
            StateContainer = chat;
            Command = toggleOverlayCommand;
        }
    }
}
