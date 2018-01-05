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

namespace osu.Game.Rulesets.Catch.Beatmaps
{
    internal class CatchBeatmapProcessor : BeatmapProcessor<CatchHitObject>
    {
        public override void PostProcess(Beatmap<CatchHitObject> beatmap)
        {
            if (beatmap.ComboColors.Count == 0)
                return;

            int comboIndex = 0;
            int colourIndex = 0;

            CatchHitObject lastObj = null;

            initialiseHyperDash(beatmap.HitObjects);

            foreach (var obj in beatmap.HitObjects)
            {
                if (obj.NewCombo)
                {
                    if (lastObj != null) lastObj.LastInCombo = true;

                    comboIndex = 0;
                    colourIndex = (colourIndex + 1) % beatmap.ComboColors.Count;
                }

                obj.ComboIndex = comboIndex++;
                obj.ComboColour = beatmap.ComboColors[colourIndex];

                lastObj = obj;
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
