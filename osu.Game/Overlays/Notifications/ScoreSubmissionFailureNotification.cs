// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Notifications
{
    public partial class ScoreSubmissionFailureNotification : SimpleNotification
    {
        private readonly string heading;
        private readonly string reason;

        public ScoreSubmissionFailureNotification(string heading, string reason)
        {
            this.heading = heading;
            this.reason = reason;

            IsCritical = true;

            Text = $"{heading}: {reason}";
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Icon = FontAwesome.Solid.Unlink;
            IconContent.Colour = colours.RedDark;

            TextFlow.Clear();
            TextFlow.AddText(heading.ToUpperInvariant(), s =>
            {
                s.Font = OsuFont.Style.Caption2.With(weight: FontWeight.Bold);
                s.Colour = colours.Red0;
            });
            TextFlow.AddParagraph(reason, s => s.Font = OsuFont.Style.Caption1);
        }
    }
}
