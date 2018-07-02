// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects.Types;
using OpenTK;
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
            // todo: add difficulty adjust.
            double halfCatcherWidth = CatcherArea.CATCHER_SIZE * (objects.FirstOrDefault()?.Scale ?? 1) / CatchPlayfield.BASE_WIDTH / 2;

            int lastDirection = 0;
            double lastExcess = halfCatcherWidth;

            int objCount = objects.Count;

            for (int i = 0; i < objCount - 1; i++)
            {
                CatchHitObject currentObject = objects[i];

                // not needed?
                // if (currentObject is TinyDroplet) continue;

                CatchHitObject nextObject = objects[i + 1];

                // while (nextObject is TinyDroplet)
                // {
                //     if (++i == objCount - 1) break;
                //     nextObject = objects[i + 1];
                // }

                int thisDirection = nextObject.X > currentObject.X ? 1 : -1;
                double timeToNext = nextObject.StartTime - ((currentObject as IHasEndTime)?.EndTime ?? currentObject.StartTime) - 4;
                double distanceToNext = Math.Abs(nextObject.X - currentObject.X) - (lastDirection == thisDirection ? lastExcess : halfCatcherWidth);

                if (timeToNext * CatcherArea.Catcher.BASE_SPEED < distanceToNext)
                {
                    currentObject.HyperDashTarget = nextObject;
                    lastExcess = halfCatcherWidth;
                }
                else
                {
                    //currentObject.DistanceToHyperDash = timeToNext - distanceToNext;
                    lastExcess = MathHelper.Clamp(timeToNext - distanceToNext, 0, halfCatcherWidth);
                }

                lastDirection = thisDirection;
            }
        }
    }
}
