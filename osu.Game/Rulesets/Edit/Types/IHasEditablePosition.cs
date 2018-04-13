// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Objects.Types;
using OpenTK;

namespace osu.Game.Rulesets.Edit.Types
{
    public interface IHasEditablePosition : IHasPosition
    {
        void OffsetPosition(Vector2 offset);
    }
}
