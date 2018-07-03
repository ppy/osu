// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Beatmaps
{
    internal class OsuBeatmapProcessor : BeatmapProcessor
    {
        public OsuBeatmapProcessor(IBeatmap beatmap)
            : base(beatmap)
        {
        }

        public override void PreProcess()
        {
            base.PreProcess();
            applyStacking((Beatmap<OsuHitObject>)Beatmap);
        }

        private void applyStacking(Beatmap<OsuHitObject> beatmap)
        {
            const int stack_distance = 3;

            // Reset stacking
            for (int i = 0; i <= beatmap.HitObjects.Count - 1; i++)
                beatmap.HitObjects[i].StackHeight = 0;

            // Extend the end index to include objects they are stacked on
            int extendedEndIndex = beatmap.HitObjects.Count - 1;
            for (int i = beatmap.HitObjects.Count - 1; i >= 0; i--)
            {
                int stackBaseIndex = i;
                for (int n = stackBaseIndex + 1; n < beatmap.HitObjects.Count; n++)
                {
                    OsuHitObject stackBaseObject = beatmap.HitObjects[stackBaseIndex];
                    if (stackBaseObject is Spinner) break;

                    OsuHitObject objectN = beatmap.HitObjects[n];
                    if (objectN is Spinner)
                        continue;

                    double endTime = (stackBaseObject as IHasEndTime)?.EndTime ?? stackBaseObject.StartTime;
                    double stackThreshold = objectN.TimePreempt * beatmap.BeatmapInfo.StackLeniency;

                    if (objectN.StartTime - endTime > stackThreshold)
                        //We are no longer within stacking range of the next object.
                        break;

                    if (Vector2Extensions.Distance(stackBaseObject.Position, objectN.Position) < stack_distance ||
                        stackBaseObject is Slider && Vector2Extensions.Distance(stackBaseObject.EndPosition, objectN.Position) < stack_distance)
                    {
                        stackBaseIndex = n;

                        // HitObjects after the specified update range haven't been reset yet
                        objectN.StackHeight = 0;
                    }
                }

                if (stackBaseIndex > extendedEndIndex)
                {
                    extendedEndIndex = stackBaseIndex;
                    if (extendedEndIndex == beatmap.HitObjects.Count - 1)
                        break;
                }
            }

            //Reverse pass for stack calculation.
            int extendedStartIndex = 0;
            for (int i = extendedEndIndex; i > 0; i--)
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

                OsuHitObject objectI = beatmap.HitObjects[i];
                if (objectI.StackHeight != 0 || objectI is Spinner) continue;

                double stackThreshold = objectI.TimePreempt * beatmap.BeatmapInfo.StackLeniency;

                /* If this object is a hitcircle, then we enter this "special" case.
                    * It either ends with a stack of hitcircles only, or a stack of hitcircles that are underneath a slider.
                    * Any other case is handled by the "is Slider" code below this.
                    */
                if (objectI is HitCircle)
                {
                    while (--n >= 0)
                    {
                        OsuHitObject objectN = beatmap.HitObjects[n];
                        if (objectN is Spinner) continue;

                        double endTime = (objectN as IHasEndTime)?.EndTime ?? objectN.StartTime;

                        if (objectI.StartTime - endTime > stackThreshold)
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
                        if (objectN is Slider && Vector2Extensions.Distance(objectN.EndPosition, objectI.Position) < stack_distance)
                        {
                            int offset = objectI.StackHeight - objectN.StackHeight + 1;
                            for (int j = n + 1; j <= i; j++)
                            {
                                //For each object which was declared under this slider, we will offset it to appear *below* the slider end (rather than above).
                                OsuHitObject objectJ = beatmap.HitObjects[j];
                                if (Vector2Extensions.Distance(objectN.EndPosition, objectJ.Position) < stack_distance)
                                    objectJ.StackHeight -= offset;
                            }

                            //We have hit a slider.  We should restart calculation using this as the new base.
                            //Breaking here will mean that the slider still has StackCount of 0, so will be handled in the i-outer-loop.
                            break;
                        }

                        if (Vector2Extensions.Distance(objectN.Position, objectI.Position) < stack_distance)
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
                    while (--n >= 0)
                    {
                        OsuHitObject objectN = beatmap.HitObjects[n];
                        if (objectN is Spinner) continue;

                        if (objectI.StartTime - objectN.StartTime > stackThreshold)
                            //We are no longer within stacking range of the previous object.
                            break;

                        if (Vector2Extensions.Distance(objectN.EndPosition, objectI.Position) < stack_distance)
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
