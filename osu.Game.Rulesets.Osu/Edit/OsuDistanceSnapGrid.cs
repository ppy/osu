// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Rulesets.Osu.Edit
{
    public class OsuDistanceSnapGrid : CircularDistanceSnapGrid
    {
        public OsuDistanceSnapGrid(OsuHitObject hitObject)
            : base(hitObject, hitObject.StackedEndPosition)
        {
            Masking = true;
        }
    }
}
