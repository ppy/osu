// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Osu.Utils;

namespace osu.Game.Rulesets.Osu.Mods
{
    /// <summary>
    /// Mod that randomises the positions of the <see cref="HitObject"/>s
    /// </summary>
    public class OsuModRandom : ModRandom, IApplicableToBeatmap
    {
        public override string Description => "It never gets boring!";

        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(OsuModTarget)).ToArray();

        private Random? rng;

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            if (!(beatmap is OsuBeatmap osuBeatmap))
                return;

            Seed.Value ??= RNG.Next();

            rng = new Random((int)Seed.Value);

            var positionInfos = OsuHitObjectGenerationUtils.GeneratePositionInfos(osuBeatmap.HitObjects);

            float sequenceOffset = 0;
            bool flowDirection = false;

            for (int i = 0; i < positionInfos.Count; i++)
            {
                bool invertFlow = false;

                if (i == 0 ||
                    (positionInfos[i - 1].HitObject.NewCombo && (i <= 1 || !positionInfos[i - 2].HitObject.NewCombo) && (i <= 2 || !positionInfos[i - 3].HitObject.NewCombo)) ||
                    OsuHitObjectGenerationUtils.IsHitObjectOnBeat(osuBeatmap, positionInfos[i - 1].HitObject, true) ||
                    (OsuHitObjectGenerationUtils.IsHitObjectOnBeat(osuBeatmap, positionInfos[i - 1].HitObject) && rng.NextDouble() < 0.25))
                {
                    sequenceOffset = OsuHitObjectGenerationUtils.RandomGaussian(rng, 0, 0.02f);

                    if (rng.NextDouble() < 0.6)
                        invertFlow = true;
                }

                if (i == 0)
                {
                    positionInfos[i].DistanceFromPrevious = (float)(rng.NextDouble() * OsuPlayfield.BASE_SIZE.Y / 2);
                    positionInfos[i].RelativeAngle = (float)(rng.NextDouble() * 2 * Math.PI - Math.PI);
                }
                else
                {
                    float flowChangeOffset = 0;
                    float oneTimeOffset = OsuHitObjectGenerationUtils.RandomGaussian(rng, 0, 0.03f);

                    if (positionInfos[i - 1].HitObject.NewCombo && (i <= 1 || !positionInfos[i - 2].HitObject.NewCombo) && rng.NextDouble() < 0.6)
                    {
                        flowChangeOffset = OsuHitObjectGenerationUtils.RandomGaussian(rng, 0, 0.05f);

                        if (rng.NextDouble() < 0.8)
                            invertFlow = true;
                    }

                    if (invertFlow)
                        flowDirection ^= true;

                    positionInfos[i].RelativeAngle = OsuHitObjectGenerationUtils.GetRelativeTargetAngle(
                        positionInfos[i].DistanceFromPrevious,
                        (sequenceOffset + oneTimeOffset) * (float)Math.Sqrt(positionInfos[i].DistanceFromPrevious) +
                        flowChangeOffset * (float)Math.Sqrt(640 - positionInfos[i].DistanceFromPrevious),
                        flowDirection
                    );
                }
            }

            osuBeatmap.HitObjects = OsuHitObjectGenerationUtils.RepositionHitObjects(positionInfos);
        }
    }
}
