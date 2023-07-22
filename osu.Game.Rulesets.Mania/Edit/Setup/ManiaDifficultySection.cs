// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Screens.Edit.Setup;

namespace osu.Game.Rulesets.Mania.Edit.Setup
{
    public partial class ManiaDifficultySection : DifficultySection
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            CircleSizeSlider.Label = BeatmapsetsStrings.ShowStatsCsMania;
            CircleSizeSlider.Description = "The number of columns in the beatmap";
            if (CircleSizeSlider.Current is BindableNumber<float> circleSizeFloat)
                circleSizeFloat.Precision = 1;
        }
    }
}
