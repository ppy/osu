// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Objects;
using OpenTK;

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
            initialiseHyperDash((List<CatchHitObject>)Beatmap.HitObjects);

            base.PostProcess();

            int index = 0;
            foreach (var obj in Beatmap.HitObjects.OfType<CatchHitObject>())
                obj.IndexInBeatmap = index++;
        }

        private void initialiseHyperDash(List<CatchHitObject> objects)
        {
            // todo: add difficulty adjust.
            double catcherWidth = (1.0f - 0.7f * (Beatmap.BeatmapInfo.BaseDifficulty.CircleSize - 5) / 5) * 0.62064f;
            double halfCatcherWidth = catcherWidth / 2;
            //halfCatcherWidth *= halfCatcherWidth * 0.8;

            int lastDirection = 0;
            double lastExcess = halfCatcherWidth;

            List<CatchHitObject> objectWithDroplets = new List<CatchHitObject>();

            for (int i = 0; i < objects.Count; i++)
            {
                CatchHitObject currentObject = objects[i];

                if (currentObject is Fruit)
                    objectWithDroplets.Add(currentObject);

                if (currentObject is JuiceStream)
                {
                    IEnumerator<HitObject> nestedHitObjectsEnumerator = currentObject.NestedHitObjects.GetEnumerator();

                    while (nestedHitObjectsEnumerator.MoveNext())
                    {
                        CatchHitObject objectInJuiceStream = (CatchHitObject)nestedHitObjectsEnumerator.Current;
                        objectWithDroplets.Add(objectInJuiceStream);
                    }
                }
            }

            int objCount = objectWithDroplets.Count;

            for (int i = 0; i < objCount - 1; i++)
            {
                CatchHitObject currentObject = objectWithDroplets[i];

                // not needed?
                if (currentObject is TinyDroplet) continue;

                CatchHitObject nextObject = objectWithDroplets[i + 1];

                while (nextObject is TinyDroplet)
                {
                    if (++i == objCount - 1) break;
                    nextObject = objectWithDroplets[i + 1];
                }

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
                    currentObject.DistanceToHyperDash = timeToNext - distanceToNext;
                    lastExcess = MathHelper.Clamp(timeToNext - distanceToNext, 0, halfCatcherWidth);
                }

                lastDirection = thisDirection;
            }
        }
    }
}
