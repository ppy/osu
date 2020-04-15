// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Replays.Legacy;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Replays.Types;

namespace osu.Game.Rulesets.Mania.Replays
{
    public class ManiaReplayFrame : ReplayFrame, IConvertibleReplayFrame
    {
        public List<ManiaAction> Actions = new List<ManiaAction>();

        public ManiaReplayFrame()
        {
        }

        public ManiaReplayFrame(double time, params ManiaAction[] actions)
            : base(time)
        {
            Actions.AddRange(actions);
        }

        public void FromLegacy(LegacyReplayFrame legacyFrame, IBeatmap beatmap, ReplayFrame lastFrame = null)
        {
            var maniaBeatmap = (ManiaBeatmap)beatmap;

            var normalAction = ManiaAction.Key1;
            var specialAction = ManiaAction.Special1;

            int activeColumns = (int)(legacyFrame.MouseX ?? 0);
            int counter = 0;

            while (activeColumns > 0)
            {
                bool isSpecial = isColumnAtIndexSpecial(maniaBeatmap, counter);

                if ((activeColumns & 1) > 0)
                    Actions.Add(isSpecial ? specialAction : normalAction);

                if (isSpecial)
                    specialAction++;
                else
                    normalAction++;

                counter++;
                activeColumns >>= 1;
            }
        }

        public LegacyReplayFrame ToLegacy(IBeatmap beatmap)
        {
            var maniaBeatmap = (ManiaBeatmap)beatmap;

            int keys = 0;

            foreach (var action in Actions)
            {
                switch (action)
                {
                    case ManiaAction.Special1:
                        keys |= 1 << getSpecialColumnIndex(maniaBeatmap, 0);
                        break;

                    case ManiaAction.Special2:
                        keys |= 1 << getSpecialColumnIndex(maniaBeatmap, 1);
                        break;

                    default:
                        // the index in lazer, which doesn't include special keys.
                        int nonSpecialKeyIndex = action - ManiaAction.Key1;

                        // the index inclusive of special keys.
                        int overallIndex = 0;

                        // iterate to find the index including special keys.
                        for (; overallIndex < maniaBeatmap.TotalColumns; overallIndex++)
                        {
                            // skip over special columns.
                            if (isColumnAtIndexSpecial(maniaBeatmap, overallIndex))
                                continue;
                            // found a non-special column to use.
                            if (nonSpecialKeyIndex == 0)
                                break;
                            // found a non-special column but not ours.
                            nonSpecialKeyIndex--;
                        }

                        keys |= 1 << overallIndex;
                        break;
                }
            }

            return new LegacyReplayFrame(Time, keys, null, ReplayButtonState.None);
        }

        /// <summary>
        /// Find the overall index (across all stages) for a specified special key.
        /// </summary>
        /// <param name="maniaBeatmap">The beatmap.</param>
        /// <param name="specialOffset">The special key offset (0 is S1).</param>
        /// <returns>The overall index for the special column.</returns>
        private int getSpecialColumnIndex(ManiaBeatmap maniaBeatmap, int specialOffset)
        {
            for (int i = 0; i < maniaBeatmap.TotalColumns; i++)
            {
                if (isColumnAtIndexSpecial(maniaBeatmap, i))
                {
                    if (specialOffset == 0)
                        return i;

                    specialOffset--;
                }
            }

            throw new ArgumentException("Special key index is too high.", nameof(specialOffset));
        }

        /// <summary>
        /// Check whether the column at an overall index (across all stages) is a special column.
        /// </summary>
        /// <param name="beatmap">The beatmap.</param>
        /// <param name="index">The overall index to check.</param>
        private bool isColumnAtIndexSpecial(ManiaBeatmap beatmap, int index)
        {
            foreach (var stage in beatmap.Stages)
            {
                if (index >= stage.Columns)
                {
                    index -= stage.Columns;
                    continue;
                }

                return stage.IsSpecialColumn(index);
            }

            throw new ArgumentException("Column index is too high.", nameof(index));
        }
    }
}
