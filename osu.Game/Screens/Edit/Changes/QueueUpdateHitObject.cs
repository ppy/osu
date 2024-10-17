// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit.Changes
{
    /// <summary>
    /// Queues the update of a <see cref="HitObject"/> in an <see cref="EditorBeatmap"/> for undo/redo.
    /// The order of the updates in the transaction does not matter, because the updates are aggregated and applied on the next frame.
    /// </summary>
    public class QueueUpdateHitObject : IRevertibleChange
    {
        public EditorBeatmap? Beatmap;

        public HitObject HitObject;

        public QueueUpdateHitObject(EditorBeatmap? beatmap, HitObject hitObject)
        {
            Beatmap = beatmap;
            HitObject = hitObject;
        }

        public void Apply() => Beatmap?.Update(HitObject);

        public void Revert() => Beatmap?.Update(HitObject);
    }
}
