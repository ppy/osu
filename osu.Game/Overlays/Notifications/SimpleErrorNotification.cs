// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.Notifications
{
    public class SimpleErrorNotification : SimpleNotification
    {
        public override string PopInSampleName => "UI/error-notification-pop-in";

        public SimpleErrorNotification()
        {
            Icon = FontAwesome.Solid.Bomb;
        }
    }
}
