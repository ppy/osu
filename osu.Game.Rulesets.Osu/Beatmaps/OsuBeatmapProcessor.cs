// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Beatmaps
{
    public class OsuBeatmapProcessor : BeatmapProcessor
    {
        private const int stack_distance = 3;

        public OsuBeatmapProcessor(IBeatmap beatmap)
            : base(beatmap)
        {
        }

        public override void PreProcess()
        {
            IHasComboInformation? lastObj = null;

            // For sanity, ensures that both the first hitobject and the first hitobject after a spinner start a new combo.
            // This is normally enforced by the legacy decoder, but is not enforced by the editor.
            foreach (var obj in Beatmap.HitObjects.OfType<IHasComboInformation>())
            {
                if (obj is not Spinner && (lastObj == null || lastObj is Spinner))
                    obj.NewCombo = true;
                lastObj = obj;
            }

            base.PreProcess();
        }

        public override void PostProcess()
        {
            base.PostProcess();

            ApplyStacking(Beatmap);
        }

        internal static void ApplyStacking(IBeatmap beatmap)
        {
            var hitObjects = beatmap.HitObjects as List<OsuHitObject> ?? beatmap.HitObjects.OfType<OsuHitObject>().ToList();

            if (hitObjects.Count > 0)
            {
                // Reset stacking
                foreach (var h in hitObjects)
                    h.StackHeight = 0;

                if (beatmap.BeatmapVersion >= 6)
                    applyStacking(beatmap, hitObjects, 0, hitObjects.Count - 1);
                else
                    applyStackingOld(beatmap, hitObjects);
            }
        }

        private static void applyStacking(IBeatmap beatmap, List<OsuHitObject> hitObjects, int startIndex, int endIndex)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(startIndex, endIndex);
            ArgumentOutOfRangeException.ThrowIfNegative(startIndex);
            ArgumentOutOfRangeException.ThrowIfNegative(endIndex);

            int extendedEndIndex = endIndex;

            if (endIndex < hitObjects.Count - 1)
            {
                // Extend the end index to include objects they are stacked on
                for (int i = endIndex; i >= startIndex; i--)
                {
                    int stackBaseIndex = i;

                    for (int n = stackBaseIndex + 1; n < hitObjects.Count; n++)
                    {
                        OsuHitObject stackBaseObject = hitObjects[stackBaseIndex];
                        if (stackBaseObject is Spinner) break;

                        OsuHitObject objectN = hitObjects[n];
                        if (objectN is Spinner)
                            continue;

                        double endTime = stackBaseObject.GetEndTime();
                        double stackThreshold = objectN.TimePreempt * beatmap.StackLeniency;

                        if (objectN.StartTime - endTime > stackThreshold)
                            // We are no longer within stacking range of the next object.
                            break;

                        if (Vector2Extensions.Distance(stackBaseObject.Position, objectN.Position) < stack_distance
                            || (stackBaseObject is Slider && Vector2Extensions.Distance(stackBaseObject.EndPosition, objectN.Position) < stack_distance))
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
            }

            // Reverse pass for stack calculation.
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

                double stackThreshold = objectI.TimePreempt * beatmap.StackLeniency;

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

                        double endTime = objectN.GetEndTime();

                        if (objectI.StartTime - endTime > stackThreshold)
                            // We are no longer within stacking range of the previous object.
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
                                // For each object which was declared under this slider, we will offset it to appear *below* the slider end (rather than above).
                                OsuHitObject objectJ = hitObjects[j];
                                if (Vector2Extensions.Distance(objectN.EndPosition, objectJ.Position) < stack_distance)
                                    objectJ.StackHeight -= offset;
                            }

                            // We have hit a slider.  We should restart calculation using this as the new base.
                            // Breaking here will mean that the slider still has StackCount of 0, so will be handled in the i-outer-loop.
                            break;
                        }

                        if (Vector2Extensions.Distance(objectN.Position, objectI.Position) < stack_distance)
                        {
                            // Keep processing as if there are no sliders.  If we come across a slider, this gets cancelled out.
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
                            // We are no longer within stacking range of the previous object.
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

        private static void applyStackingOld(IBeatmap beatmap, List<OsuHitObject> hitObjects)
        {
            for (int i = 0; i < hitObjects.Count; i++)
            {
                OsuHitObject currHitObject = hitObjects[i];

                if (currHitObject.StackHeight != 0 && !(currHitObject is Slider))
                    continue;

                double startTime = currHitObject.GetEndTime();
                int sliderStack = 0;

                for (int j = i + 1; j < hitObjects.Count; j++)
                {
                    double stackThreshold = hitObjects[i].TimePreempt * beatmap.StackLeniency;

                    if (hitObjects[j].StartTime - stackThreshold > startTime)
                        break;

                    // The start position of the hitobject, or the position at the end of the path if the hitobject is a slider
                    Vector2 position2 = currHitObject is Slider currSlider
                        ? currSlider.Position + currSlider.Path.PositionAt(1)
                        : currHitObject.Position;

                    // Note the use of `StartTime` in the code below doesn't match stable's use of `EndTime`.
                    // This is because in the stable implementation, `UpdateCalculations` is not called on the inner-loop hitobject (j)
                    // and therefore it does not have a correct `EndTime`, but instead the default of `EndTime = StartTime`.
                    //
                    // Effects of this can be seen on https://osu.ppy.sh/beatmapsets/243#osu/1146 at sliders around 86647 ms, where
                    // if we use `EndTime` here it would result in unexpected stacking.

                    if (Vector2Extensions.Distance(hitObjects[j].Position, currHitObject.Position) < stack_distance)
                    {
                        currHitObject.StackHeight++;
                        startTime = hitObjects[j].StartTime;
                    }
                    else if (Vector2Extensions.Distance(hitObjects[j].Position, position2) < stack_distance)
                    {
                        // Case for sliders - bump notes down and right, rather than up and left.
                        sliderStack++;
                        hitObjects[j].StackHeight -= sliderStack;
                        startTime = hitObjects[j].StartTime;
                    }
                }
            }
        }
    }
}
