﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.MathUtils;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Catch.Beatmaps
{
    public class CatchBeatmapProcessor : BeatmapProcessor
    {
        public const int RNG_SEED = 1337;

        public CatchBeatmapProcessor(IBeatmap beatmap)
            : base(beatmap)
        {
        }

        public override void PostProcess()
        {
            base.PostProcess();

            ApplyPositionOffsets(Beatmap);

            int index = 0;

            foreach (var obj in Beatmap.HitObjects.OfType<CatchHitObject>())
            {
                obj.IndexInBeatmap = index;
                foreach (var nested in obj.NestedHitObjects.OfType<CatchHitObject>())
                    nested.IndexInBeatmap = index;

                if (obj.LastInCombo && obj.NestedHitObjects.LastOrDefault() is IHasComboInformation lastNested)
                    lastNested.LastInCombo = true;

                index++;
            }
        }

        public static void ApplyPositionOffsets(IBeatmap beatmap, params Mod[] mods)
        {
            var rng = new FastRandom(RNG_SEED);

            bool shouldApplyHardRockOffset = mods.Any(m => m is ModHardRock);
            float? lastPosition = null;
            double lastStartTime = 0;

            foreach (var obj in beatmap.HitObjects.OfType<CatchHitObject>())
            {
                obj.XOffset = 0;

                switch (obj)
                {
                    case Fruit fruit:
                        if (shouldApplyHardRockOffset)
                            applyHardRockOffset(fruit, ref lastPosition, ref lastStartTime, rng);
                        break;

                    case BananaShower bananaShower:
                        foreach (var banana in bananaShower.NestedHitObjects.OfType<Banana>())
                        {
                            banana.XOffset = (float)(rng.NextDouble() * CatchPlayfield.WIDTH);
                            rng.Next(); // osu!stable retrieved a random banana type
                            rng.Next(); // osu!stable retrieved a random banana rotation
                            rng.Next(); // osu!stable retrieved a random banana colour
                        }

                        break;

                    case JuiceStream juiceStream:
                        // Todo: BUG!! Stable used the last control point as the final position of the path, but it should use the computed path instead.
                        lastPosition = juiceStream.OriginalX + juiceStream.Path.ControlPoints[^1].Position.Value.X;

                        // Todo: BUG!! Stable attempted to use the end time of the stream, but referenced it too early in execution and used the start time instead.
                        lastStartTime = juiceStream.StartTime;

                        foreach (var nested in juiceStream.NestedHitObjects)
                        {
                            var catchObject = (CatchHitObject)nested;
                            catchObject.XOffset = 0;

                            if (catchObject is TinyDroplet)
                                catchObject.XOffset = Math.Clamp(rng.Next(-20, 20), -catchObject.OriginalX, CatchPlayfield.WIDTH - catchObject.OriginalX);
                            else if (catchObject is Droplet)
                                rng.Next(); // osu!stable retrieved a random droplet rotation
                        }

                        break;
                }
            }

            initialiseHyperDash(beatmap);
        }

        private static void applyHardRockOffset(CatchHitObject hitObject, ref float? lastPosition, ref double lastStartTime, FastRandom rng)
        {
            float offsetPosition = hitObject.OriginalX;
            double startTime = hitObject.StartTime;

            if (lastPosition == null)
            {
                lastPosition = offsetPosition;
                lastStartTime = startTime;

                return;
            }

            float positionDiff = offsetPosition - lastPosition.Value;

            // Todo: BUG!! Stable calculated time deltas as ints, which affects randomisation. This should be changed to a double.
            int timeDiff = (int)(startTime - lastStartTime);

            if (timeDiff > 1000)
            {
                lastPosition = offsetPosition;
                lastStartTime = startTime;
                return;
            }

            if (positionDiff == 0)
            {
                applyRandomOffset(ref offsetPosition, timeDiff / 4d, rng);
                hitObject.XOffset = offsetPosition - hitObject.OriginalX;
                return;
            }

            // ReSharper disable once PossibleLossOfFraction
            if (Math.Abs(positionDiff) < timeDiff / 3)
                applyOffset(ref offsetPosition, positionDiff);

            hitObject.XOffset = offsetPosition - hitObject.OriginalX;

            lastPosition = offsetPosition;
            lastStartTime = startTime;
        }

        /// <summary>
        /// Applies a random offset in a random direction to a position, ensuring that the final position remains within the boundary of the playfield.
        /// </summary>
        /// <param name="position">The position which the offset should be applied to.</param>
        /// <param name="maxOffset">The maximum offset, cannot exceed 20px.</param>
        /// <param name="rng">The random number generator.</param>
        private static void applyRandomOffset(ref float position, double maxOffset, FastRandom rng)
        {
            bool right = rng.NextBool();
            float rand = Math.Min(20, (float)rng.Next(0, Math.Max(0, maxOffset)));

            if (right)
            {
                // Clamp to the right bound
                if (position + rand <= CatchPlayfield.WIDTH)
                    position += rand;
                else
                    position -= rand;
            }
            else
            {
                // Clamp to the left bound
                if (position - rand >= 0)
                    position -= rand;
                else
                    position += rand;
            }
        }

        /// <summary>
        /// Applies an offset to a position, ensuring that the final position remains within the boundary of the playfield.
        /// </summary>
        /// <param name="position">The position which the offset should be applied to.</param>
        /// <param name="amount">The amount to offset by.</param>
        private static void applyOffset(ref float position, float amount)
        {
            if (amount > 0)
            {
                // Clamp to the right bound
                if (position + amount < CatchPlayfield.WIDTH)
                    position += amount;
            }
            else
            {
                // Clamp to the left bound
                if (position + amount > 0)
                    position += amount;
            }
        }

        private static void initialiseHyperDash(IBeatmap beatmap)
        {
            List<PalpableCatchHitObject> palpableObjects = new List<PalpableCatchHitObject>();

            foreach (var currentObject in beatmap.HitObjects)
            {
                if (currentObject is Fruit fruitObject)
                    palpableObjects.Add(fruitObject);

                if (currentObject is JuiceStream)
                {
                    foreach (var juice in currentObject.NestedHitObjects)
                    {
                        if (juice is PalpableCatchHitObject palpableObject && !(juice is TinyDroplet))
                            palpableObjects.Add(palpableObject);
                    }
                }
            }

            palpableObjects.Sort((h1, h2) => h1.StartTime.CompareTo(h2.StartTime));

            double halfCatcherWidth = Catcher.CalculateCatchWidth(beatmap.BeatmapInfo.BaseDifficulty) / 2;

            // Todo: This is wrong. osu!stable calculated hyperdashes using the full catcher size, excluding the margins.
            // This should theoretically cause impossible scenarios, but practically, likely due to the size of the playfield, it doesn't seem possible.
            // For now, to bring gameplay (and diffcalc!) completely in-line with stable, this code also uses the full catcher size.
            halfCatcherWidth /= Catcher.ALLOWED_CATCH_RANGE;

            int lastDirection = 0;
            double lastExcess = halfCatcherWidth;

            for (int i = 0; i < palpableObjects.Count - 1; i++)
            {
                var currentObject = palpableObjects[i];
                var nextObject = palpableObjects[i + 1];

                // Reset variables in-case values have changed (e.g. after applying HR)
                currentObject.HyperDashTarget = null;
                currentObject.DistanceToHyperDash = 0;

                int thisDirection = nextObject.EffectiveX > currentObject.EffectiveX ? 1 : -1;
                double timeToNext = nextObject.StartTime - currentObject.StartTime - 1000f / 60f / 4; // 1/4th of a frame of grace time, taken from osu-stable
                double distanceToNext = Math.Abs(nextObject.EffectiveX - currentObject.EffectiveX) - (lastDirection == thisDirection ? lastExcess : halfCatcherWidth);
                float distanceToHyper = (float)(timeToNext * Catcher.BASE_SPEED - distanceToNext);

                if (distanceToHyper < 0)
                {
                    currentObject.HyperDashTarget = nextObject;
                    lastExcess = halfCatcherWidth;
                }
                else
                {
                    currentObject.DistanceToHyperDash = distanceToHyper;
                    lastExcess = Math.Clamp(distanceToHyper, 0, halfCatcherWidth);
                }

                lastDirection = thisDirection;
            }
        }
    }
}
