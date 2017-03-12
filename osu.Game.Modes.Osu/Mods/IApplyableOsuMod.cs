// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Mods;
using osu.Game.Modes.Osu.Objects;
using osu.Game.Modes.UI;

namespace osu.Game.Modes.Osu.Mods
{
    internal interface IApplyableOsuMod : IApplyableMod<HitRenderer<OsuHitObject>>
    {
    }
}
