// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarTimeButton : ToolbarButton
    {
        [Resolved(CanBeNull = true)]
        private NotificationOverlay notifications { get; set; }

        public ToolbarTimeButton()
        {
            SetIcon(FontAwesome.Solid.Clock);
            TooltipMain =   "时间";
            TooltipSub =    "现在几点了?";
            Action = () => SendTimeToNotifation();
        }

        private void SendTimeToNotifation()
        {
                var dt = DateTime.Now;
                notifications?.Post(new SimpleNotification
                {
                    Text =  $"当前时间: \n"+
                            $"{GetTimeInfo()}",
                    Icon = FontAwesome.Solid.CheckCircle,
                });
                return;
        }

        private DateTime GetTimeInfo()
        {
            var dt = DateTime.Now;
            return dt;
        }

    }
}
