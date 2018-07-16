// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Input;

namespace osu.Game.Overlays.Changelog.Header
{
    public class TextBadgePairRelease : TextBadgePair
    {
        private TextBadgePairListing listingBadge;

        public TextBadgePairRelease(ColourInfo badgeColour, string displayText) : base(badgeColour, displayText)
        {
            this.listingBadge = listingBadge;
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
            ClearTransforms();
            if (text.IsPresent) ChangeText(250, displayText);
            else ShowText();
            OnActivation?.Invoke();
        }

        public override void Deactivate()
        {
            FinishTransforms(true);
            HideText(250);
            OnDeactivation?.Invoke();
        }
    }
}
