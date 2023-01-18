// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Screens.Play.HUD
{
    public interface ISongProgressBar
    {
        /// <summary>
        /// Whether the progress bar should allow interaction, ie. to perform seek operations.
        /// </summary>
        public bool Interactive { get; set; }

        /// <summary>
        /// Action which is invoked when a seek is requested, with the proposed millisecond value for the seek operation.
        /// </summary>
        public Action<double>? OnSeek { get; set; }
    }
}
