// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit.Screens.Compose
{
    public interface IPlacementHandler
    {
        void BeginPlacement(HitObject hitObject);
        void EndPlacement(HitObject hitObject);

        void Delete(HitObject hitObject);
    }
}
