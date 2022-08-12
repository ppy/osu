// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
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

        private static readonly float playfield_diagonal = OsuPlayfield.BASE_SIZE.LengthFast;

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
                if (i == 0 ||
                    (positionInfos[Math.Max(0, i - 2)].HitObject.IndexInCurrentCombo > 1 && positionInfos[i - 1].HitObject.NewCombo && rng.NextDouble() < 0.6) ||
                    OsuHitObjectGenerationUtils.IsHitObjectOnBeat(osuBeatmap, positionInfos[i - 1].HitObject, true) ||
                    (OsuHitObjectGenerationUtils.IsHitObjectOnBeat(osuBeatmap, positionInfos[i - 1].HitObject) && rng.NextDouble() < 0.4))
                {
                    sequenceOffset = OsuHitObjectGenerationUtils.RandomGaussian(rng, 0, 0.001f);
                    flowDirection = !flowDirection;
                }

                if (i == 0)
                {
                    positionInfos[i].DistanceFromPrevious = (float)(rng.NextDouble() * OsuPlayfield.BASE_SIZE.Y / 2);
                    positionInfos[i].RelativeAngle = (float)(rng.NextDouble() * 2 * Math.PI - Math.PI);
                }
                else
                {
                    float flowChangeOffset = 0;
                    float oneTimeOffset = OsuHitObjectGenerationUtils.RandomGaussian(rng, 0, 0.002f);

                    if (positionInfos[Math.Max(0, i - 2)].HitObject.IndexInCurrentCombo > 1 && positionInfos[i - 1].HitObject.NewCombo && rng.NextDouble() < 0.6)
                    {
                        flowChangeOffset = OsuHitObjectGenerationUtils.RandomGaussian(rng, 0, 0.002f);
                        flowDirection = !flowDirection;
                    }

                    positionInfos[i].RelativeAngle = getRelativeTargetAngle(
                        positionInfos[i].DistanceFromPrevious,
                        (sequenceOffset + oneTimeOffset) * positionInfos[i].DistanceFromPrevious +
                        flowChangeOffset * (playfield_diagonal - positionInfos[i].DistanceFromPrevious),
                        flowDirection
                    );
                }
            }

            osuBeatmap.HitObjects = OsuHitObjectGenerationUtils.RepositionHitObjects(positionInfos);
        }

        /// <param name="targetDistance">The target distance between the previous and the current <see cref="OsuHitObject"/>.</param>
        /// <param name="offset">The angle (in rad) by which the target angle should be offset.</param>
        /// <param name="flowDirection">Whether the relative angle should be positive or negative.</param>
        private static float getRelativeTargetAngle(float targetDistance, float offset, bool flowDirection)
        {
            float angle = (float)(2.16 / (1 + 200 * Math.Exp(0.036 * (targetDistance - 320))) + 0.5 + offset);
            float relativeAngle = (float)Math.PI - angle;
            return flowDirection ? -relativeAngle : relativeAngle;
        }
    }
}
