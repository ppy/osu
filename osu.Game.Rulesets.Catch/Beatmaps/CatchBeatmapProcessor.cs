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
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Beatmaps
{
    public class CatchBeatmapProcessor : BeatmapProcessor
    {
        public const int RNG_SEED = 1337;

        public IEnumerable<Mod> mods;

        private float lastStartX;
        private int lastStartTime;

        private FastRandom rng = new FastRandom(RNG_SEED);

        public CatchBeatmapProcessor(IBeatmap beatmap, IEnumerable<Mod> mods)
            : base(beatmap)
        {
            this.mods = mods;
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
                        var firstFruit = (CatchHitObject)juiceStream.NestedHitObjects.FirstOrDefault();
                        lastStartX = juiceStream.X + juiceStream.ControlPoints.LastOrDefault().X / CatchPlayfield.BASE_WIDTH;
                        lastStartTime = (int)firstFruit.StartTime;
                        break;
                    case Fruit fruit:
                        if (mods.OfType<ModHardRock>().Count() == 0) break;

                        var catchObject = (CatchHitObject)fruit;

                        float position = catchObject.X;
                        int startTime = (int)catchObject.StartTime;

                        if (lastStartX == 0)
                        {
                            lastStartX = position;
                            lastStartTime = startTime;
                            break;
                        }

                        float diff = lastStartX - position;
                        int timeDiff = startTime - lastStartTime;

                        if (timeDiff > 1000)
                        {
                            lastStartX = position;
                            lastStartTime = startTime;
                            break;
                        }

                        if (diff == 0)
                        {
                            bool right = rng.NextBool();

                            float rand = Math.Min(20, rng.Next(0, timeDiff / 4)) / CatchPlayfield.BASE_WIDTH;

                            if (right)
                            {
                                if (position + rand <= 1)
                                    position += rand;
                                else
                                    position -= rand;
                            }
                            else
                            {
                                if (position - rand >= 0)
                                    position -= rand;
                                else
                                    position += rand;
                            }

                            catchObject.X = position;

                            break;
                        }

                        if (Math.Abs(diff * CatchPlayfield.BASE_WIDTH) < timeDiff / 3)
                        {
                            if (diff > 0)
                            {
                                if (position - diff > 0)
                                    position -= diff;
                            }
                            else
                            {
                                if (position - diff < 1)
                                    position -= diff;
                            }
                        }

                        catchObject.X = position;

                        lastStartX = position;
                        lastStartTime = startTime;
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
