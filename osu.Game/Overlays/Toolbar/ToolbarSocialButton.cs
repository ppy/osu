// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarSocialButton : ToolbarOverlayToggleButton
    {
        public ToolbarSocialButton()
        {
            Icon = FontAwesome.fa_users;
        }

        [BackgroundDependencyLoader(true)]
        private void load(SocialOverlay chat)
        {
            StateContainer = chat;
        }
    }
}
