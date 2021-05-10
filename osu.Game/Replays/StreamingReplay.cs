// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Replays
{
    /// <summary>
    /// A <see cref="Replay"/> supporting addition of frames at the end.
    /// </summary>
    public class StreamingReplay : Replay
    {
        public override bool IsComplete => isComplete;

        private bool isComplete;

        /// <summary>
        /// Declare this replay is complete and no more frames will be added to this replay.
        /// </summary>
        public void MarkCompleted() => isComplete = true;

        public sealed override IReadOnlyList<ReplayFrame> Frames => frameList;

        private readonly List<ReplayFrame> frameList = new List<ReplayFrame>();

        /// <summary>
        /// Add a new frame at the end of this replay.
        /// </summary>
        /// <param name="newFrame">The new frame. The <see cref="ReplayFrame.Time"/> of this frame must be later or equals to </param>
        /// <exception cref="InvalidOperationException"></exception>
        public void Add(ReplayFrame newFrame)
        {
            if (IsComplete)
                throw new InvalidOperationException("May not add frames to a completed replay.");

            if (frameList.Count != 0 && !(frameList[^1].Time <= newFrame.Time))
                throw new InvalidOperationException("May not add a new frame to a replay without respecting the frame time ordering.");

            frameList.Add(newFrame);
        }
    }
}
