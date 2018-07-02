// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects;
using OpenTK;
using osu.Game.Rulesets.Catch.MathUtils;

namespace osu.Game.Rulesets.Catch.Beatmaps
{
    public class CatchBeatmapProcessor : BeatmapProcessor
    {
        public CatchBeatmapProcessor(IBeatmap beatmap)
            : base(beatmap)
        {
        }

        public override void PostProcess()
        {
            applyPositionOffsets();

            initialiseHyperDash((List<CatchHitObject>)Beatmap.HitObjects);

            base.PostProcess();

            int index = 0;
            foreach (var obj in Beatmap.HitObjects.OfType<CatchHitObject>())
            {
                obj.IndexInBeatmap = index++;
                if (obj.LastInCombo && obj.NestedHitObjects.LastOrDefault() is IHasComboInformation lastNested)
                    lastNested.LastInCombo = true;
            }
        }

        public const int RNG_SEED = 1337;

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
            // todo: add difficulty adjust.
            double catcherWidth = (1.0f - 0.7f * (Beatmap.BeatmapInfo.BaseDifficulty.CircleSize - 5) / 5) * 0.62064f;
            double halfCatcherWidth = catcherWidth / 2;
            halfCatcherWidth *= halfCatcherWidth * 0.8;

            int lastDirection = 0;
            double lastExcess = halfCatcherWidth;

            List<CatchHitObject> objectWithDroplets = new List<CatchHitObject>();

            foreach (var currentObject in objects)
            {
                if (currentObject is Fruit)
                    objectWithDroplets.Add(currentObject);

                if (currentObject is JuiceStream)
                {
                    IEnumerator<HitObject> nestedHitObjectsEnumerator = currentObject.NestedHitObjects.GetEnumerator();

                    while (nestedHitObjectsEnumerator.MoveNext())
                    {
                        CatchHitObject objectInJuiceStream = (CatchHitObject)nestedHitObjectsEnumerator.Current;
                        // We don't want TinyDroplets for Hyper Calculating.
                        if (!(objectInJuiceStream is TinyDroplet))
                            objectWithDroplets.Add(objectInJuiceStream);
                    }

                    nestedHitObjectsEnumerator.Dispose();
                }
            }

            int objCount = objectWithDroplets.Count;

            for (int i = 0; i < objCount - 1; i++)
            {
                CatchHitObject currentObject = objectWithDroplets[i];

                CatchHitObject nextObject = objectWithDroplets[i + 1];

                int thisDirection = nextObject.X > currentObject.X ? 1 : -1;
                double timeToNext = nextObject.StartTime - currentObject.StartTime - 4;
                double distanceToNext = Math.Abs(nextObject.X - currentObject.X) - (lastDirection == thisDirection ? lastExcess : halfCatcherWidth);


                if (timeToNext * CatcherArea.Catcher.BASE_SPEED < distanceToNext)
                {
                    currentObject.HyperDashTarget = nextObject;
                    lastExcess = halfCatcherWidth;
                }
                else
                {
                    currentObject.DistanceToHyperDash = (float)(timeToNext * CatcherArea.Catcher.BASE_SPEED - distanceToNext);
                    lastExcess = MathHelper.Clamp(timeToNext * CatcherArea.Catcher.BASE_SPEED - distanceToNext, 0, halfCatcherWidth);
                }

                lastDirection = thisDirection;
            }
        }
    }
}
