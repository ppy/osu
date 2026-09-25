// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Screens.Select
{
    public partial class FilterControl
    {
        public partial class BPMRangeSlider : ShearedRangeSlider
        {
            public BPMRangeSlider()
                : base(BeatmapsetsStrings.ShowStatsBpm + " Rate")
            {
                NubWidth = ShearedNub.HEIGHT * 1.16f;
                DefaultStringUpperBound = "∞";
            }

            // Force BPM to be of type integer
            protected override BoundSliderBar CreateBoundSlider(bool isUpper) => new BPMBoundSliderBar(this, isUpper);

            private partial class BPMBoundSliderBar : BoundSliderBar
            {
                public BPMBoundSliderBar(ShearedRangeSlider rangeSlider, bool isUpper)
                    : base(rangeSlider, isUpper)
                {
                }

                public override LocalisableString TooltipText =>
                    Current.IsDefault ? string.Empty : $"{Current.Value:N0} BPM";

                protected override void UpdateDisplay(double value)
                {
                    if (Current.IsDefault && DefaultString != null)
                        NubText.Text = DefaultString.Value;
                    else
                        NubText.Text = value.ToLocalisableString(@"N0");
                }
            }
        }
    }
}
