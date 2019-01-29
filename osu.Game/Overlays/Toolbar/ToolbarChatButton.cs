// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarChatButton : ToolbarOverlayToggleButton
    {
        public ToolbarChatButton()
        {
            SetIcon(FontAwesome.fa_comments);
        }

        [BackgroundDependencyLoader(true)]
        private void load(ChatOverlay chat)
        {
            StateContainer = chat;
        }
    }
}
