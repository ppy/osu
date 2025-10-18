// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit.Changes
{
    public class AddHitObjectChange : IRevertibleChange
    {
        public EditorBeatmap Beatmap;

        public HitObject HitObject;

        public AddHitObjectChange(EditorBeatmap beatmap, HitObject hitObject)
        {
            Beatmap = beatmap;
            HitObject = hitObject;
        }

        public void Apply() => Beatmap.Add(HitObject);

        public void Revert() => Beatmap.Remove(HitObject);
    }
}
