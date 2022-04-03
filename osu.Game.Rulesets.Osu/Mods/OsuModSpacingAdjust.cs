// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Utils;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModSpacingAdjust : Mod, IApplicableToBeatmap
    {
        public override string Name => "Spacing Adjust";

        public override string Description => "Adjust object spacing to your liking.";

        public override double ScoreMultiplier => 1;

        public override string Acronym => "SA";

        public override ModType Type => ModType.Conversion;

        public override bool RequiresConfiguration => true;

        [SettingSource("Object spacing", "Modifies the spacing between objects.")]
        public BindableNumber<float> ObjectSpacing { get; } = new BindableFloat
        {
            MinValue = 0.5f,
            MaxValue = 2,
            Default = 1,
            Value = 1,
            Precision = 0.01f,
        };

        public override string SettingDescription => ObjectSpacing.IsDefault ? string.Empty : $"{ObjectSpacing.Value:N2}x";

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            if (!(beatmap is OsuBeatmap osuBeatmap))
                return;

            var positionInfos = OsuHitObjectGenerationUtils.GeneratePositionInfos(osuBeatmap.HitObjects);
            var firstObjectsAfterBreaks = osuBeatmap.Breaks
                                                    .Select(period => osuBeatmap.HitObjects.FirstOrDefault(o => o.GetEndTime() >= period.EndTime))
                                                    .Where(o => o != null)
                                                    .ToList();

            for (int i = 0; i < positionInfos.Count; i++)
            {
                var positionInfo = positionInfos[i];

                if (i == 0 || positionInfos[i - 1].HitObject is Spinner || firstObjectsAfterBreaks.Contains(positionInfo.HitObject))
                {
                    positionInfo.StayInPlace = true;
                }

                positionInfo.DistanceFromPrevious *= ObjectSpacing.Value;
            }

            osuBeatmap.HitObjects = OsuHitObjectGenerationUtils.RepositionHitObjects(positionInfos);
        }
    }
}
