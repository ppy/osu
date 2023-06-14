// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Online.API
{
    public enum APIRequestCompletionState
    {
        /// <summary>
        /// Not yet run or currently waiting on response.
        /// </summary>
        Waiting,

        /// <summary>
        /// Ran to completion.
        /// </summary>
        Completed,

        /// <summary>
        /// Cancelled or failed due to error.
        /// </summary>
        Failed
    }
}
