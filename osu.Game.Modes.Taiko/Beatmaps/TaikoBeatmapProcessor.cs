// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Modes.Taiko.Objects;

namespace osu.Game.Modes.Taiko.Beatmaps
{
    internal class TaikoBeatmapProcessor : IBeatmapProcessor<TaikoBaseHit>
    {
        public void PostProcess(Beatmap<TaikoBaseHit> beatmap)
        {
        }

        public void SetDefaults(TaikoBaseHit hitObject)
        {
        }
    }
}
