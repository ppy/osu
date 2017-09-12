// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;

namespace osu.Game.Rulesets.Mania.Beatmaps
{
    internal class ManiaBeatmap : Beatmap<ManiaHitObject>
    {
        public ManiaBeatmap(Beatmap<ManiaHitObject> original = null)
            : base(original)
        {
        }
    }
}
