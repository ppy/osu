// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Edit
{
    /// <summary>
    /// Visualises a <see cref="EditSnapLine"/>. Although this derives DrawableHitObject,
    /// this does not handle input/sound like a normal hit object.
    /// </summary>
    public abstract class DrawableEditSnapLine : DrawableHitObject<EditSnapLine>
    {
        protected DrawableEditSnapLine(EditSnapLine snapLine)
            : base(snapLine)
        {
        }

        protected override void UpdateState(ArmedState state)
        {
        }
    }
}
