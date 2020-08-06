// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Input.Bindings;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarChatButton : ToolbarOverlayToggleButton
    {
        public ToolbarChatButton()
        {
            SetIcon(FontAwesome.Solid.Comments);
            TooltipMain = "Chat";
            TooltipSub = "Join the real-time discussion";

            Hotkey = GlobalAction.ToggleChat;
        }

        [BackgroundDependencyLoader(true)]
        private void load(ChatOverlay chat)
        {
            StateContainer = chat;
        }
    }
}
