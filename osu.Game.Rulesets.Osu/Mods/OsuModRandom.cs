// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
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
        public override LocalisableString Description => "It never gets boring!";

        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(OsuModTargetPractice)).ToArray();

        [SettingSource("Angle sharpness", "How sharp angles should be")]
        public BindableFloat AngleSharpness { get; } = new BindableFloat(7)
        {
            MinValue = 1,
            MaxValue = 10,
            Precision = 0.1f
        };

        private static readonly float playfield_diagonal = OsuPlayfield.BASE_SIZE.LengthFast;

        private Random random = null!;

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            if (beatmap is not OsuBeatmap osuBeatmap)
                return;

            Seed.Value ??= RNG.Next();

            random = new Random((int)Seed.Value);

            var positionInfos = OsuHitObjectGenerationUtils.GeneratePositionInfos(osuBeatmap.HitObjects);

            // Offsets the angles of all hit objects in a "section" by the same amount.
            float sectionOffset = 0;

            // Whether the angles are positive or negative (clockwise or counter-clockwise flow).
            bool flowDirection = false;

            for (int i = 0; i < positionInfos.Count; i++)
            {
                if (shouldStartNewSection(osuBeatmap, positionInfos, i))
                {
                    sectionOffset = getRandomOffset(0.0008f);
                    flowDirection = !flowDirection;
                }

                if (positionInfos[i].HitObject is Slider slider && random.NextDouble() < 0.5)
                {
                    OsuHitObjectGenerationUtils.FlipSliderInPlaceHorizontally(slider);
                }

                if (i == 0)
                {
                    positionInfos[i].DistanceFromPrevious = (float)(random.NextDouble() * OsuPlayfield.BASE_SIZE.Y / 2);
                    positionInfos[i].RelativeAngle = (float)(random.NextDouble() * 2 * Math.PI - Math.PI);
                }
                else
                {
                    // Offsets only the angle of the current hit object if a flow change occurs.
                    float flowChangeOffset = 0;

                    // Offsets only the angle of the current hit object.
                    float oneTimeOffset = getRandomOffset(0.002f);

                    if (shouldApplyFlowChange(positionInfos, i))
                    {
                        flowChangeOffset = getRandomOffset(0.002f);
                        flowDirection = !flowDirection;
                    }

                    float totalOffset =
                        // sectionOffset and oneTimeOffset should mainly affect patterns with large spacing.
                        (sectionOffset + oneTimeOffset) * positionInfos[i].DistanceFromPrevious +
                        // flowChangeOffset should mainly affect streams.
                        flowChangeOffset * (playfield_diagonal - positionInfos[i].DistanceFromPrevious);

                    positionInfos[i].RelativeAngle = getRelativeTargetAngle(positionInfos[i].DistanceFromPrevious, totalOffset, flowDirection);
                }
            }

            osuBeatmap.HitObjects = OsuHitObjectGenerationUtils.RepositionHitObjects(positionInfos);
        }

        private float getRandomOffset(float stdDev)
        {
            // Range: [0.5, 2]
            // Higher angle sharpness -> lower multiplier
            float customMultiplier = (1.5f * AngleSharpness.MaxValue - AngleSharpness.Value) / (1.5f * AngleSharpness.MaxValue - AngleSharpness.Default);

            return OsuHitObjectGenerationUtils.RandomGaussian(random, 0, stdDev * customMultiplier);
        }

        /// <param name="targetDistance">The target distance between the previous and the current <see cref="OsuHitObject"/>.</param>
        /// <param name="offset">The angle (in rad) by which the target angle should be offset.</param>
        /// <param name="flowDirection">Whether the relative angle should be positive or negative.</param>
        private float getRelativeTargetAngle(float targetDistance, float offset, bool flowDirection)
        {
            // Range: [0.1, 1]
            float angleSharpness = AngleSharpness.Value / AngleSharpness.MaxValue;
            // Range: [0, 0.9]
            float angleWideness = 1 - angleSharpness;

            // Range: [-60, 30]
            float customOffsetX = angleSharpness * 100 - 70;
            // Range: [-0.075, 0.15]
            float customOffsetY = angleWideness * 0.25f - 0.075f;

            targetDistance += customOffsetX;
            float angle = (float)(2.16 / (1 + 200 * Math.Exp(0.036 * (targetDistance - 310 + customOffsetX))) + 0.5);
            angle += offset + customOffsetY;

            float relativeAngle = (float)Math.PI - angle;

            return flowDirection ? -relativeAngle : relativeAngle;
        }

        /// <returns>Whether a new section should be started at the current <see cref="OsuHitObject"/>.</returns>
        private bool shouldStartNewSection(OsuBeatmap beatmap, IReadOnlyList<OsuHitObjectGenerationUtils.ObjectPositionInfo> positionInfos, int i)
        {
            if (i == 0)
                return true;

            // Exclude new-combo-spam and 1-2-combos.
            bool previousObjectStartedCombo = positionInfos[Math.Max(0, i - 2)].HitObject.IndexInCurrentCombo > 1 &&
                                              positionInfos[i - 1].HitObject.NewCombo;
            bool previousObjectWasOnDownbeat = OsuHitObjectGenerationUtils.IsHitObjectOnBeat(beatmap, positionInfos[i - 1].HitObject, true);
            bool previousObjectWasOnBeat = OsuHitObjectGenerationUtils.IsHitObjectOnBeat(beatmap, positionInfos[i - 1].HitObject);

            return (previousObjectStartedCombo && random.NextDouble() < 0.6f) ||
                   previousObjectWasOnDownbeat ||
                   (previousObjectWasOnBeat && random.NextDouble() < 0.4f);
        }

        /// <returns>Whether a flow change should be applied at the current <see cref="OsuHitObject"/>.</returns>
        private bool shouldApplyFlowChange(IReadOnlyList<OsuHitObjectGenerationUtils.ObjectPositionInfo> positionInfos, int i)
        {
            // Exclude new-combo-spam and 1-2-combos.
            bool previousObjectStartedCombo = positionInfos[Math.Max(0, i - 2)].HitObject.IndexInCurrentCombo > 1 &&
                                              positionInfos[i - 1].HitObject.NewCombo;

            return previousObjectStartedCombo && random.NextDouble() < 0.6f;
        }
    }
}
