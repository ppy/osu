// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Modes.Objects;
using osu.Game.Beatmaps;
using osu.Game.Modes.Osu.Objects.Drawables;
using OpenTK;

namespace osu.Game.Modes.Osu.Objects
{
    public class OsuHitObjectConverter : HitObjectConverter<OsuHitObject>
    {
        public override List<OsuHitObject> Convert(Beatmap beatmap)
        {
            List<OsuHitObject> output = new List<OsuHitObject>();

            int combo = 0;
            foreach (HitObject h in beatmap.HitObjects)
            {
                if (h.NewCombo) combo = 0;

                h.ComboIndex = combo++;
                output.Add(h as OsuHitObject);
            }

            UpdateStacking(output, beatmap.BeatmapInfo?.StackLeniency ?? 0.7f);

            return output;
        }

        public static void UpdateStacking(List<OsuHitObject> hitObjects, float stackLeniency, int startIndex = 0, int endIndex = -1)
        {
            if (endIndex == -1)
                endIndex = hitObjects.Count - 1;

            int stackDistance = 3;
            float stackThreshold = DrawableOsuHitObject.TIME_PREEMPT * stackLeniency;

            // Reset stacking inside the update range
            for (int i = startIndex; i <= endIndex; i++)
                hitObjects[i].StackHeight = 0;

            // Extend the end index to include objects they are stacked on
            int extendedEndIndex = endIndex;
            for (int i = endIndex; i >= startIndex; i--)
            {
                int stackBaseIndex = i;
                for (int n = stackBaseIndex + 1; n < hitObjects.Count; n++)
                {
                    OsuHitObject stackBaseObject = hitObjects[stackBaseIndex];
                    if (stackBaseObject is Spinner) break;

                    OsuHitObject objectN = hitObjects[n];
                    if (objectN is Spinner) continue;

                    if (objectN.StartTime - stackBaseObject.EndTime > stackThreshold)
                        //We are no longer within stacking range of the next object.
                        break;

                    if (Vector2.Distance(stackBaseObject.Position, objectN.Position) < stackDistance ||
                        (stackBaseObject is Slider && Vector2.Distance(stackBaseObject.EndPosition, objectN.Position) < stackDistance))
                    {
                        stackBaseIndex = n;

                        // HitObjects after the specified update range haven't been reset yet
                        objectN.StackHeight = 0;
                    }
                }

                if (stackBaseIndex > extendedEndIndex)
                {
                    extendedEndIndex = stackBaseIndex;
                    if (extendedEndIndex == hitObjects.Count - 1)
                        break;
                }
            }

            //Reverse pass for stack calculation.
            int extendedStartIndex = startIndex;
            for (int i = extendedEndIndex; i > startIndex; i--)
            {
                int n = i;
                /* We should check every note which has not yet got a stack.
                    * Consider the case we have two interwound stacks and this will make sense.
                    *
                    * o <-1      o <-2
                    *  o <-3      o <-4
                    *
                    * We first process starting from 4 and handle 2,
                    * then we come backwards on the i loop iteration until we reach 3 and handle 1.
                    * 2 and 1 will be ignored in the i loop because they already have a stack value.
                    */

                OsuHitObject objectI = hitObjects[i];
                if (objectI.StackHeight != 0 || objectI is Spinner) continue;

                /* If this object is a hitcircle, then we enter this "special" case.
                    * It either ends with a stack of hitcircles only, or a stack of hitcircles that are underneath a slider.
                    * Any other case is handled by the "is Slider" code below this.
                    */
                if (objectI is HitCircle)
                {
                    while (--n >= 0)
                    {
                        OsuHitObject objectN = hitObjects[n];
                        if (objectN is Spinner) continue;

                        if (objectI.StartTime - objectN.EndTime > stackThreshold)
                            //We are no longer within stacking range of the previous object.
                            break;

                        // HitObjects before the specified update range haven't been reset yet
                        if (n < extendedStartIndex)
                        {
                            objectN.StackHeight = 0;
                            extendedStartIndex = n;
                        }

                        /* This is a special case where hticircles are moved DOWN and RIGHT (negative stacking) if they are under the *last* slider in a stacked pattern.
                            *    o==o <- slider is at original location
                            *        o <- hitCircle has stack of -1
                            *         o <- hitCircle has stack of -2
                            */
                        if (objectN is Slider && Vector2.Distance(objectN.EndPosition, objectI.Position) < stackDistance)
                        {
                            int offset = objectI.StackHeight - objectN.StackHeight + 1;
                            for (int j = n + 1; j <= i; j++)
                            {
                                //For each object which was declared under this slider, we will offset it to appear *below* the slider end (rather than above).
                                OsuHitObject objectJ = hitObjects[j];
                                if (Vector2.Distance(objectN.EndPosition, objectJ.Position) < stackDistance)
                                    objectJ.StackHeight -= offset;
                            }

                            //We have hit a slider.  We should restart calculation using this as the new base.
                            //Breaking here will mean that the slider still has StackCount of 0, so will be handled in the i-outer-loop.
                            break;
                        }

                        if (Vector2.Distance(objectN.Position, objectI.Position) < stackDistance)
                        {
                            //Keep processing as if there are no sliders.  If we come across a slider, this gets cancelled out.
                            //NOTE: Sliders with start positions stacking are a special case that is also handled here.

                            objectN.StackHeight = objectI.StackHeight + 1;
                            objectI = objectN;
                        }
                    }
                }
                else if (objectI is Slider)
                {
                    /* We have hit the first slider in a possible stack.
                        * From this point on, we ALWAYS stack positive regardless.
                        */
                    while (--n >= startIndex)
                    {
                        OsuHitObject objectN = hitObjects[n];
                        if (objectN is Spinner) continue;

                        if (objectI.StartTime - objectN.StartTime > stackThreshold)
                            //We are no longer within stacking range of the previous object.
                            break;

                        if (Vector2.Distance(objectN.EndPosition, objectI.Position) < stackDistance)
                        {
                            objectN.StackHeight = objectI.StackHeight + 1;
                            objectI = objectN;
                        }
                    }
                }
            }
        }

    }
}
