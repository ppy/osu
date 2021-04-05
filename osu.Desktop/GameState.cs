// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;

namespace osu.Desktop
{
    [Serializable]
    public class GameState
    {
        public string Ruleset { get; set; }

        public string Activity { get; set; }

        public BeatmapInfo Beatmap { get; set; }

        public IEnumerable<string> Mods { get; set; }

        // TODO: Figure out
        // public object Play { get; set; }
    }
}
