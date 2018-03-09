// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Edit.Layers.Selection
{
    public class HitObjectOverlay : Container
    {
        public readonly DrawableHitObject HitObject;

        public HitObjectOverlay(DrawableHitObject hitObject)
        {
            HitObject = hitObject;
        }
    }
}
