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

            applyPositionOffsets();

            initialiseHyperDash((List<CatchHitObject>)Beatmap.HitObjects);

            int index = 0;
            foreach (var obj in Beatmap.HitObjects.OfType<CatchHitObject>())
            {
                obj.IndexInBeatmap = index++;
                if (obj.LastInCombo && obj.NestedHitObjects.LastOrDefault() is IHasComboInformation lastNested)
                    lastNested.LastInCombo = true;
            }
        }

        private void applyPositionOffsets()
        {
            var rng = new FastRandom(RNG_SEED);
            // todo: HardRock displacement should be applied here

            foreach (var obj in Beatmap.HitObjects)
            {
                switch (obj)
                {
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
