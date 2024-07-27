// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Localisation;
using osu.Game.Extensions;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Utils;

namespace osu.Game.Skinning.Components
{ 
    [UsedImplicitly]
    public partial class ModdedLengthDisplay : ModdedAttributeDisplay
    {
        protected override LocalisableString AttributeLabel => ArtistStrings.TracklistLength.ToTitle();

        protected override void UpdateValue()
        {
            double rate = ModUtils.CalculateRateWithMods(Mods.Value);
            Current.Value = TimeSpan.FromMilliseconds(BeatmapInfo.Length / rate).ToFormattedDuration();
        }
    }
}
