// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Edit
{
    /// <summary>
    /// A mask placed above a <see cref="DrawableHitObject"/> adding editing functionality.
    /// </summary>
    public class HitObjectMask : VisibilityContainer
    {
        public readonly DrawableHitObject HitObject;

        public HitObjectMask(DrawableHitObject hitObject)
        {
            HitObject = hitObject;
            State = Visibility.Hidden;
        }

        protected override void PopIn() => Alpha = 1;
        protected override void PopOut() => Alpha = 0;
    }
}
