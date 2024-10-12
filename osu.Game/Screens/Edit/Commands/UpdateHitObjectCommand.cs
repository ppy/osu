// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics.CodeAnalysis;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit.Commands
{
    public class UpdateHitObjectCommand : IMergeableCommand
    {
        public EditorBeatmap Beatmap;

        public HitObject HitObject;

        public UpdateHitObjectCommand(EditorBeatmap beatmap, HitObject hitObject)
        {
            Beatmap = beatmap;
            HitObject = hitObject;
        }

        public void Apply() => Beatmap.Update(HitObject);

        public IEditorCommand CreateUndo() => new UpdateHitObjectCommand(Beatmap, HitObject);

        public bool MergeWithPrevious(IEditorCommand previousCommand, [MaybeNullWhen(false)] out IEditorCommand merged)
        {
            // Updates are debounced so we only need one update command in a transaction.
            if (previousCommand is UpdateHitObjectCommand previousUpdateCommand && previousUpdateCommand.HitObject == HitObject)
            {
                merged = this;
                return true;
            }

            merged = null;
            return false;
        }
    }
}
