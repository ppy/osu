// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Mania.Edit.Blueprints;
using osu.Game.Rulesets.Mania.Objects;

namespace osu.Game.Rulesets.Mania.Edit
{
    public class NoteCompositionTool : HitObjectCompositionTool
    {
        public NoteCompositionTool()
            : base(nameof(Note))
        {
        }

        public override PlacementBlueprint CreatePlacementBlueprint() => new NotePlacementBlueprint();
    }
}
