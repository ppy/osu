// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Toolbar
{
    internal class ToolbarNotificationButton : ToolbarOverlayToggleButton
    {
        protected override Anchor TooltipAnchor => Anchor.TopRight;

        public ToolbarNotificationButton()
        {
            Icon = FontAwesome.fa_bars;
            TooltipMain = "Notifications";
            TooltipSub = "Waiting for 'ya";
        }

        [BackgroundDependencyLoader]
        private void load(NotificationManager notificationManager)
        {
            StateContainer = notificationManager;
            Action = notificationManager.ToggleVisibility;
        }
    }
}