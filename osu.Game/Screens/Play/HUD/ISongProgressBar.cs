// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Screens.Play.HUD
{
    public interface ISongProgressBar
    {
        public Action<double>? OnSeek { get; set; }
        public double StartTime { set; }
        public double EndTime { set; }
        public double CurrentTime { set; }
        public bool Interactive { get; set; }
    }
}
