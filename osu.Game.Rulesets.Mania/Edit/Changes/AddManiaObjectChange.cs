// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Screens.Edit.Changes;

namespace osu.Game.Rulesets.Mania.Edit.Changes
{
    public class AddManiaObjectChange : IRevertibleChange
    {
        public readonly ManiaPlayfield Playfield;

        public readonly ManiaHitObject HitObject;

        public AddManiaObjectChange(ManiaPlayfield playfield, ManiaHitObject hitObject)
        {
            Playfield = playfield;
            HitObject = hitObject;
        }

        public void Apply() => Playfield.Add(HitObject);

        public void Revert() => Playfield.Remove(HitObject);
    }
}
