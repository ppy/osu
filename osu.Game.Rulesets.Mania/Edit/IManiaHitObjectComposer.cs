// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Mania.UI;
using OpenTK;

namespace osu.Game.Rulesets.Mania.Edit
{
    public interface IManiaHitObjectComposer
    {
        Column ColumnAt(Vector2 screenSpacePosition);
    }
}
