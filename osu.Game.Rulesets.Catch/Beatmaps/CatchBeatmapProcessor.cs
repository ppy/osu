// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects.Types;
using osuTK;
using osu.Game.Rulesets.Catch.MathUtils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;

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

            initialiseHyperDash((List<CatchHitObject>)Beatmap.HitObjects);

            int index = 0;

            foreach (var obj in Beatmap.HitObjects.OfType<CatchHitObject>())
            {
                obj.IndexInBeatmap = index++;
                if (obj.LastInCombo && obj.NestedHitObjects.LastOrDefault() is IHasComboInformation lastNested)
                    lastNested.LastInCombo = true;
            }
        }

        public static void ApplyPositionOffsets(IBeatmap beatmap, params Mod[] mods)
        {
            var rng = new FastRandom(RNG_SEED);

            bool shouldApplyHardRockOffset = mods.Any(m => m is ModHardRock);
            float? lastPosition = null;
            double lastStartTime = 0;

            foreach (var obj in beatmap.HitObjects)
            {
                switch (obj)
                {
                    case Fruit fruit:
                        if (shouldApplyHardRockOffset)
                            applyHardRockOffset(fruit, ref lastPosition, ref lastStartTime, rng);
                        break;

                    case BananaShower bananaShower:
                        foreach (var banana in bananaShower.NestedHitObjects.OfType<Banana>())
                        {
                            banana.X = (float)rng.NextDouble();
                            rng.Next(); // osu!stable retrieved a random banana type
                            rng.Next(); // osu!stable retrieved a random banana rotation
                            rng.Next(); // osu!stable retrieved a random banana colour
                        }

                        break;

                    case JuiceStream juiceStream:
                        foreach (var nested in juiceStream.NestedHitObjects)
                        {
                            var hitObject = (CatchHitObject)nested;
                            if (hitObject is TinyDroplet)
                                hitObject.X += rng.Next(-20, 20) / CatchPlayfield.BASE_WIDTH;
                            else if (hitObject is Droplet)
                                rng.Next(); // osu!stable retrieved a random droplet rotation
                            hitObject.X = MathHelper.Clamp(hitObject.X, 0, 1);
                        }

                        break;
                }
            }
        }

        private static void applyHardRockOffset(HitObject hitObject, ref float? lastPosition, ref double lastStartTime, FastRandom rng)
        {
            if (hitObject is JuiceStream stream)
            {
                lastPosition = stream.EndX;
                lastStartTime = stream.EndTime;
                return;
            }

            if (!(hitObject is Fruit))
                return;

            var catchObject = (CatchHitObject)hitObject;

            float position = catchObject.X;
            double startTime = hitObject.StartTime;

            if (lastPosition == null)
            {
                lastPosition = position;
                lastStartTime = startTime;

                return;
            }

            float positionDiff = position - lastPosition.Value;
            double timeDiff = startTime - lastStartTime;

            if (timeDiff > 1000)
            {
                lastPosition = position;
                lastStartTime = startTime;
                return;
            }

            if (positionDiff == 0)
            {
                applyRandomOffset(ref position, timeDiff / 4d, rng);
                catchObject.X = position;
                return;
            }

            if (Math.Abs(positionDiff * CatchPlayfield.BASE_WIDTH) < timeDiff / 3d)
                applyOffset(ref position, positionDiff);

            catchObject.X = position;

            lastPosition = position;
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
            float rand = Math.Min(20, (float)rng.Next(0, Math.Max(0, maxOffset))) / CatchPlayfield.BASE_WIDTH;

            if (right)
            {
                // Clamp to the right bound
                if (position + rand <= 1)
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
                if (position + amount < 1)
                    position += amount;
            }
            else
            {
                // Clamp to the left bound
                if (position + amount > 0)
                    position += amount;
            }
        }

        private void initialiseHyperDash(List<CatchHitObject> objects)
        {
            List<CatchHitObject> objectWithDroplets = new List<CatchHitObject>();

            foreach (var currentObject in objects)
            {
                if (currentObject is Fruit)
                    objectWithDroplets.Add(currentObject);
                if (currentObject is JuiceStream)
                    foreach (var currentJuiceElement in currentObject.NestedHitObjects)
                        if (!(currentJuiceElement is TinyDroplet))
                            objectWithDroplets.Add((CatchHitObject)currentJuiceElement);
            }

            objectWithDroplets.Sort((h1, h2) => h1.StartTime.CompareTo(h2.StartTime));

            double halfCatcherWidth = CatcherArea.GetCatcherSize(Beatmap.BeatmapInfo.BaseDifficulty) / 2;
            int lastDirection = 0;
            double lastExcess = halfCatcherWidth;

            for (int i = 0; i < objectWithDroplets.Count - 1; i++)
            {
                CatchHitObject currentObject = objectWithDroplets[i];
                CatchHitObject nextObject = objectWithDroplets[i + 1];

                int thisDirection = nextObject.X > currentObject.X ? 1 : -1;
                double timeToNext = nextObject.StartTime - currentObject.StartTime - 1000f / 60f / 4; // 1/4th of a frame of grace time, taken from osu-stable
                double distanceToNext = Math.Abs(nextObject.X - currentObject.X) - (lastDirection == thisDirection ? lastExcess : halfCatcherWidth);
                float distanceToHyper = (float)(timeToNext * CatcherArea.Catcher.BASE_SPEED - distanceToNext);

                if (distanceToHyper < 0)
                {
                    currentObject.HyperDashTarget = nextObject;
                    lastExcess = halfCatcherWidth;
                }
                else
                {
                    currentObject.DistanceToHyperDash = distanceToHyper;
                    lastExcess = MathHelper.Clamp(distanceToHyper, 0, halfCatcherWidth);
                }

                lastDirection = thisDirection;
            }
        }
    }
}
