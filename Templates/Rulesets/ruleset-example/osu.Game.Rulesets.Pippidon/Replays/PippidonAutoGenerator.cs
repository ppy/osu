// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Pippidon.Objects;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Pippidon.Replays
{
    public class PippidonAutoGenerator : AutoGenerator<PippidonReplayFrame>
    {
        public new Beatmap<PippidonHitObject> Beatmap => (Beatmap<PippidonHitObject>)base.Beatmap;

        public PippidonAutoGenerator(IBeatmap beatmap)
            : base(beatmap)
        {
        }

        protected override void GenerateFrames()
        {
            Frames.Add(new PippidonReplayFrame());

            foreach (PippidonHitObject hitObject in Beatmap.HitObjects)
            {
                Frames.Add(new PippidonReplayFrame
                {
                    Time = hitObject.StartTime,
                    Position = hitObject.Position,
                });
            }
        }
    }
}
