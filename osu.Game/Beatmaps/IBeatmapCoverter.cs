// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Objects;

namespace osu.Game.Beatmaps
{
    public interface IBeatmapConverter<T> where T : HitObject
    {
        Beatmap<T> Convert(Beatmap original);
    }
}
