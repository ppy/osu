// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.Notifications
{
    public partial class ProgressCompletionNotification : SimpleNotification
    {
        public override string PopInSampleName => "UI/notification-done";

        public ProgressCompletionNotification()
        {
            Icon = FontAwesome.Solid.Check;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            IconContent.Colour = ColourInfo.GradientVertical(colours.GreenDark, colours.GreenLight);
        }
    }
}
