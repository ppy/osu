// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets.EmptyFreeform.Objects;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.EmptyFreeform.Replays
{
    public class EmptyFreeformAutoGenerator : AutoGenerator<EmptyFreeformReplayFrame>
    {
        public new Beatmap<EmptyFreeformHitObject> Beatmap => (Beatmap<EmptyFreeformHitObject>)base.Beatmap;

        public EmptyFreeformAutoGenerator(IBeatmap beatmap)
            : base(beatmap)
        {
        }

        protected override void GenerateFrames()
        {
            Frames.Add(new EmptyFreeformReplayFrame());

            foreach (EmptyFreeformHitObject hitObject in Beatmap.HitObjects)
            {
                Frames.Add(new EmptyFreeformReplayFrame
                {
                    Time = hitObject.StartTime,
                    Position = hitObject.Position,
                    // todo: add required inputs and extra frames.
                });
            }
        }
    }
}
