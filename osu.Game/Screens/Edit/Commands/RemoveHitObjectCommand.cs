// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit.Commands
{
    public class RemoveHitObjectCommand : IEditorCommand
    {
        public EditorBeatmap Beatmap;

        public HitObject HitObject;

        public RemoveHitObjectCommand(EditorBeatmap beatmap, HitObject hitObject)
        {
            Beatmap = beatmap;
            HitObject = hitObject;
        }

        public void Apply() => Beatmap.Remove(HitObject);

        public IEditorCommand CreateUndo() => new AddHitObjectCommand(Beatmap, HitObject);
    }
}
