// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Utils;

namespace osu.Game.Skinning.Components
{
    [UsedImplicitly]
    public partial class ModdedBPMDisplay : ModdedAttributeDisplay
    {
        protected override LocalisableString AttributeLabel => BeatmapsetsStrings.ShowStatsBpm;

        protected override void UpdateValue()
        {
            double rate = ModUtils.CalculateRateWithMods(Mods.Value);
            Current.Value = FormatUtils.RoundBPM(BeatmapInfo.BPM, rate).ToLocalisableString(@"F0");
        }
    }
}
