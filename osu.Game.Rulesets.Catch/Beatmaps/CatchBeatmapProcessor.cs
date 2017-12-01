// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
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

            convertHyperDash(beatmap.HitObjects);

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

        private void convertHyperDash(List<CatchHitObject> objects)
        {
            // todo: add difficulty adjust.
            const double catcher_width = CatcherArea.CATCHER_SIZE / CatchPlayfield.BASE_WIDTH;
            const double catcher_width_half = catcher_width / 2;

            int lastDirection = 0;
            double lastExcess = catcher_width_half;

            int objCount = objects.Count;

            for (int i = 0; i < objCount - 1; i++)
            {
                CatchHitObject currentObject = objects[i];

                // not needed?
                if (currentObject is TinyDroplet) continue;

                CatchHitObject nextObject = objects[i + 1];
                while (nextObject is TinyDroplet)
                {
                    if (++i == objCount - 1) break;
                    nextObject = objects[i + 1];
                }

                int thisDirection = nextObject.X > currentObject.X ? 1 : -1;
                double timeToNext = nextObject.StartTime - ((currentObject as IHasEndTime)?.EndTime ?? currentObject.StartTime) - 4;
                double distanceToNext = Math.Abs(nextObject.X - currentObject.X) - (lastDirection == thisDirection ? lastExcess : catcher_width_half);

                if (timeToNext * CatcherArea.Catcher.BASE_SPEED < distanceToNext)
                {
                    currentObject.HyperDash = true;
                    lastExcess = catcher_width_half;
                }
                else
                {
                    //currentObject.DistanceToHyperDash = timeToNext - distanceToNext;
                    lastExcess = MathHelper.Clamp(timeToNext - distanceToNext, 0, catcher_width_half);
                }

                lastDirection = thisDirection;
            }
        }
    }
}
