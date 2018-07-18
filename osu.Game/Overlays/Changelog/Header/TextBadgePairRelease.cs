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
            text.Font = "Exo2.0-Bold";
            text.Y = 20;
            text.Alpha = 0;
        }

        public void SetText(string displayText)
        {
            text.Text = displayText;
        }

        public void Activate(string displayText = null)
        {
            //ClearTransforms();
            // not using if (!lineBadge.IsCollapsed) because the text sometimes gets reset
            // when quickly switching release streams
            if (text.IsPresent) ChangeText(transition_duration, displayText);
            else ShowText(transition_duration, displayText);
            OnActivation?.Invoke();
        }

        public override void Deactivate()
        {
            //FinishTransforms(true);
            HideText(transition_duration);
            OnDeactivation?.Invoke();
        }
    }
}
