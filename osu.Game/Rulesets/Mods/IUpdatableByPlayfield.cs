// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mods
{
    public interface IUpdatableByPlayfield : IApplicableMod
    {
        void Update(Playfield playfield);
    }
}
