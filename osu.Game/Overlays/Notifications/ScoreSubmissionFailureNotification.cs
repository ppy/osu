// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Notifications
{
    public partial class ScoreSubmissionFailureNotification : SimpleNotification
    {
        private readonly LocalisableString heading;
        private readonly LocalisableString reason;

        public ScoreSubmissionFailureNotification(LocalisableString heading, LocalisableString reason)
        {
            this.heading = heading;
            this.reason = reason;

            IsCritical = true;

            Text = LocalisableString.Interpolate($"{heading}: {reason}");
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Icon = FontAwesome.Solid.Unlink;
            IconContent.Colour = colours.RedDark;

            TextFlow.Clear();
            TextFlow.AddText(heading.ToUpper(), s =>
            {
                s.Font = OsuFont.Style.Caption2.With(weight: FontWeight.Bold);
                s.Colour = colours.Red0;
            });
            TextFlow.AddParagraph(reason, s => s.Font = OsuFont.Style.Caption1);
        }
    }
}
