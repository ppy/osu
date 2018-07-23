// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Colour;

namespace osu.Game.Overlays.Changelog.Header
{
    public class TextBadgePairRelease : TextBadgePair
    {
        private TextBadgePairListing listingBadge;
        private const float transition_duration = 125;

        public TextBadgePairRelease(ColourInfo badgeColour, string displayText) : base(badgeColour, displayText)
        {
            Text.Font = "Exo2.0-Bold";
            Text.Y = 20;
            Text.Alpha = 0;
        }

        public void SetText(string displayText) => Text.Text = displayText;

        public void Activate(string displayText = null)
        {
            if (IsActivated)
            {
                if (displayText != Text.Text)
                    ChangeText(transition_duration, displayText);
            }
            else
                ShowText(transition_duration, displayText);
            IsActivated = true;
        }

        public override void Deactivate()
        {
            IsActivated = false;
            HideText(transition_duration);
        }
    }
}
