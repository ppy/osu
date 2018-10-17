// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit.Screens.Compose
{
    public interface IPlacementHandler
    {
        event Action<HitObject> PlacementStarted;
        event Action<HitObject> PlacementFinished;

        void BeginPlacement(HitObject hitObject);
        void EndPlacement(HitObject hitObject);
    }
}
