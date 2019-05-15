// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Colour;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Changelog.Header
{
    public class BreadcrumbRelease : Breadcrumb
    {
        private const float transition_duration = 125;

        public BreadcrumbRelease(ColourInfo badgeColour, string displayText)
            : base(badgeColour, displayText)
        {
            Text.Font = Text.Font.With(weight: FontWeight.Bold);
            Text.Y = 20;
            Text.Alpha = 0;
        }

        public void ShowBuild(string displayText = null)
        {
            ShowText(transition_duration, displayText);
            IsActivated = true;
        }

        public override void Deactivate()
        {
            if (!IsActivated)
                return;

            HideText(transition_duration);
        }
    }
}
