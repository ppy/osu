// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit.Commands;

namespace osu.Game.Rulesets.Osu.Edit.Commands
{
    public class SetNewComboCommand : IEditorCommand
    {
        public OsuHitObject Target;

        public bool NewCombo;

        public SetNewComboCommand(OsuHitObject target, bool newCombo)
        {
            Target = target;
            NewCombo = newCombo;
        }

        public void Apply() => Target.NewCombo = NewCombo;

        public IEditorCommand CreateUndo() => new SetNewComboCommand(Target, Target.NewCombo);

        public bool IsRedundant => NewCombo == Target.NewCombo;
    }
}
