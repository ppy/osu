// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
    }
}
