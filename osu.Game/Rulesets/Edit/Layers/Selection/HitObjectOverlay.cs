// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Edit.Layers.Selection
{
    public class HitObjectOverlay : CompositeDrawable
    {
        // ReSharper disable once NotAccessedField.Local
        // This will be used later to handle drag movement, etc
        private readonly DrawableHitObject hitObject;

        public HitObjectOverlay(DrawableHitObject hitObject)
        {
            this.hitObject = hitObject;
        }
    }
}
