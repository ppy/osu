// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Notifications
{
    public partial class OutageNotification : SimpleNotification
    {
        private readonly LocalisableString message;

        public OutageNotification(LocalisableString message)
        {
            Text = this.message = message;

            IsCritical = true;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Icon = FontAwesome.Solid.FireExtinguisher;
            IconContent.Colour = ColourInfo.GradientVertical(Colour4.Orange, Colour4.OrangeRed);

            TextFlow.Clear();
            TextFlow.AddText(NotificationsStrings.ServerOutageInProgress.ToUpper(), s =>
            {
                s.Font = OsuFont.Style.Caption2.With(weight: FontWeight.Bold);
                s.Colour = Colour4.Orange;
            });
            TextFlow.AddParagraph(message, s => s.Font = OsuFont.Style.Caption1);
        }
    }
}
