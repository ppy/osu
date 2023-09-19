// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;

namespace osu.Game.Tournament.Models
{
    public class SeedingResult
    {
        public List<SeedingBeatmap> Beatmaps = new List<SeedingBeatmap>();

        public Bindable<string> Mod = new Bindable<string>(string.Empty);

        public Bindable<int> Seed = new BindableInt
        {
            MinValue = 1,
            MaxValue = 256
        };
    }
}
